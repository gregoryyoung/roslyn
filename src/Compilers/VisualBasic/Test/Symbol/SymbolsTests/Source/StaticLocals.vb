﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Globalization
Imports System.Text
Imports System.Xml.Linq
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.UnitTests.Symbols
Imports Roslyn.Test.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests

    Public Class StaticLocals
        Inherits BasicTestBase

        <Fact>
        Public Sub FromSpec()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Imports System

Module Test
    Sub F()
        Static x As Integer = 5

        Console.WriteLine("Static variable x = " & x)
        x += 1
    End Sub

    Sub Main()
        Dim i As Integer

        For i = 1 to 3
            F()
        Next i

        i = 3
label:
        Dim y As Integer = 8

        If i > 0 Then
            Console.WriteLine("Local variable y = " & y)
            y -= 1
            i -= 1
            Goto label
        End If
    End Sub
End Module
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
Static variable x = 5
Static variable x = 6
Static variable x = 7
Local variable y = 8
Local variable y = 8
Local variable y = 8
]]>)
        End Sub

        <Fact>
        Public Sub ArrayWithSizes()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
        TestProc()
        TestProc()
    End Sub

    Shared Sub TestProc()
        Static x(9) As Integer
        Static y(8), z(7) As Integer

        System.Console.WriteLine(x.Length)
        System.Console.WriteLine(y.Length)
        System.Console.WriteLine(z.Length)

        x = New Integer(7) {}
        y = New Integer(8) {}
        z = New Integer(9) {}
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
10
9
8
8
9
10
]]>)
        End Sub

        <Fact>
        Public Sub AsNew1()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
        TestProc()
        TestProc()
    End Sub

    Shared Sub TestProc()
        Static x, y As New Object()

        System.Console.WriteLine(x Is Nothing)
        System.Console.WriteLine(y Is Nothing)
        System.Console.WriteLine(x Is y)

        x = Nothing
        y = Nothing
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
False
False
False
True
True
True
]]>)
        End Sub

        <Fact>
        Public Sub AsNew2()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Structure Value
        Public z As Integer
    End Structure

    Shared Sub Main()
        TestProc()
        TestProc()
    End Sub

    Shared Sub TestProc()
        Static x, y As New Value() With {.z = 1}

        System.Console.WriteLine(x.z)
        System.Console.WriteLine(y.z)

        x = New Value() With {.z = 2}
        y = x
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
1
1
2
2
]]>)
        End Sub

        <Fact>
        Public Sub UseSiteErrors()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
    End Sub

    Shared Sub TestProc1()
        Static x(9) As Integer
        Static y1, y2 As New Integer()
        Static z As Integer = 1
    End Sub

    Shared Sub TestProc2()
        Static x() As Integer
        Static y As Integer

        x = nothing
        y = 1
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlib(compilationDef, options:=TestOptions.ReleaseExe)
            AssertTheseEmitDiagnostics(compilation,
<expected>
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor' is not defined.
        Static x(9) As Integer
               ~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor' is not defined.
        Static x(9) As Integer
               ~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State' is not defined.
        Static x(9) As Integer
               ~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State' is not defined.
        Static y1, y2 As New Integer()
                         ~~~~~~~~~~~~~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor' is not defined.
        Static z As Integer = 1
                              ~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor' is not defined.
        Static z As Integer = 1
                              ~
BC35000: Requested operation is not available because the runtime library function 'Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State' is not defined.
        Static z As Integer = 1
                              ~
</expected>)
        End Sub

        <Fact>
        Public Sub DimWithStatic()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
        TestProc()
        TestProc()
    End Sub

    Shared Sub TestProc()
        Static Dim x As Integer = 1
        Dim Static y As Integer = 2

        System.Console.WriteLine(x)
        System.Console.WriteLine(y)

        x+=2
        y+=3
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
1
2
3
5
]]>)
        End Sub

        <Fact>
        Public Sub ModifierErrors()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
    End Sub

    Shared Sub TestProc()
        Static Dim Dim x As Integer = 1
        Dim Static Static y As Integer = 2
        Static Dim Static z As Integer = 1
        Dim Static Dim u As Integer = 2
        Static Const v As Integer = 0 
        Const Static w As Integer = 0 
        Dim Static Const a As Integer = 0 
        Dim Const Static b As Integer = 0 
        Const Dim Static c As Integer = 0 
        Const Static Static d As Integer = 0 
        Static Static e As Integer = 0 
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            AssertTheseDiagnostics(compilation,
<expected>
BC30178: Specifier is duplicated.
        Static Dim Dim x As Integer = 1
                   ~~~
BC30178: Specifier is duplicated.
        Dim Static Static y As Integer = 2
                   ~~~~~~
BC30178: Specifier is duplicated.
        Static Dim Static z As Integer = 1
                   ~~~~~~
BC30178: Specifier is duplicated.
        Dim Static Dim u As Integer = 2
                   ~~~
BC30246: 'Static' is not valid on a local constant declaration.
        Static Const v As Integer = 0 
        ~~~~~~
BC42099: Unused local constant: 'v'.
        Static Const v As Integer = 0 
                     ~
BC30246: 'Static' is not valid on a local constant declaration.
        Const Static w As Integer = 0 
              ~~~~~~
BC42099: Unused local constant: 'w'.
        Const Static w As Integer = 0 
                     ~
BC30246: 'Dim' is not valid on a local constant declaration.
        Dim Static Const a As Integer = 0 
        ~~~
BC42099: Unused local constant: 'a'.
        Dim Static Const a As Integer = 0 
                         ~
BC30246: 'Dim' is not valid on a local constant declaration.
        Dim Const Static b As Integer = 0 
        ~~~
BC42099: Unused local constant: 'b'.
        Dim Const Static b As Integer = 0 
                         ~
BC30246: 'Dim' is not valid on a local constant declaration.
        Const Dim Static c As Integer = 0 
              ~~~
BC42099: Unused local constant: 'c'.
        Const Dim Static c As Integer = 0 
                         ~
BC30246: 'Static' is not valid on a local constant declaration.
        Const Static Static d As Integer = 0 
              ~~~~~~
BC30178: Specifier is duplicated.
        Const Static Static d As Integer = 0 
                     ~~~~~~
BC42099: Unused local constant: 'd'.
        Const Static Static d As Integer = 0 
                            ~
BC30178: Specifier is duplicated.
        Static Static e As Integer = 0 
               ~~~~~~
</expected>)
        End Sub

        <Fact>
        Public Sub FieldMetadata()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Imports System
