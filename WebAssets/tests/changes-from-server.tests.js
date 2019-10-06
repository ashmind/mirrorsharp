const TestDriver = require('./test-driver.js');

test('array of changes is handled correctly', async () => {
    const text = 'if ((true)){}';
    const driver = await TestDriver.new({ text });

    driver.receive.changes([
        { start: text.indexOf('(tr'), length: 1, text: '' },
        { start: text.indexOf('){}'), length: 1, text: '' }
    ]);
    await driver.completeBackgroundWork();

    const updated = driver.getCodeMirror().getValue();
    expect(updated).toBe('if (true){}');
});