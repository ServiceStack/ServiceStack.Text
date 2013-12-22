//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

#if XBOX
using System.IO;

namespace ServiceStack
{
    public class XboxPclExport : PclExport
    {
        public new static XboxPclExport Instance = new XboxPclExport();

        public XboxPclExport()
        {
            this.PlatformName = "XBOX";
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
