using System;
using System.Collections.Generic;

namespace ServiceStack.Text
{
	public static class CsvConfig<T>
	{
		public static bool OmitHeaders { get; set; }

		private static Dictionary<string, string> customHeadersMap;
		public static Dictionary<string, string> CustomHeadersMap
		{
			get
			{
				return customHeadersMap;
			}
			set
			{
				customHeadersMap = value;
				if (value == null) return;
				CsvWriter<T>.ConfigureCustomHeaders(customHeadersMap);
			}
		}

		public static object CustomHeaders
		{
			set
			{
				if (value == null) return;
				if (value.GetType().IsValueType)
					throw new ArgumentException("CustomHeaders is a ValueType");

				var propertyInfos = value.GetType().GetProperties();
				if (propertyInfos.Length == 0) return;

				customHeadersMap = new Dictionary<string, string>();
				foreach (var pi in propertyInfos)
				{
					var getMethod = pi.GetGetMethod();
					if (getMethod == null) continue;

					var oValue = getMethod.Invoke(value, new object[0]);
					if (oValue == null) continue;
					customHeadersMap[pi.Name] = oValue.ToString();
				}
				CsvWriter<T>.ConfigureCustomHeaders(customHeadersMap);
			}
		}

		public static void Reset()
		{
			OmitHeaders = false;
			CsvWriter<T>.Reset();
		}
	}
}