//
// Formatter.cs
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
using System.Text;

namespace Mono.Reflection {

	public static class Formatter {

		public static string FormatInstruction (Instruction instruction)
		{
			var writer = new StringWriter ();
			WriteInstruction (writer, instruction);
			return writer.ToString ();
		}

		public static string FormatMethodBody (MethodBase method)
		{
			var writer = new StringWriter ();
			WriteMethodBody (writer, method);
			return writer.ToString ();
		}

		public static void WriteMethodBody (TextWriter writer, MethodBase method)
		{
			foreach (var instruction in method.GetInstructions ()) {
				writer.Write ('\t');
				WriteInstruction (writer, instruction);
				writer.WriteLine ();
			}
		}

		public static void WriteInstruction (TextWriter writer, Instruction instruction)
		{
			writer.Write (FormatLabel (instruction.Offset));
			writer.Write (": ");
			writer.Write (instruction.OpCode.Name);
			WriteOperand (writer, instruction);
		}

		static string FormatLabel (int offset)
		{
			string label = "000" + offset.ToString ("x");
			return "IL_" + label.Substring (label.Length - 4);
		}

		static string FormatLabel (Instruction instruction)
		{
			return FormatLabel (instruction.Offset);
		}

		static bool TargetsLocalVariable (OpCode opcode)
		{
			return opcode.Name.Contains ("loc");
		}

		static void WriteOperand (TextWriter writer, Instruction instruction)
		{
			var opcode = instruction.OpCode;
			var operand = instruction.Operand;

			if (opcode.OperandType == OperandType.InlineNone)
				return;

			writer.Write (' ');

			switch (opcode.OperandType) {
			case OperandType.ShortInlineBrTarget:
			case OperandType.InlineBrTarget:
				writer.Write (FormatLabel ((Instruction) operand));
				return;
			case OperandType.InlineSwitch:
				WriteLabelList (writer, (Instruction []) operand);
				return;
			case OperandType.InlineString:
				writer.Write ("\"" + operand.ToString () + "\"");
				return;
			case OperandType.ShortInlineVar:
			case OperandType.InlineVar:
				if (TargetsLocalVariable (opcode)) {
					var local = (LocalVariableInfo) operand;
					writer.Write ("V_{0}", local.LocalIndex);
					return;
				}

				var parameter = (ParameterInfo) operand;
				writer.Write (parameter.Name);
				return;
			case OperandType.InlineTok:
			case OperandType.InlineType:
			case OperandType.InlineMethod:
			case OperandType.InlineField:
				var member = (MemberInfo) operand;
				switch (member.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					WriteMethodReference (writer, (MethodBase) member);
					return;
				case MemberTypes.Field:
					WriteFieldReference (writer, (FieldInfo) member);
					return;
				case MemberTypes.TypeInfo:
				case MemberTypes.NestedType:
					writer.Write (FormatTypeReference ((Type) member));
					return;
				default:
					throw new NotSupportedException ();
				}
			case OperandType.InlineI:
			case OperandType.ShortInlineI:
			case OperandType.InlineR:
			case OperandType.ShortInlineR:
			case OperandType.InlineI8:
				writer.Write (ToInvariantCultureString (operand));
				return;
			default:
				throw new NotSupportedException ();
			}
		}

		static void WriteLabelList (TextWriter writer, Instruction [] targets)
		{
			writer.Write ("(");

			for (int i = 0; i < targets.Length; i++) {
				if (i != 0) writer.Write (", ");
				writer.Write (FormatLabel (targets [i]));
			}

			writer.Write (")");
		}

		static string ToInvariantCultureString (object value)
		{
			IConvertible convertible = value as IConvertible;
			return (null != convertible)
				? convertible.ToString (System.Globalization.CultureInfo.InvariantCulture)
				: value.ToString ();
		}

		static void WriteFieldReference (TextWriter writer, FieldInfo field)
		{
			writer.Write (FormatTypeReference (field.FieldType));
			writer.Write (' ');
			writer.Write (field.DeclaringType.FullName);
			writer.Write ("::");
			writer.Write (field.Name);
		}

		static void WriteMethodReference (TextWriter writer, MethodBase method)
		{
			writer.Write (method is ConstructorInfo ?
				FormatTypeReference (typeof (void)) :
				FormatTypeReference (((MethodInfo) method).ReturnType));

			writer.Write (' ');
			writer.Write (method.DeclaringType.FullName);
			writer.Write ("::");
			writer.Write (method.Name);
			writer.Write ("(");
			var parameters = method.GetParameters ();
			for (int i = 0; i < parameters.Length; ++i) {
				if (i > 0)
					writer.Write (", ");

				writer.Write (FormatTypeReference (parameters [i].ParameterType));
			}
			writer.Write (")");
		}

		static string FormatTypeReference (Type type)
		{
			string name = type.FullName;
			switch (name) {
			case "System.Void": return "void";
			case "System.String": return "string";
			case "System.Int16": return "int16";
			case "System.Int32": return "int32";
			case "System.Long": return "int64";
			case "System.Boolean": return "bool";
			case "System.Single": return "float32";
			case "System.Double": return "float64";
			default: return name;
			}
		}
	}
}
