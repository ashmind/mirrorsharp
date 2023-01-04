import { TestDriver } from '../testing/test-driver';

test('setText replaces document text', async () => {
    const driver = await TestDriver.new({
        text: 'initial'
    });

    driver.mirrorsharp.setText('updated');
    await driver.completeBackgroundWork();

    expect(driver.mirrorsharp.getText()).toBe('updated');
    expect(driver.socket.sent).toContain(`R0:7:0::updated`);
});