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

		void WriteBuiltIn(TextWriter writer, object value, bool includeType=false);
		void WriteObjectString(TextWriter writer, object value, bool includeType=false);
		void WriteException(TextWriter writer, object value, bool includeType=false);
		void WriteString(TextWriter writer, string value, bool includeType=false);
		void WriteDateTime(TextWriter writer, object oDateTime, bool includeType=false);
		void WriteNullableDateTime(TextWriter writer, object dateTime, bool includeType=false);
		void WriteGuid(TextWriter writer, object oValue, bool includeType=false);
		void WriteNullableGuid(TextWriter writer, object oValue, bool includeType=false);
		void WriteBytes(TextWriter writer, object oByteValue, bool includeType=false);
		void WriteInteger(TextWriter writer, object integerValue, bool includeType=false);
		void WriteBool(TextWriter writer, object boolValue, bool includeType=false);
		void WriteFloat(TextWriter writer, object floatValue, bool includeType=false);
		void WriteDouble(TextWriter writer, object doubleValue, bool includeType=false);
		void WriteDecimal(TextWriter writer, object decimalValue, bool includeType=false);

		//object EncodeMapKey(object value);

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