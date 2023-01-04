import { TestDriver } from '../../../testing/test-driver';

const csharpCode = `
public class C<T> {
    // test comment
    public void M<T>() {
        double d = 1.2e3;
        string s1 = "test";
        string s2 = $"a{s1}b";
        char c = 't';
        Action a = () => {};
    }
}`.trim();

test('C# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({ text: csharpCode });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('C# highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({ text: csharpCode });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});