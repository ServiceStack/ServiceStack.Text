#if NETCORE2_1

using System;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack 
{
    public class NetCorePclExport : NetStandardPclExport
    {
        public NetCorePclExport()
        {
            this.PlatformName = Platforms.NetCore;
            ReflectionOptimizer.Instance = EmitReflectionOptimizer.Provider;            
        }

        public override ParseStringDelegate GetJsReaderParseMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.Parse;
            }

            return null;
        }

        public override ParseStringSpanDelegate GetJsReaderParseStringSpanMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.ParseStringSpan;
            }
            
            return null;
        }
    }
}

#endif