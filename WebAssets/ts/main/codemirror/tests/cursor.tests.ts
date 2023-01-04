import { TestDriver } from '../../../testing/test-driver';

test('cursor move is sent to server', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction({ selection: { anchor: 2 } });
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toEqual('M2');
});

test('cursor move is not sent to server if cursor position was sent by edit', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|' });

    driver.text.type('x');
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.filter(s => s !== 'U')).toEqual(['Cx']);
});