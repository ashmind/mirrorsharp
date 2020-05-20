import { TestDriver } from './test-driver';

test('cursor move is sent to server', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction({ selection: { anchor: 2 } });
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('M2');
});