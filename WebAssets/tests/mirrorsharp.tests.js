const TestDriver = require('./test-driver.js');

// TODO: remove in year 3000 when TC39 finally specs this
// eslint-disable-next-line no-extend-native
Array.prototype.last = Array.prototype.last || function() { return this[this.length - 1]; };

describe('basic editing', () => {
    test('undo sends all changes as a single replace', async () => {
        const driver = await TestDriver.new({ textWithCursor: '{d:f2}{d:f2}|' });
        const cm = driver.getCodeMirror();

        driver.keys.backspace('{d:f2}'.length);
        cm.execCommand('undo');
        await driver.completeBackgroundWork();

        const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).last();
        expect(lastSent).toBe('R6:0:12::{d:f2}');
    });
});

describe('hotkeys', () => {
    test('Shift+Tab un-indents selected block', async () => {
        const text = multiline(`
        ┊    abc
        ┊    def
        ┊    fgh
        `);
        const driver = await TestDriver.new({ text });
        const cm = driver.getCodeMirror();

        driver.keys.press('ctrl+a');
        driver.keys.press('shift+tab');

        await driver.completeBackgroundWork();

        expect(cm.getValue()).toEqual(multiline(`
        ┊abc
        ┊def
        ┊fgh
        `));
    });
});

describe('produced events', () => {
    test('slowUpdateWait is triggered on first change', async () => {
        const slowUpdateWait = jest.fn();
        const driver = await TestDriver.new({ options: { on: { slowUpdateWait } } });

        driver.keys.type('x');
        await driver.completeBackgroundWork();

        expect(slowUpdateWait.mock.calls).toEqual([[]]);
    });
});

function multiline(string) {
    return string
        .replace(/ *┊/g, '')
        .replace(/\r?\n/g, '\r\n')
        .trim();
}