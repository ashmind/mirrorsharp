const TestDriver = require('./test-driver.js');

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

function multiline(string) {
    return string
        .replace(/ *┊/g, '')
        .replace(/\r?\n/g, '\r\n')
        .trim();
}