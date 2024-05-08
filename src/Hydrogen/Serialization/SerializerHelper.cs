﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Hydrogen.Mapping;

namespace Hydrogen;

internal static class SerializerHelper {
	public static Member[] GetSerializableMembers(Type type) {
		var inheritanceDepth = type.Visit(x => x.BaseType, x => x is not null).ToList();
		return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
			.Where(x => x is PropertyInfo || x is FieldInfo)
			.Select(x => x.ToMember())
			.Where(x => x.CanRead && x.CanWrite)
			.Where(x => !x.MemberInfo.HasAttribute<TransientAttribute>(false))
			.OrderByDescending(x => inheritanceDepth.IndexOf(x.DeclaringType))  // this order to ensure base-type members are serialized before sub-type members
			.ToArray();
	}


	public static IItemSerializer AssembleSerializer(SerializerFactory serializerFactory, Type itemType, bool retainRegisteredTypesInFactory, long typeCodeStart) {

		// During the construction, a factory is required to store generated serializers.
		var factoryToUse = retainRegisteredTypesInFactory ? serializerFactory : new SerializerFactory(serializerFactory) { MinimumGeneratedTypeCode = typeCodeStart };

		var assembledSerializer = AssembleRecursively(factoryToUse, itemType);

		return assembledSerializer;

		IItemSerializer AssembleRecursively(SerializerFactory factory, Type itemType) {

			// Ensure serializers for member types are registered
			// (i.e. resolving serializer for List<UnregisteredType> serializer requires a serializer for UnregisteredType)
			foreach (var unregisteredMemberType in GetUnregisteredMemberTypes(factory, itemType))
				AssembleRecursively(factory, unregisteredMemberType);

			// If serializer already exists for this type in factory, use that
			if (factory.HasSerializer(itemType)) {
				var typeSerializer = factory.GetCachedSerializer(itemType);
				// Wrap it in a ReferenceSerializer if it doesn't support null values
				if (!itemType.IsValueType && !typeSerializer.SupportsNull) {
					return typeSerializer.AsReferenceSerializer(ReferenceSerializerMode.Default);
				}
				return typeSerializer;
			}

			// Special Case: if we're serializing an enum (or nullable enum), we register it with the factory now and return
			if (itemType.IsEnum || itemType.IsConstructedGenericTypeOf(typeof(Nullable<>)) && itemType.GenericTypeArguments[0].IsEnum) {
				factory.RegisterEnum(itemType.IsEnum ? itemType : itemType.GenericTypeArguments[0]);
				return factory.GetCachedSerializer(itemType);
			}

			// No serializer registered so we need to assemble one as a CompositeSerializer. First, we need to 
			// register the serializer (before it is assembled) so that it may recursively refer to itself. So we 
			// activate a CompositeSerializer with no members (we'll configure it later)
			var compositeSerializer = CreateCompositeSerializer(itemType);

			// pre-register the composite serializer instance now for recursive use by member's
			if (itemType != typeof(object))
				factory.RegisterInternal(factory.GenerateTypeCode(), itemType, compositeSerializer.GetType(), compositeSerializer, null);

			// Prepare serializers for all members
			var members = GetSerializableMembers(itemType);
			var memberBindings = new List<MemberSerializationBinding>(members.Length);
			foreach (var member in members) {

				// Ensure we have a serializer for the member type
				if (member.PropertyType != typeof(object) && !factory.HasSerializer(member.PropertyType))
					AssembleRecursively(factory, member.PropertyType);

				Guard.Ensure(factory.HasSerializer(member.PropertyType), "Failed to assemble serializer for member type");

				// Get any specified reference annotation for this member
				var referenceMode = 
					member.MemberInfo.TryGetCustomAttributeOfType<ReferenceModeAttribute>(false, out var attr) ? 
						attr.Mode : 
						ReferenceSerializerMode.Default;

				// Ensure value types have no such annotations (since they can never be references)
				Guard.Against(attr is not null && member.PropertyType.IsValueType, $"Member {member.DeclaringType.ToStringCS()}.{member.Name} has incorrectly specified a {nameof(ReferenceModeAttribute)} for a value type {member.PropertyType.ToStringCS()}");

				// If member type is sealed, we can use a serializer for that type, otherwise we need a polymorphic serializer for that type.
				// NOTE: these may recursively call AssembleRecursively to ensure all types are registered
				var memberSerializer = 
					member.PropertyType.IsSealed ? 
					factory.GetSerializer(member.PropertyType).AsDereferencedSerializer() : 
					CreatePolymorphicSerializer(factory, member.PropertyType);

				// if member type is a reference type, we need to wrap the serializer as a reference serializer
				if (!member.PropertyType.IsValueType)
					memberSerializer = memberSerializer.AsReferenceSerializer(referenceMode);

				memberBindings.Add(new(member, memberSerializer));
			}
			ConfigureCompositeSerializer(compositeSerializer, itemType, memberBindings);
			
			// return the composite serializer (wrapped in a ReferenceSerializer if necessary)
			return itemType.IsValueType ? compositeSerializer : compositeSerializer.AsReferenceSerializer();
		}

		IEnumerable<Type> GetUnregisteredMemberTypes(SerializerFactory factory, Type type, HashSet<Type> alreadyVisited = null) {
			alreadyVisited ??= new HashSet<Type>();

			// List<Type>
			// Type[]
			// Type1<Type2, Type3>

			// Avoid recursive loops
			if (alreadyVisited.Contains(type))
				yield break;
			alreadyVisited.Add(type);

			// Case 1: There is an explicit serializer for this type, no component types need to be assembled
			if (factory.HasSerializer(type))
				yield break;


			// Case 2: Array element type may need assembling
			if (type.IsArray) {
				var elementType = type.GetElementType();
				if (!factory.HasSerializer(elementType)) {
					foreach (var elementTypeUnregisteredComponentTypes in GetUnregisteredMemberTypes(factory, elementType, alreadyVisited))
						yield return elementTypeUnregisteredComponentTypes;
					yield return elementType;
				}
			}

			// Case 4: Serializer for generic type definition exists but not for generic type arguments 
			// e.g. List<UnregType>, Dictionary<UnregType1, UnregType2>, etc
			if (type.IsConstructedGenericType && factory.HasSerializer(type.GetGenericTypeDefinition())) {
				foreach (var genericArgumentType in type.GetGenericArguments().Where(x => !factory.HasSerializer(x))) {
					foreach (var subType in GetUnregisteredMemberTypes(factory, genericArgumentType, alreadyVisited))
						yield return subType;
					yield return genericArgumentType;
				}
			}
		}
	}

	public static IItemSerializer CreateCompositeSerializer(Type itemType) 
		=> (IItemSerializer)typeof(CompositeSerializer<>)
			.MakeGenericType(itemType)
			.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null)
			.Invoke(null);

	public static void ConfigureCompositeSerializer(IItemSerializer serializer, Type itemType, IEnumerable<MemberSerializationBinding> memberBindings) {
		Guard.Ensure(serializer.GetType().IsConstructedGenericTypeOf(typeof(CompositeSerializer<>)), "Serializer must be a CompositeSerializer");
		serializer
			.GetType()
			.GetMethod(nameof(CompositeSerializer<object>.Configure), BindingFlags.Instance | BindingFlags.NonPublic)
			.Invoke(serializer, [ Tools.Lambda.CastFunc( () => itemType.ActivateWithCompatibleArgs(), itemType), memberBindings.ToArray() ]);
	}

	public static IItemSerializer CreatePolymorphicSerializer(SerializerFactory factory, Type itemType) 
		=> (IItemSerializer)typeof(PolymorphicSerializer<>)
			.MakeGenericType(itemType)
			.ActivateWithCompatibleArgs(factory);
}
