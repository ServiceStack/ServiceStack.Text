using System;
using System.IO;

namespace ServiceStack.Text.Common
{
	internal interface ITypeSerializer
	{
		WriteObjectDelegate GetWriteFn<T>();
		WriteObjectDelegate GetWriteFn(Type type);

		void WriteRawString(TextWriter writer, string value);
		void WritePropertyName(TextWriter writer, string value);

		void WriteBuiltIn(TextWriter writer, object value, bool includeType);
		void WriteObjectString(TextWriter writer, object value, bool includeType);
		void WriteException(TextWriter writer, object value, bool includeType);
		void WriteString(TextWriter writer, string value, bool includeType);
		void WriteDateTime(TextWriter writer, object oDateTime, bool includeType);
		void WriteNullableDateTime(TextWriter writer, object dateTime, bool includeType);
		void WriteGuid(TextWriter writer, object oValue, bool includeType);
		void WriteNullableGuid(TextWriter writer, object oValue, bool includeType);
		void WriteBytes(TextWriter writer, object oByteValue);
		void WriteInteger(TextWriter writer, object integerValue, bool includeType);
		void WriteBool(TextWriter writer, object boolValue, bool includeType);
		void WriteFloat(TextWriter writer, object floatValue, bool includeType);
		void WriteDouble(TextWriter writer, object doubleValue, bool includeType);
		void WriteDecimal(TextWriter writer, object decimalValue, bool includeType);

		ParseStringDelegate GetParseFn<T>();
		ParseStringDelegate GetParseFn(Type type);

		string ParseRawString(string value);
		string ParseString(string value);
		string EatTypeValue(string value, ref int i);
		bool EatMapStartChar(string value, ref int i);
		string EatMapKey(string value, ref int i);
		bool EatMapKeySeperator(string value, ref int i);
		string EatValue(string value, ref int i);
		bool EatItemSeperatorOrMapEndChar(string value, ref int i);
	}
}