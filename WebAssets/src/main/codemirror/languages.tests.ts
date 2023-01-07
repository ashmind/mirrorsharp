import { LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../../interfaces/protocol';
import { TestDriver } from '../../testing/test-driver-jest';
import { CODE_CSHARP, CODE_FSHARP, CODE_IL, CODE_PHP, CODE_VB } from './languages/test.data';

test('C# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({ text: CODE_CSHARP });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('VB highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_VB,
        options: { language: LANGUAGE_VB }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('F# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_FSHARP,
        options: { language: LANGUAGE_FSHARP }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('IL highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_IL,
        options: { language: LANGUAGE_IL }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('PHP highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        text: CODE_PHP,
        options: { language: LANGUAGE_PHP }
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});