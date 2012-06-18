namespace ServiceStack.Text.Dynamic
{
    public static class DynamicExtensions
    {
        public static dynamic ToDynamic(this string input)
        {
            if (input.StartsWith("<"))
                return new DynamicXml(input);
            return new DynamicJson(input);
        }
    }
}