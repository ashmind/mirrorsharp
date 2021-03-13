import { TestDriver } from './test-driver';

test('C# highlighting is rendered correctly', async () => {
    if (TestDriver.shouldSkipRender)
        return;
    const driver = await TestDriver.new({ text: [
        'public class C<T> {',
        '    // test comment',
        '    public void M<T>() {',
        '        double d = 1.2e3;',
        '        string s1 = "test";',
        '        string s2 = $"a{s1}b";',
        "        char c = 't';",
        '        Action a = () => {};',
        '    }',
        '}'
    ].join('\n') });
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});