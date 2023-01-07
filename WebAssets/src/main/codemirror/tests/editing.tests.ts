import { TestDriver } from '../../../testing/test-driver-jest';

test('change at cursor is sent as typed text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.text.type('x');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('Cx');
});

test('enter at cursor is sent as typed newline', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.domEvents.keydown('Enter');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('C\n');
});

test('change not at cursor is sent as replaced text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction({
        changes: { from: 2, insert: 'x' }
    });
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('R2:0:1::x');
});

// two changed are handled in a special way in code
test('two changes are sent as individual replaced text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction({
        changes: [{ from: 1, insert: 'x' }, { from: 2, insert: 'y' }]
    });
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U'));
    expect(lastSent).toEqual([
        'R1:0:1::x',
        'R2:0:1::y'
    ]);
});

test('three changes are sent as individual replaced text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction({
        changes: [{ from: 1, insert: 'x' }, { from: 2, insert: 'y' }, { from: 3, insert: 'z' }]
    });
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U'));
    expect(lastSent).toEqual([
        'R1:0:1::x',
        'R2:0:1::y',
        'R3:0:1::z'
    ]);
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