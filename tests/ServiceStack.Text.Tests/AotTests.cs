using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class AotTests
	{
#if SL5 || IOS
		[Test]
		public void Can_Register_AOT()
		{
			JsConfig.RegisterForAot();
		}
#endif
	}
}