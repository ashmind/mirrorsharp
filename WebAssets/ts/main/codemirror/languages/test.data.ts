const normalize = (code: string) => code.replace(/\r\n|\r|\n/g, '\r\n').trim();

export const CODE_CSHARP = normalize(`
public class C<T> {
    // test comment
    public void M<U>() {
        double d = 1.2e3;
        string s1 = "test";
        string s2 = $"a{s1}b";
        char c = 't';
        Action a = () => {};
    }
}`);

export const CODE_VB = normalize(`
Public Class C (Of T)
    ' test comment
    Public Sub M (Of U)()
        Dim d As Decimal = 1.2e3
        Dim s1 As String = "test"
        Dim s2 As String = $"a{s1}b"
        Dim c As Char = 't'
        Dim a As Action = Sub()
                          End Sub
    End Sub
End Class
`);

export const CODE_FSHARP = normalize(`
type C<'a> =
    // test comment
    member this.M<'b>() =
        let d = 1.2e3
        let s1 = "test"
        let s2 = $"a{s1}b"
        let c = 't'
        let a = fun () -> ()
        ()
`);

export const CODE_IL = normalize(`
.class public C\`1<T>
    extends [System.Runtime]System.Object
{
    // test comment
    .method public
        instance void M<U> () cil managed
    {
        .maxstack 3
        .locals init (
            [0] float64 d,
            [1] string s1
        )

        IL_0000: ldc.r8 1.2e3
        IL_0009: stloc.0
        IL_000a: ldstr "test"
        IL_000f: stloc.1
        IL_0010: ret
    }
}
`);

export const CODE_PHP = normalize(`
<?php

class C {
    // test comment
    public function M() {
        $d = 1.2e3;
        $s1 = 'test';
        $s2 = "a{$s1}b";
        $a = fn() => 0;
    }
}
`);