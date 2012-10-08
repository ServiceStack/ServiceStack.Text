using System;

namespace ServiceStack.Text
{
	public class Tracer
	{
		public static ITracer Instance = new NullTracer();

		public class NullTracer : ITracer
		{
			public void WriteDebug(string error) { }

			public void WriteDebug(string format, params object[] args) { }
		    
            public void WriteWarning(string warning) { }

		    public void WriteWarning(string format, params object[] args) { }

		    public void WriteError(Exception ex) { }

			public void WriteError(string error) { }

			public void WriteError(string format, params object[] args) { }

		}

		public class ConsoleTracer : ITracer
		{
			public void WriteDebug(string error)
			{
				Console.WriteLine(error);
			}

			public void WriteDebug(string format, params object[] args)
			{
				Console.WriteLine(format, args);
			}

		    public void WriteWarning(string warning)
		    {
                Console.WriteLine(warning);                
		    }

		    public void WriteWarning(string format, params object[] args)
		    {
                Console.WriteLine(format, args);
            }

		    public void WriteError(Exception ex)
			{
				Console.WriteLine(ex);
			}

			public void WriteError(string error)
			{
				Console.WriteLine(error);
			}

			public void WriteError(string format, params object[] args)
			{
				Console.WriteLine(format, args);
			}
		}
	}
}