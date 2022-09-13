//-----------------------------------------------------------------------
// <copyright file="EnumTool.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Hydrogen;

// ReSharper disable CheckNamespace
namespace Tools {

	public static class Enums {
		
		public static string[] GetSerializableNames<T>() where T : struct
			=> GetSerializableNames(typeof(T));


		public static string[] GetSerializableNames(Type enumType) {
			var names = new List<string>();
			Guard.ArgumentNotNull(enumType, nameof(enumType));
			foreach (var field in enumType.GetFields().Where(f => !f.IsSpecialName) ) {
				var enumMemberAttribute = field.GetCustomAttributeOfType<EnumMemberAttribute>(false, false);
				names.Add( enumMemberAttribute != null ? enumMemberAttribute.Value : field.Name);
			}
			return names.ToArray();
		}

		public static string GetSerializableName(Enum @enum) {
			var attributes =  @enum.GetAttributes<EnumMemberAttribute>();
			if (attributes.Any())
				return attributes.First().Value;
			return @enum.ToString();
		}

		public static IEnumerable<T> GetValues<T>() {
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static IEnumerable<int> GetIntValues(Enum @enum) {
			return Enum.GetValues(typeof(@Enum)).Cast<int>().OrderBy(x => x);
		}

		public static bool IsInRange(Type enumType, int value) {
			var values = Enum.GetValues(enumType).Cast<int>().OrderBy(x => x).ToArray();
			return value >= values.First() && value <= values.Last();
		}

		public static bool IsInRange<TEnum>(int value) {
			return IsInRange(typeof(TEnum), value);
		}

		public static IEnumerable<T> GetAttributes<T>(Enum value) where T : Attribute {
			return value.GetType().GetField(value.ToString()).GetCustomAttributesOfType<T>(false);
		}

		public static T GetAttribute<T>(Enum value) where T : Attribute {
			return GetAttributes<T>(value).First();
		}

		public static bool HasAttribute<T>(Enum value) where T : Attribute {
			return GetAttributes<T>(value).Any();
		}

		public static bool HasDescription(Enum value) {
			return GetAttributes<DescriptionAttribute>(value).Any();
		}

		public static IEnumerable<string> GetDescriptions(Enum value) {
			return GetAttributes<DescriptionAttribute>(value).Select(x => x.Description);
		}

		public static string GetDescription(Enum value, string @default = null) => GetDescriptions(value).FirstOrDefault() ?? @default ?? value.ToString();

		public static T GetValueFromDescription<T>(string description) {
			return (T)GetValueFromDescription(typeof(T), description);
		}

		public static object GetValueFromDescription(Type type, string description) {
			if (!type.IsEnum) throw new InvalidOperationException();
			foreach (var field in type.GetFields()) {
				if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
					if (attribute.Description == description)
						return field.GetValue(null);
				} else {
					if (field.Name == description)
						return field.GetValue(null);
				}
			}
			throw new ArgumentException("Not found.", nameof(description));
		}

		public static IEnumerable<object> GetDefaultValues(Enum value) {
			return GetAttributes<DefaultValueAttribute>(value).Select(x => x.Value);
		}

		public static object GetDefaultValue(Enum value) {
			return GetDefaultValues(value).First();
		}


		public static string ToTextForm(Enum enumValue) {
			throw new NotImplementedException();
		}

		public static Enum ToTextForm(string enumValueText) {
			throw new NotImplementedException();
		}

		private static readonly char[] FlagDelimiter = new[] { ',' };

		public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct {
			if (!TryParseEnum(typeof(TEnum), value, false, out var objResult)) {
				result = default;
				return false;
			}
			result = (TEnum)objResult;
			return true;
		}

