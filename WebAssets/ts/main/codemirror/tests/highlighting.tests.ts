import { LANGUAGE_FSHARP, LANGUAGE_VB } from '../../../interfaces/protocol';
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
Class C (Of T)
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