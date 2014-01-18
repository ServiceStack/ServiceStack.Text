//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

#if XBOX
using System.IO;

namespace ServiceStack
{
    public class XboxPclExport : PclExport
    {
        public static XboxPclExport Provider = new XboxPclExport();

        public XboxPclExport()
        {
            this.PlatformName = "XBOX";
        }

        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string ReadAllText(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return new StreamReader(fileStream).ReadToEnd();
            }
        }
    }
}
#endif