Imports System.Linq

Class Test

    Shared Sub Main()
        Dim type = GetType(Test)

        For Each fld In type.GetFields(Reflection.BindingFlags.DeclaredOnly Or
                                        Reflection.BindingFlags.NonPublic Or
                                        Reflection.BindingFlags.Public Or
                                        Reflection.BindingFlags.Instance Or
                                        Reflection.BindingFlags.Static).OrderBy(Function(f) f.Name)
            System.Console.WriteLine(fld.Name)
            System.Console.WriteLine(fld.Attributes)
            System.Console.WriteLine(fld.GetCustomAttributes(True).Length)
        Next
    End Sub

    Shared Sub TestProc1()
        Static x As Integer
        x = 0
    End Sub

    Sub TestProc2()
        Static x As Integer
        x = 0
    End Sub

    Shared Sub TestProc3()
        Static x As Integer = 0
        x = 0
    End Sub

    Sub TestProc4()
        Static x As Integer = 0
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntimeAndReferences(compilationDef, {SystemCoreRef}, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
$STATIC$TestProc1$001$x
529
0
$STATIC$TestProc2$2001$x
513
0
$STATIC$TestProc3$001$x
529
0
$STATIC$TestProc3$001$x$Init
529
0
$STATIC$TestProc4$2001$x
513
0
$STATIC$TestProc4$2001$x$Init
513
0
]]>)
        End Sub

        <Fact>
        Public Sub ILTest1()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
    End Sub

    Shared Sub TestProc1(a As TestStruct1)
        Static x As Integer = 0
    End Sub

    Shared Sub TestProc2(a As TestStruct2)
        Static x As Integer
        x = 0
    End Sub

    Structure TestStruct1
    End Structure

    Structure TestStruct2
    End Structure

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.DebugExe)

            Dim verifier = CompileAndVerify(compilation)

            verifier.VerifyIL("Test.TestProc1",
            <![CDATA[
{
  // Code size      141 (0x8d)
  .maxstack  3
  .locals init (Boolean V_0,
                Boolean V_1)
  IL_0000:  nop
  IL_0001:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_0006:  ldnull
  IL_0007:  cgt.un
  IL_0009:  stloc.1
  IL_000a:  ldloc.1
  IL_000b:  brtrue.s   IL_001e
  IL_000d:  ldsflda    "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_0012:  newobj     "Sub Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor()"
  IL_0017:  ldnull
  IL_0018:  call       "Function System.Threading.Interlocked.CompareExchange(Of Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag)(ByRef Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag, Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag, Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag) As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_001d:  pop
  IL_001e:  ldc.i4.0
  IL_001f:  stloc.0
  .try
  {
    IL_0020:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0025:  ldloca.s   V_0
    IL_0027:  call       "Sub System.Threading.Monitor.Enter(Object, ByRef Boolean)"
    IL_002c:  nop
    IL_002d:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0032:  ldfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_0037:  ldc.i4.0
    IL_0038:  cgt.un
    IL_003a:  stloc.1
    IL_003b:  ldloc.1
    IL_003c:  brtrue.s   IL_0051
    IL_003e:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0043:  ldc.i4.2
    IL_0044:  stfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_0049:  ldc.i4.0
    IL_004a:  stsfld     "Test.x As Integer"
    IL_004f:  br.s       IL_006b
    IL_0051:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0056:  ldfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_005b:  ldc.i4.2
    IL_005c:  ceq
    IL_005e:  ldc.i4.0
    IL_005f:  ceq
    IL_0061:  stloc.1
    IL_0062:  ldloc.1
    IL_0063:  brtrue.s   IL_006b
    IL_0065:  newobj     "Sub Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor()"
    IL_006a:  throw
    IL_006b:  leave.s    IL_008c
  }
  finally
  {
    IL_006d:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0072:  ldc.i4.1
    IL_0073:  stfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_0078:  ldloc.0
    IL_0079:  ldc.i4.0
    IL_007a:  ceq
    IL_007c:  stloc.1
    IL_007d:  ldloc.1
    IL_007e:  brtrue.s   IL_008b
    IL_0080:  ldsfld     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0085:  call       "Sub System.Threading.Monitor.Exit(Object)"
    IL_008a:  nop
    IL_008b:  endfinally
  }
  IL_008c:  ret
}
]]>)

            verifier.VerifyIL("Test.TestProc2",
            <![CDATA[
{
  // Code size        8 (0x8)
  .maxstack  1
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stsfld     "Test.x As Integer"
  IL_0007:  ret
}
]]>)
        End Sub

        <Fact>
        Public Sub ILTest2()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
    End Sub

    Sub TestProc1(a As TestStruct1)
        Static x As Integer = 0
    End Sub

    Sub TestProc2(a As TestStruct2)
        Static x As Integer
        x = 0
    End Sub

    Structure TestStruct1
    End Structure

    Structure TestStruct2
    End Structure

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithReferences(
                compilationDef,
                {MsvbRef, TestReferences.NetFx.v2_0_50727.mscorlib},
                TestOptions.DebugExe.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default))

            Dim verifier = CompileAndVerify(compilation)

            verifier.VerifyIL("Test.TestProc1",
            <![CDATA[
{
  // Code size      138 (0x8a)
  .maxstack  3
  .locals init (Boolean V_0)
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_0007:  ldnull
  IL_0008:  cgt.un
  IL_000a:  stloc.0
  IL_000b:  ldloc.0
  IL_000c:  brtrue.s   IL_0020
  IL_000e:  ldarg.0
  IL_000f:  ldflda     "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_0014:  newobj     "Sub Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag..ctor()"
  IL_0019:  ldnull
  IL_001a:  call       "Function System.Threading.Interlocked.CompareExchange(Of Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag)(ByRef Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag, Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag, Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag) As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_001f:  pop
  IL_0020:  ldarg.0
  IL_0021:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
  IL_0026:  call       "Sub System.Threading.Monitor.Enter(Object)"
  IL_002b:  nop
  .try
  {
    IL_002c:  ldarg.0
    IL_002d:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0032:  ldfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_0037:  ldc.i4.0
    IL_0038:  cgt.un
    IL_003a:  stloc.0
    IL_003b:  ldloc.0
    IL_003c:  brtrue.s   IL_0053
    IL_003e:  ldarg.0
    IL_003f:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0044:  ldc.i4.2
    IL_0045:  stfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_004a:  ldarg.0
    IL_004b:  ldc.i4.0
    IL_004c:  stfld      "Test.x As Integer"
    IL_0051:  br.s       IL_006e
    IL_0053:  ldarg.0
    IL_0054:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0059:  ldfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_005e:  ldc.i4.2
    IL_005f:  ceq
    IL_0061:  ldc.i4.0
    IL_0062:  ceq
    IL_0064:  stloc.0
    IL_0065:  ldloc.0
    IL_0066:  brtrue.s   IL_006e
    IL_0068:  newobj     "Sub Microsoft.VisualBasic.CompilerServices.IncompleteInitialization..ctor()"
    IL_006d:  throw
    IL_006e:  leave.s    IL_0089
  }
  finally
  {
    IL_0070:  ldarg.0
    IL_0071:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0076:  ldc.i4.1
    IL_0077:  stfld      "Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag.State As Short"
    IL_007c:  ldarg.0
    IL_007d:  ldfld      "Test.x$Init As Microsoft.VisualBasic.CompilerServices.StaticLocalInitFlag"
    IL_0082:  call       "Sub System.Threading.Monitor.Exit(Object)"
    IL_0087:  nop
    IL_0088:  endfinally
  }
  IL_0089:  ret
}
]]>)

            verifier.VerifyIL("Test.TestProc2",
            <![CDATA[
{
  // Code size        9 (0x9)
  .maxstack  2
  IL_0000:  nop
  IL_0001:  ldarg.0
  IL_0002:  ldc.i4.0
  IL_0003:  stfld      "Test.x As Integer"
  IL_0008:  ret
}
]]>)
        End Sub

        <Fact>
        Public Sub IncompleteInitialization()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Imports System

