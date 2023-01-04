import { TestDriver } from '../../../testing/test-driver';
import { selectAll } from '@codemirror/commands';

test('Tab indents selected block', async () => {
    const text = `abc\r\ndef`;
    const driver = await TestDriver.new({ text });

    selectAll(driver.getCodeMirrorView());
    driver.domEvents.keydown('Tab');

    await driver.completeBackgroundWork();

    expect(driver.mirrorsharp.getText()).toEqual('    abc\r\n    def');
});

test('Shift+Tab unindents selected block', async () => {
    const text = `    abc\r\n    def`;
    const driver = await TestDriver.new({ text });

    selectAll(driver.getCodeMirrorView());
    driver.domEvents.keydown('Tab', { shiftKey: true });

    await driver.completeBackgroundWork();

    expect(driver.mirrorsharp.getText()).toEqual('abc\r\ndef');
});