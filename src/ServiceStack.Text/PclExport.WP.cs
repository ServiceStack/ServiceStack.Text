//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

#if WP
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;

namespace ServiceStack
{
    public class WpPclExport : PclExport
    {
        public static WpPclExport Provider = new WpPclExport();

        public WpPclExport()
        {
            this.PlatformName = Platforms.WindowsPhone;
        }

        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string ReadAllText(string filePath)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = isoStore.OpenFile(filePath, FileMode.Open))
                {
                    return new StreamReader(fileStream).ReadToEnd();
                }
            }
        }

        public override Assembly LoadAssembly(string assemblyPath)
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        public override string GetAssemblyCodeBase(Assembly assembly)
        {
            return assembly.GetName().CodeBase;
        }

        public override Type GetGenericCollectionType(Type type)
        {
            return type.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }
    }
}
#endif