Class Test

    Shared Sub Main()
        Try
            TestProc1()
        Catch ex As Exception
            System.Console.WriteLine(ex.GetType())
        End Try

        TestProc1()

        Dim t As New Test()

        Try
            t.TestProc2()
        Catch ex As Exception
            System.Console.WriteLine(ex.GetType())
        End Try

        t.TestProc2()
    End Sub

    Shared Function TestProc1() As Integer
        System.Console.WriteLine("TestProc1")
        Static x As Integer = TestProc1()
        System.Console.WriteLine(x)
        Return 123
    End Function

    Function TestProc2() As Integer
        System.Console.WriteLine("TestProc2")
        Static x As Integer = TestProc2()
        System.Console.WriteLine(x)
        Return 456
    End Function

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            CompileAndVerify(compilation,
            <![CDATA[
TestProc1
TestProc1
Microsoft.VisualBasic.CompilerServices.IncompleteInitialization
TestProc1
0
TestProc2
TestProc2
Microsoft.VisualBasic.CompilerServices.IncompleteInitialization
TestProc2
0
]]>)
        End Sub

        <Fact>
        Public Sub NameConflicts()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
        Dim x As Integer = 1

        Static y As Integer = 2

        If x > 0 Then
            Static x As Integer = 3
            Static y As Integer = 4
            Static z As Integer = 5
        Else
            Static z As Integer = 6
        End If
    End Sub

