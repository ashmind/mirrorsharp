const TestDriver = require('./test-driver.js');

// TODO: remove in year 3000 when TC39 finally specs this
// eslint-disable-next-line no-extend-native
Array.prototype.last = Array.prototype.last || function() { return this[this.length - 1]; };

test('opening hints requests info for the first hint', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion()
    ]);
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('SI0');
});

test('selecting hint requests info for the selected hint', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion(),
        completion(),
        completion()
    ]);
    await driver.completeBackgroundWork();
    driver.keys.press('down');
    driver.keys.press('down');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('SI2');
});

test('picking hint cancels info request', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion(),
        completion()
    ]);
    await driver.completeBackgroundWork();
    driver.keys.press('down');
    driver.keys.press('tab');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('S1');
});

test('completionDescription message shows info tip', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([completion()]);
    await driver.completeBackgroundWork();
    driver.receive.completionInfo(0, [
        { kind: 'type',  text: 'int' },
        { kind: 'space', text: ' ' },
        { kind: 'local', text: 'x' },
    ]);
    await driver.completeBackgroundWork();

    const tip = getTooltip();
    expect(tip.style.display).toBe('block');
    expect(tip.innerHTML).toBe([
        '<span class="cm-type">int</span>',
        '<span class="cm-space"> </span>',
        '<span class="cm-local">x</span>'
    ].join(''));
});

test('picking hint hides info tip', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([completion()]);
    await driver.completeBackgroundWork();
    driver.receive.completionInfo(0, []);
    await driver.completeBackgroundWork();
    driver.keys.press('tab');
    await driver.completeBackgroundWork();

    const tip = getTooltip();
    expect(tip.style.display).toBe('none');
});

function completion() {
    return { kinds: [] };
}

function getTooltip() {
    return document.querySelector('.mirrorsharp-hint-info-tooltip');
}