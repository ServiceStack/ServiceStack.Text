.assembly extern mscorlib
{
	.publickeytoken = (B7 7A 5C 56 19 34 E0 89)
	.ver 2:0:0:0
}

.assembly target {}

.module target.dll

.class private Test extends [mscorlib]System.Object {

	.method public specialname rtspecialname instance void .ctor () cil managed
	{
		ldarg.0
		call		instance void [mscorlib]System.Object::.ctor ()
		ret
	}

	.field private string str

	.method public instance string ReadField () cil managed
	{
		ldarg.0
		ldfld		string Test::str
		ret
	}

	.method public instance void WriteField (string val) cil managed
	{
		ldarg.0
		ldarg		val
		stfld		string Test::str
		ret
	}

	.method public instance class [mscorlib]System.Type TypeOfString () cil managed
	{
		ldtoken    [mscorlib]System.String
		call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle (valuetype [mscorlib]System.RuntimeTypeHandle)
		ret
	}

	.method public instance int32 ShortBranch (int32 i) cil managed
	{
		ldarg.1
		ldc.i4.0
		ble.s		negative

		ldarg.1
		ret

	negative:
		ldc.i4.m1
		ret
	}

	.method public instance int32 Branch (int32 i) cil managed
	{
		ldarg.1
		ldc.i4.0
		ble			negative

		ldarg.1
		ret

	negative:
		ldc.i4.m1
		ret
	}

	.method public instance int32 Switch (int32 i) cil managed
	{
		ldarg.1
		switch (zero, one, two, other)
		br.s		other

	zero:
		ldc.i4.0
		ret

	one:
		ldc.i4.1
		ret

	two:
		ldc.i4.2
		ret

	other:
		ldc.i4.m1
		ret
	}

	// optimized instance property

	.field private bool opan

	.property instance bool OPan () {
		.get instance bool Test::get_OPan ()
		.set instance void Test::set_OPan (bool)
	}

	.method public hidebysig specialname instance bool get_OPan () cil managed
	{
		ldarg.0
		ldfld		bool Test::opan
		ret
	}

	.method public hidebysig specialname instance void set_OPan (bool val) cil managed
	{
		ldarg.0
		ldarg.1
		stfld		bool Test::opan
		ret
	}

	// optimized static property

	.field private static bool sopan

	.property bool SOPan () {
		.get bool Test::get_SOPan ()
		.set void Test::set_SOPan (bool)
	}

	.method public hidebysig specialname static bool get_SOPan () cil managed
	{
		ldsfld		bool Test::sopan
		ret
	}

	.method public hidebysig specialname static void set_SOPan (bool val) cil managed
	{
		ldarg.0
		stsfld		bool Test::sopan
		ret
	}

	// unoptimized instance property

	.field private bool upan

	.property instance bool UPan () {
		.get instance bool Test::get_UPan ()
		.set instance void Test::set_UPan (bool)
	}

	.method public hidebysig specialname instance bool get_UPan () cil managed
	{
		.locals init (bool b)
		nop
		ldarg.0
		ldfld		bool Test::upan
		stloc.0
		br.s		retval
	retval:
		ldloc.0
		ret
	}

	.method public hidebysig specialname instance void set_UPan (bool val) cil managed
	 {
		nop
		ldarg.0
		ldarg.1
		stfld		bool Test::upan
		ret
	}

	// unoptimized static property

	.field private static bool supan

	.property bool SUPan () {
		.get bool Test::get_SUPan ()
		.set void Test::set_SUPan (bool)
	}

	.method public hidebysig specialname static bool get_SUPan () cil managed
	{
		.locals init (bool val)
		nop
		ldsfld		bool Test::supan
		stloc.0
		br.s		retval
	retval:
		ldloc.0
		ret
	}

	.method public hidebysig specialname static void set_SUPan (bool val) cil managed
	{
		nop
		ldarg.0
		stsfld		bool Test::supan
		ret
	}
}
