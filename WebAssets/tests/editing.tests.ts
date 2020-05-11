import { TestDriver } from './test-driver';

test('change at cursor is sent as typed text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction(t => t.replace(1, 1, 'x'));
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('Cx');
});

test('change not at cursor is sent as replaced text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction(t => t.replace(2, 2, 'x'));
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('R2:0:0::x');
});

/*test('undo sends all changes as a single replace', async () => {
    const driver = await TestDriver.new({ textWithCursor: '{d:f2}{d:f2}|' });
    const cm = driver.getCodeMirror();

    driver.keys.backspace('{d:f2}'.length);
    cm.execCommand('undo');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('R6:0:12::{d:f2}');
});*/

// test('tab is replaced with 4 spaces', async () => {
//     const driver = await TestDriver.new({ textWithCursor: '|' });

//     driver.keys.press('tab');
//     await driver.completeBackgroundWork();

//     expect(driver.mirrorsharp.getText()).toBe('    ');
// });