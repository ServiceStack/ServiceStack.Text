//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2011 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Globalization;
using System.IO;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
	internal class JsvTypeSerializer 
		: ITypeSerializer
	{
		public static ITypeSerializer Instance = new JsvTypeSerializer();

		public WriteObjectDelegate GetWriteFn<T>()
		{
			return JsvWriter<T>.WriteFn();
		}

		public WriteObjectDelegate GetWriteFn(Type type)
		{
			return JsvWriter.GetWriteFn(type);
		}

		public void WriteRawString(TextWriter writer, string value)
		{
			writer.Write(value.ToCsvField());
		}

		public void WritePropertyName(TextWriter writer, string value)
		{
			writer.Write(value);
		}

		public void WriteBuiltIn(TextWriter writer, object value, bool includeType=false)
		{
			writer.Write(value);
		}

		public void WriteObjectString(TextWriter writer, object value, bool includeType=false)
		{
			if (value != null)
			{
				writer.Write(value.ToString().ToCsvField());
			}
		}

		public void WriteException(TextWriter writer, object value, bool includeType=false)
		{
			writer.Write(((Exception)value).Message.ToCsvField());
		}

		public void WriteString(TextWriter writer, string value, bool includeType=false)
		{
			writer.Write(value.ToCsvField());
		}

		public void WriteDateTime(TextWriter writer, object oDateTime, bool includeType=false)
		{
			writer.Write(DateTimeSerializer.ToShortestXsdDateTimeString((DateTime)oDateTime));
		}

		public void WriteNullableDateTime(TextWriter writer, object dateTime, bool includeType=false)
		{
			if (dateTime == null) return;
			writer.Write(DateTimeSerializer.ToShortestXsdDateTimeString((DateTime)dateTime));
		}

		public void WriteGuid(TextWriter writer, object oValue, bool includeType=false)
		{
			writer.Write(((Guid)oValue).ToString("N"));
		}

		public void WriteNullableGuid(TextWriter writer, object oValue, bool includeType=false)
		{
			if (oValue == null) return;
			writer.Write(((Guid)oValue).ToString("N"));
		}

		public void WriteBytes(TextWriter writer, object oByteValue, bool includeType=false)
		{
			if (oByteValue == null) return;
			writer.Write(Convert.ToBase64String((byte[])oByteValue));
		}

		public void WriteInteger(TextWriter writer, object integerValue, bool includeType=false)
		{
			if (integerValue == null) return;
			writer.Write(integerValue.ToString());
		}

		public void WriteBool(TextWriter writer, object boolValue, bool includeType=false)
		{
			if (boolValue == null) return;
			writer.Write(boolValue.ToString());
		}

		public void WriteFloat(TextWriter writer, object floatValue, bool includeType=false)
		{
			if (floatValue == null) return;
			writer.Write(((float)floatValue).ToString(CultureInfo.InvariantCulture));
		}

		public void WriteDouble(TextWriter writer, object doubleValue, bool includeType=false)
		{
			if (doubleValue == null) return;
			writer.Write(((double)doubleValue).ToString(CultureInfo.InvariantCulture));
		}

		public void WriteDecimal(TextWriter writer, object decimalValue, bool includeType=false)
		{
			if (decimalValue == null) return;
			writer.Write(((decimal)decimalValue).ToString(CultureInfo.InvariantCulture));
		}


		public object EncodeMapKey(object value)
		{
			return value;
		}

		public ParseStringDelegate GetParseFn<T>()
		{
			return JsvReader.Instance.GetParseFn<T>();
		}

		public ParseStringDelegate GetParseFn(Type type)
		{
			return JsvReader.GetParseFn(type);
		}

		public string ParseRawString(string value)
		{
			return value;
		}

		public string ParseString(string value)
		{
			return value.FromCsvField();
		}

		public string EatTypeValue(string value, ref int i)
		{
			return EatValue(value, ref i);
		}

		public bool EatMapStartChar(string value, ref int i)
		{
			var success = value[i] == JsWriter.MapStartChar;
			if (success) i++;
			return success;
		}

		public string EatMapKey(string value, ref int i)
		{
			var tokenStartPos = i;

			var valueLength = value.Length;

			var valueChar = value[tokenStartPos];
			if (valueChar == JsWriter.QuoteChar)
			{
				while (++i < valueLength)
				{
					valueChar = value[i];

					if (valueChar != JsWriter.QuoteChar) continue;

					var isLiteralQuote = i + 1 < valueLength && value[i + 1] == JsWriter.QuoteChar;

					i++; //skip quote
					if (!isLiteralQuote)
						break;
				}
				return value.Substring(tokenStartPos, i - tokenStartPos);
			}

			//Is Type/Map, i.e. {...}
			if (valueChar == JsWriter.MapStartChar)
			{
				var endsToEat = 1;
				var withinQuotes = false;
				while (++i < valueLength && endsToEat > 0)
				{
					valueChar = value[i];

					if (valueChar == JsWriter.QuoteChar)
						withinQuotes = !withinQuotes;

					if (withinQuotes)
						continue;

					if (valueChar == JsWriter.MapStartChar)
						endsToEat++;

					if (valueChar == JsWriter.MapEndChar)
						endsToEat--;
				}
				return value.Substring(tokenStartPos, i - tokenStartPos);
			}

			while (value[++i] != JsWriter.MapKeySeperator) { }
			return value.Substring(tokenStartPos, i - tokenStartPos);
		}

		public bool EatMapKeySeperator(string value, ref int i)
		{
			return value[i++] == JsWriter.MapKeySeperator;
		}

		public bool EatItemSeperatorOrMapEndChar(string value, ref int i)
		{
			if (i == value.Length) return false;

			var success = value[i] == JsWriter.ItemSeperator
				|| value[i] == JsWriter.MapEndChar;
			i++;
			return success;
		}

		public string EatValue(string value, ref int i)
		{
			var tokenStartPos = i;
			var valueLength = value.Length;
			if (i == valueLength) return null;

			var valueChar = value[i];

			//If we are at the end, return.
			if (valueChar == JsWriter.ItemSeperator
				|| valueChar == JsWriter.MapEndChar)
			{
				return null;
			}

			//Is List, i.e. [...]
			var withinQuotes = false;
			if (valueChar == JsWriter.ListStartChar)
			{
				var endsToEat = 1;
				while (++i < valueLength && endsToEat > 0)
				{
					valueChar = value[i];

					if (valueChar == JsWriter.QuoteChar)
						withinQuotes = !withinQuotes;

					if (withinQuotes)
						continue;

					if (valueChar == JsWriter.ListStartChar)
						endsToEat++;

					if (valueChar == JsWriter.ListEndChar)
						endsToEat--;
				}
				return value.Substring(tokenStartPos, i - tokenStartPos);
			}

			//Is Type/Map, i.e. {...}
			if (valueChar == JsWriter.MapStartChar)
			{
				var endsToEat = 1;
				while (++i < valueLength && endsToEat > 0)
				{
					valueChar = value[i];

					if (valueChar == JsWriter.QuoteChar)
						withinQuotes = !withinQuotes;

					if (withinQuotes)
						continue;

					if (valueChar == JsWriter.MapStartChar)
						endsToEat++;

					if (valueChar == JsWriter.MapEndChar)
						endsToEat--;
				}
				return value.Substring(tokenStartPos, i - tokenStartPos);
			}


			//Is Within Quotes, i.e. "..."
			if (valueChar == JsWriter.QuoteChar)
			{
				while (++i < valueLength)
				{
					valueChar = value[i];

					if (valueChar != JsWriter.QuoteChar) continue;

					var isLiteralQuote = i + 1 < valueLength && value[i + 1] == JsWriter.QuoteChar;

					i++; //skip quote
					if (!isLiteralQuote)
						break;
				}
				return value.Substring(tokenStartPos, i - tokenStartPos);
			}

			//Is Value
			while (++i < valueLength)
			{
				valueChar = value[i];

				if (valueChar == JsWriter.ItemSeperator
					|| valueChar == JsWriter.MapEndChar)
				{
					break;
				}
			}

			return value.Substring(tokenStartPos, i - tokenStartPos);
		}
	}
}