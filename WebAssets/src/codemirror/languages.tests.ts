import { LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../protocol/languages';
import { TestDriver } from '../testing/test-driver-jest';
import { CODE_CSHARP, CODE_FSHARP, CODE_IL, CODE_PHP, CODE_VB } from './languages/test.data';

test('C# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({ text: CODE_CSHARP });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('VB highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        language: LANGUAGE_VB,
        text: CODE_VB
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('F# highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        language: LANGUAGE_FSHARP,
        text: CODE_FSHARP
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('IL highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        language: LANGUAGE_IL,
        text: CODE_IL
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});

test('PHP highlighting applies expected classes', async () => {
    const driver = await TestDriver.new({
        language: LANGUAGE_PHP,
        text: CODE_PHP
    });
    await driver.completeBackgroundWork();

    const html = driver.getCodeMirrorView().contentDOM.innerHTML;

    expect(html).toMatchSnapshot();
});