End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            AssertTheseDiagnostics(compilation,
<expected>
BC30616: Variable 'x' hides a variable in an enclosing block.
            Static x As Integer = 3
                   ~
BC30616: Variable 'y' hides a variable in an enclosing block.
            Static y As Integer = 4
                   ~
BC31401: Static local variable 'y' is already declared.
            Static y As Integer = 4
                   ~
BC31401: Static local variable 'z' is already declared.
            Static z As Integer = 6
                   ~
</expected>)
        End Sub

        <Fact>
        Public Sub RestrictedTypes()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Imports System

Class Test

    Shared Sub Main()
        Static x1 As ArgIterator
        Static x2 As ArgIterator()
        Static x3 As New ArgIterator
        Static x4, x5 As New ArgIterator
        Static x6() As ArgIterator
        Static x7(1) As ArgIterator
    End Sub
End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            AssertTheseDiagnostics(compilation,
<expected>
BC42024: Unused local variable: 'x1'.
        Static x1 As ArgIterator
               ~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x1 As ArgIterator
                     ~~~~~~~~~~~
BC42024: Unused local variable: 'x2'.
        Static x2 As ArgIterator()
               ~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x2 As ArgIterator()
                     ~~~~~~~~~~~~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x3 As New ArgIterator
                         ~~~~~~~~~~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x4, x5 As New ArgIterator
                             ~~~~~~~~~~~
BC42024: Unused local variable: 'x6'.
        Static x6() As ArgIterator
               ~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x6() As ArgIterator
                       ~~~~~~~~~~~
BC31396: 'ArgIterator' cannot be made nullable, and cannot be used as the data type of an array element, field, anonymous type member, type argument, 'ByRef' parameter, or return statement.
        Static x7(1) As ArgIterator
                        ~~~~~~~~~~~
</expected>)
        End Sub

        <Fact>
        Public Sub StaticInBetween()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Class Test

    Shared Sub Main()
        TestProc()
        TestProc()
    End Sub

    Shared Sub TestProc()
        Dim x As Integer
        Static y As Integer
        Dim z As Integer

        x = 0
        z = -1
        y += 1
        System.Console.WriteLine(x)
        System.Console.WriteLine(y)
        System.Console.WriteLine(z)
    End Sub
