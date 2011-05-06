using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class AotTests
	{
		[Test]
		public void Can_Register_AOT()
		{
			JsConfig.RegisterForAot<Movie>();
		}
	}
}