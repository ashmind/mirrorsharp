import { LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../../../interfaces/protocol';
import { TestDriver } from '../../../testing/test-driver';

const CODE_CSHARP = `
public class C<T> {
    // test comment
    public void M<U>() {
        double d = 1.2e3;
        string s1 = "test";
        string s2 = $"a{s1}b";
        char c = 't';
        Action a = () => {};
    }
}`.replace(/\r\n|\r|\n/g, '\r\n').trim();

test('C# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({ text: CODE_CSHARP });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('C# highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({ text: CODE_CSHARP });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});

const CODE_VB = `
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
`.replace(/\r\n|\r|\n/g, '\r\n').trim();

test('VB highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_VB,
        options: { language: LANGUAGE_VB }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('VB highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({
        text: CODE_VB,
        options: { language: LANGUAGE_VB }
    });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});

const CODE_FSHARP = `
type C<'a> =
    // test comment
    member this.M<'b>() =
        let d = 1.2e3
        let s1 = "test"
        let s2 = $"a{s1}b"
        let c = 't'
        let a = fun () -> ()
        ()
`.replace(/\r\n|\r|\n/g, '\r\n').trim();

test('F# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_FSHARP,
        options: { language: LANGUAGE_FSHARP }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('F# highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({
        text: CODE_FSHARP,
        options: { language: LANGUAGE_FSHARP }
    });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});

const CODE_IL = `
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
`.replace(/\r\n|\r|\n/g, '\r\n').trim();

test('IL highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_IL,
        options: { language: LANGUAGE_IL }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('IL highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({
        text: CODE_IL,
        options: { language: LANGUAGE_IL }
    });
    await driver.completeBackgroundWork();

    const rendered = await driver.render({ size: { height: 500 } });

    expect(rendered).toMatchImageSnapshot();
});

const CODE_PHP = `
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
`.replace(/\r\n|\r|\n/g, '\r\n').trim();

test('PHP highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_PHP,
        options: { language: LANGUAGE_PHP }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('PHP highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({
        text: CODE_PHP,
        options: { language: LANGUAGE_PHP }
    });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});