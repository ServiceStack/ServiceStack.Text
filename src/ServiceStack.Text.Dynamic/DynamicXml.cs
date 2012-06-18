using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace ServiceStack.Text.Dynamic
{
    public class DynamicXml : DynamicObject
    {
        readonly XElement xElement;

        public DynamicXml(string xml) : this(XElement.Parse(xml)) { }

        DynamicXml(XElement xElement)
        {
            this.xElement = xElement;
            foreach (var xAttribute in xElement.Attributes())
            {
                xElement.Add(new XElement(xAttribute.Name, xAttribute.Value));
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (xElement.Elements().All(x => x.Name.LocalName != binder.Name) && binder.Name != xElement.Name.LocalName)
                return false;

            var xElements = xElement.Elements().Where(x => x.Name.LocalName == binder.Name);
            if (xElements.Count() == 1)
                result = new DynamicXml(xElements.First());
            else if (!xElements.Any() && binder.Name == xElement.Name.LocalName)
            {
                result = this;
            }
            else
                result = xElements.Select(x => new DynamicXml(x)).ToArray();
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = TypeSerializer.DeserializeFromString(xElement.Value, binder.ReturnType);
            return true;
        }

        public override string ToString()
        {
            return xElement.Value;
        }
    }
}
