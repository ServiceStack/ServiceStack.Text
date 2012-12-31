using System.Text;

namespace ServiceStack.Text.Json
{
    public static class StringEscaper
    {
        static int HexDigit(char ch)
        {
            int num1;
            if ((uint)(num1 = (int)ch - 48) <= 9U)
                return num1;
            int num2;
            if ((uint)(num2 = (int)ch - 97) <= 5U)
                return num2 + 10;
            int num3;
            if ((uint)(num3 = (int)ch - 65) <= 5U)
                return num3 + 10;
            else
                return -1;
        }

        public static string Unescape(string input)
        {
            var length = input.Length;

            for (int count = 0; count < length; count++)
            {
                if(input[count] == '\\')
                {
                    var output = new StringBuilder(length);
                    output.Append(input, 0, count);

                    do
                    {
                        count++;

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
                                int d1 = HexDigit(input[count+1]);
                                int d2 = HexDigit(input[count + 2]);
                                int d3 = HexDigit(input[count + 3]);
                                int d4 = HexDigit(input[count + 4]);

                                output.Append((char) ((d1*16*16*16) + (d2*16*16) + (d3*16) + d4));
                                count += 5;
                                break;
                            case 'x':
                                int x1 = HexDigit(input[count + 1]);
                                int x2 = HexDigit(input[count + 2]);

                                output.Append((char)(x1*16 + x2));
                                count += 3;
                                break;
                            default:
                                output.Append(c);
                                count++;
                                break;
                        }
                        
                        var startIndex = count;
                        while (count < length && input[count] != '\\')
                            count++;
                        output.Append(input, startIndex, count - startIndex);

                    } while (count < length);

                    return output.ToString();
                }
            }

            return input;
        }
    }
}