//
// DisassemblerTest.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2009 - 2010 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace Mono.Reflection {

	public abstract class BaseReflectionTest {

		protected static MethodBase GetMethod (string name)
		{
			return test_target.GetType ("Test").GetMember (name,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) [0] as MethodBase;
		}

		protected static PropertyInfo GetProperty (string name)
		{
			return test_target.GetType ("Test").GetProperty (name,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		}

		protected static FieldInfo GetField (string name)
		{
			return test_target.GetType ("Test").GetField (name,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
		}

		static Assembly test_target = LoadTestTarget ();

		static Assembly LoadTestTarget ()
		{
			var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("target.dll");
			return Assembly.Load (ToArray (stream));
		}

		static byte [] ToArray (Stream stream)
		{
			var buffer = new byte [16 * 1024];
			using (MemoryStream ms = new MemoryStream ()) {
				int read;
				while ((read = stream.Read (buffer, 0, buffer.Length)) > 0)
					ms.Write (buffer, 0, read);

				return ms.ToArray ();
			}
		}
	}
}