		public static bool TryParseEnum(Type enumType, string value, bool ignoreValueCase, out object result) {
			Guard.ArgumentNotNull(enumType, nameof(enumType));
			Guard.Argument(enumType.IsEnum, nameof(enumType), "Not an Enum");

			result = default;
			if (string.IsNullOrEmpty(value)) {
				return false;
			}

			if (!enumType.IsEnum)
				throw new ArgumentException(string.Format("Type '{0}' is not an enum", enumType.FullName));

			// Try to parse the value directly 
			if (System.Enum.IsDefined(enumType, value)) {
				result = System.Enum.Parse(enumType, value);
				return true;
			}


			// Get some info on enum
			var enumValues = System.Enum.GetValues(enumType);
			if (enumValues.Length == 0)
				return false;  // probably can't happen as you cant define empty enum?
			var enumTypeCode = Type.GetTypeCode(enumValues.GetValue(0).GetType());



			// Get all the possible names and their value for enum 
			// todo: cache this for efficiency
			var enumInfo = new Dictionary<string, object>(ignoreValueCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
			var enumNames = System.Enum.GetNames(enumType).Zip(Tools.Enums.GetSerializableNames(enumType)).ToArray();
			for (var i = 0; i < enumNames.Length; i++) {
				var enumVal = enumValues.GetValue(i);
				enumInfo.Add(enumNames[i].Item1, enumVal);
				if (enumNames[i].Item1 != enumNames[i].Item2)
					if (!enumInfo.ContainsKey(enumNames[i].Item2))
						enumInfo.Add(enumNames[i].Item2, enumVal);
			}

			// Try to match name manually
			if (enumInfo.TryGetValue(value.Trim(), out result))
				return true;

			// Try to parse it as a flag 
			if (value.IndexOf(',') != -1) {
				if (!Attribute.IsDefined(enumType, typeof(FlagsAttribute)))
					return false;  // value has flags but enum is not flags
			

				ulong retVal = 0;
				foreach (var name in value.Split(FlagDelimiter)) {
					var trimmedName = name.Trim();
					if (!enumInfo.ContainsKey(trimmedName))
						return false;   // Enum has no such flag

					var enumValueObject = enumInfo[trimmedName];
					ulong enumValueLong;
					switch (enumTypeCode) {
						case TypeCode.Byte:
							enumValueLong = (byte)enumValueObject;
							break;
						case TypeCode.SByte:
							enumValueLong = (byte)((sbyte)enumValueObject);
							break;
						case TypeCode.Int16:
							enumValueLong = (ushort)((short)enumValueObject);
							break;
						case TypeCode.Int32:
							enumValueLong = (uint)((int)enumValueObject);
							break;
						case TypeCode.Int64:
							enumValueLong = (ulong)((long)enumValueObject);
							break;
						case TypeCode.UInt16:
							enumValueLong = (ushort)enumValueObject;
							break;
						case TypeCode.UInt32:
							enumValueLong = (uint)enumValueObject;
							break;
						case TypeCode.UInt64:
							enumValueLong = (ulong)enumValueObject;
							break;
						default:
							return false;   // should never happen
					}
					retVal |= enumValueLong;
				}
				result = System.Enum.ToObject(enumType, retVal);
				return true;
			}

			// the value may be a number, so parse it directly
			switch (enumTypeCode) {
				case TypeCode.SByte:
					sbyte sb;
					if (!SByte.TryParse(value, out sb))
						return false;
					result = System.Enum.ToObject(enumType, sb);
					break;
				case TypeCode.Byte:
					byte b;
					if (!Byte.TryParse(value, out b))
						return false;
					result = System.Enum.ToObject(enumType, b);
					break;
				case TypeCode.Int16:
					short i16;
					if (!Int16.TryParse(value, out i16))
						return false;
					result = System.Enum.ToObject(enumType, i16);
					break;
				case TypeCode.UInt16:
					ushort u16;
					if (!UInt16.TryParse(value, out u16))
						return false;
					result = System.Enum.ToObject(enumType, u16);
					break;
				case TypeCode.Int32:
					int i32;
					if (!Int32.TryParse(value, out i32))
						return false;
					result = System.Enum.ToObject(enumType, i32);
					break;
				case TypeCode.UInt32:
					uint u32;
					if (!UInt32.TryParse(value, out u32))
						return false;
					result = System.Enum.ToObject(enumType, u32);
					break;
				case TypeCode.Int64:
					long i64;
					if (!Int64.TryParse(value, out i64))
						return false;
					result = System.Enum.ToObject(enumType, i64);
					break;
				case TypeCode.UInt64:
					ulong u64;
					if (!UInt64.TryParse(value, out u64))
						return false;
					result = System.Enum.ToObject(enumType, u64);
					break;
				default:
					return false; // should never happen
			}

			return true;
		}
	}
}

