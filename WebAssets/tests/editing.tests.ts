import { TestDriver } from './test-driver';

// TODO: remove in year 3000 when TC39 finally specs this
// eslint-disable-next-line no-extend-native
(Array.prototype as any).last = (Array.prototype as any).last || function<T>(this: Array<T>) { return this[this.length - 1]; };
declare global {
    interface Array<T> {
        last(): T;
    }
}

test('undo sends all changes as a single replace', async () => {
    const driver = await TestDriver.new({ textWithCursor: '{d:f2}{d:f2}|' });
    const cm = driver.getCodeMirror();

    driver.keys.backspace('{d:f2}'.length);
    cm.execCommand('undo');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).last();
    expect(lastSent).toBe('R6:0:12::{d:f2}');
});

test('tab is replaced with 4 spaces', async () => {
    const driver = await TestDriver.new({ textWithCursor: '|' });
    const cm = driver.getCodeMirror();

    driver.keys.press('tab');
    await driver.completeBackgroundWork();

    const value = cm.getValue();

    expect(value).toBe('    ');
});