using System;
using System.Globalization;
using System.Text;

namespace ServiceStack.Text.Json
{
    public static class JsonEscaper
    {
        public static string Unescape(string input)
        {
            var length = input.Length;
            int start = 0;
            int count = 0; 
            StringBuilder output = new StringBuilder(length);
            for ( ; count < length; )
            {
                if (input[count] == JsonUtils.QuoteChar)
                {
                    if (start != count)
                    {
                        output.Append(input, start, count - start);
                    }                    
                    count++;
                    start = count;
                    continue;
                }

                if (input[count] == JsonUtils.EscapeChar)
                {
                    if (start != count)
                    {
                        output.Append(input, start, count - start);
                    }
                    start = count;
                    count++;
                    if (count >= length) continue;

                    //we will always be parsing an escaped char here
                    var c = input[count];

                    switch (c)
                    {
                        case 'a':
                            output.Append('\a');
                            count++;
                            break;
                        case 'b':
                            output.Append('\b');
                            count++;
                            break;
                        case 'f':
                            output.Append('\f');
                            count++;
                            break;
                        case 'n':
                            output.Append('\n');
                            count++;
                            break;
                        case 'r':
                            output.Append('\r');
                            count++;
                            break;
                        case 'v':
                            output.Append('\v');
                            count++;
                            break;
                        case 't':
                            output.Append('\t');
                            count++;
                            break;
                        case 'u':
                            if (count + 4 < length)
                            {
                                var unicodeString = input.Substring(count+1, 4);
                                var unicodeIntVal = UInt32.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(JsonTypeSerializer.ConvertFromUtf32((int) unicodeIntVal));
                                count += 5;
                            }
                            else
                            {
                                output.Append(c);
                            }
                            break;
                        case 'x':
                            if (count + 4 < length)
                            {
                                var unicodeString = input.Substring(count+1, 4);
                                var unicodeIntVal = UInt32.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(JsonTypeSerializer.ConvertFromUtf32((int) unicodeIntVal));
                                count += 5;
                            }
                            else
                            if (count + 2 < length)
                            {
                                var unicodeString = input.Substring(count+1, 2);
                                var unicodeIntVal = UInt32.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(JsonTypeSerializer.ConvertFromUtf32((int) unicodeIntVal));
                                count += 3;
                            }
                            else
                            {
                                output.Append(input, start, count - start);
                            }
                            break;
                        default:
                            output.Append(c);
                            count++;
                            break;
                    }
                    start = count;
                }
                else
                {
                    count++;
                }
            }
            output.Append(input, start, length - start);
            return output.ToString();
        }
    }
}
