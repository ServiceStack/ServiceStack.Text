using System;

namespace ServiceStack.Text
{
    public interface ICustomTypeConverter
    {
        bool CanConvert(object from, Type toType);
        object ConvertFrom(object from);
    }
}