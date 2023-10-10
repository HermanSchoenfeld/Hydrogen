﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Hydrogen;

public class CompositeSerializer<TItem> : ItemSerializer<TItem> {

	public EventHandlerEx<TItem> ItemActivated;

	private readonly Func<TItem> _activator;
	private readonly MemberSerializationBinding[] _memberBindings;
	private readonly bool _isConstantSize;
	private readonly long _constantSize;

	public CompositeSerializer(Func<TItem> activator, MemberSerializationBinding[] memberBindings) 
		: base(SizeDescriptorStrategy.UseCVarInt) {
		Guard.ArgumentNotNull(activator, nameof(activator));
		Guard.ArgumentNotNull(memberBindings, nameof(memberBindings));
		_activator = activator;
		_memberBindings = memberBindings;
		_isConstantSize = _memberBindings.All(x => x.Serializer.IsConstantSize);
		_constantSize =  IsConstantSize ? _memberBindings.Sum(x => x.Serializer.ConstantSize) : -1;
	}

	internal CompositeSerializer(Func<object> activator, MemberSerializationBinding[] memberBindings) 
		: this( () => (TItem)activator(), memberBindings ) {
	}

	internal void ConfigureLatebound(Func<object> activator, MemberSerializationBinding[] memberBindings)  {
		throw new NotImplementedException();
	}

	public override bool SupportsNull => false;

	public override bool IsConstantSize => _isConstantSize;

	public override long ConstantSize => _constantSize;

	public override long CalculateTotalSize(IEnumerable<TItem> items, bool calculateIndividualItems, out long[] itemSizes) {
		var itemsArr = items as TItem[] ?? items.ToArray();
		itemSizes = null;
		var totalSize = 0L;
		foreach(var (member, serializer) in _memberBindings) {
			var itemMemberValues = itemsArr.Select(item => member.GetValue(item)).ToArray();
			var itemMemberSizes = itemMemberValues.Select(serializer.CalculateSize).ToArray();
			totalSize += itemMemberSizes.Sum();

			if (calculateIndividualItems) {
				if (itemSizes == null) {
					itemSizes = itemMemberSizes;
				} else {
					Guard.Ensure(itemMemberSizes.Length == itemSizes.Length, "Member item size arrays were inconsistent (serializer bug).");
					for (var i = 0; i < itemSizes.Length; i++) {
						itemSizes[i] += itemMemberSizes[i];
					}
				}
			}
		}
		return totalSize;
	}

	public override long CalculateSize(SerializationContext context, TItem item) {
		var size = 0L;
		foreach(var binding in _memberBindings) {
			var memberValue = binding.Member.GetValue(item);
			var memberSerializer = binding.Serializer;
			var itemSize = memberSerializer.CalculateSize(context, memberValue);
			size += itemSize;
		}
		return size;
	}
		
	public override void Serialize(TItem item, EndianBinaryWriter writer, SerializationContext context) {
		foreach (var binding in _memberBindings) {
			var memberValue = binding.Member.GetValue(item);
			binding.Serializer.Serialize(memberValue, writer, context);
		}
	}

	public override TItem Deserialize(EndianBinaryReader reader, SerializationContext context) {
		var item = _activator();
		context.SetDeserializingItem(item);
		foreach (var binding in _memberBindings) {
			var memberValue = binding.Serializer.Deserialize(reader, context);
			binding.Member.SetValue(item, memberValue);
		}
		return item;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void NotifyActivated(TItem item) {
		ItemActivated?.Invoke(item);
	}
	

	//private SerializationScope EnterCompositeScope(TItem item, SerializationTask serializationTask, out long index) {
	//	// Due to how the serializer factory works, if this is the root-serializer, we need to initiate
	//	// a SerializationScope so that cyclic references to root object can be detected. If it's not
	//	// a root-level serializer, then the CyclicReferenceAwareSerializer wrapper will take care of everything.
	//	index = -1;
	//	var scope = new SerializationScope();
	//	if (scope.IsRootScope) {
	//		switch(serializationTask) {
	//			case SerializationTask.Sizing:
	//				scope.NotifySerializingObject(item, true);
	//				break;
	//			case SerializationTask.Serializing:
	//				scope.NotifySerializingObject(item, false);
	//				break;
	//			case SerializationTask.Deserializing:
	//				scope.NotifyDeserializingObject(out index);
	//				break;
	//		}
	//	}
	//	return scope;

	//}

	//private enum SerializationTask  {
	//	Sizing,
	//	Serializing,
	//	Deserializing
	//}
}