End Class
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.DebugExe)

            Dim v = CompileAndVerify(compilation, expectedOutput:="
0
1
-1
0
2
-1
")

            v.VerifyIL("Test.TestProc", <![CDATA[
{
  // Code size       43 (0x2b)
  .maxstack  2
  .locals init (Integer V_0, //x
                Integer V_1) //z
  IL_0000:  nop
  IL_0001:  ldc.i4.0
  IL_0002:  stloc.0
  IL_0003:  ldc.i4.m1
  IL_0004:  stloc.1
  IL_0005:  ldsfld     "Test.y As Integer"
  IL_000a:  ldc.i4.1
  IL_000b:  add.ovf
  IL_000c:  stsfld     "Test.y As Integer"
  IL_0011:  ldloc.0
  IL_0012:  call       "Sub System.Console.WriteLine(Integer)"
  IL_0017:  nop
  IL_0018:  ldsfld     "Test.y As Integer"
  IL_001d:  call       "Sub System.Console.WriteLine(Integer)"
  IL_0022:  nop
  IL_0023:  ldloc.1
  IL_0024:  call       "Sub System.Console.WriteLine(Integer)"
  IL_0029:  nop
  IL_002a:  ret
}
]]>)

            compilation.VerifyDiagnostics()
        End Sub

        <WorkItem(530442, "DevDiv")>
        <Fact()>
        Public Sub Bug16169()
            Dim compilationDef =
<compilation name="SimpleTest1">
    <file name="a.vb"><![CDATA[
Imports System
Public Module Module1
    Public Sub Main()
        MaximumLengthIdentifierIn2012()
        MaximumLengthIdentifierIn2012()
        MaximumLengthIdentifierIn2012()
    End Sub
    Sub MaximumLengthIdentifierIn2012()
        Static abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz As Integer = 1
        Console.WriteLine(abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz.ToString)
        abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz += 1
    End Sub
End Module
    ]]></file>
</compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, TestOptions.ReleaseExe)

            AssertTheseDiagnostics(compilation,
<expected>
</expected>)

            CompileAndVerify(compilation)
        End Sub

    End Class
End Namespace
