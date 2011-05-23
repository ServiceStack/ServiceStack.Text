using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class AotTests
	{
#if SILVERLIGHT || MONOTOUCH
		[Test]
		public void Can_Register_AOT()
		{
			JsConfig.RegisterForAot();
		}
#endif
	}
}