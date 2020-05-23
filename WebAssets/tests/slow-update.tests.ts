
import { TestDriver } from './test-driver';

test('slowUpdate is not sent if there is no initial text', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true
    });

    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    expect(driver.socket.sent).toEqual([]);
});

test('slowUpdate is sent if there is initial text', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true,
        text: 'Test'
    });

    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent after initial text even if lint runs before connection is open', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true,
        text: 'Test'
    });

    await driver.advanceTimeAndCompleteNextLinting();
    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent if text is set after initial setup', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true
    });

    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    driver.dispatchCodeMirrorTransaction({ changes: { from: 0, insert: 'Test' } });
    await driver.advanceTimeAndCompleteNextLinting();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent only once on reopen if connection is closed', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true,
        textWithCursor: 'a|'
    });

    await driver.advanceTimeAndCompleteNextLinting();
    driver.keys.type('b');
    await driver.advanceTimeAndCompleteNextLinting();
    driver.keys.type('c');
    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    const sent = driver.socket.sent;
    expect(sent).toEqual([
        ...(sent.slice(0, sent.length - 1).map(() => expect.not.stringMatching(/^U$/) as unknown)),
        'U'
    ]);
});

// test('slowUpdateWait is triggered on first change', async () => {
//     const slowUpdateWait = jest.fn();
//     const driver = await TestDriver.new({ options: { on: { slowUpdateWait } } });

//     driver.keys.type('x');
//     await driver.completeBackgroundWork();

//     expect(slowUpdateWait.mock.calls).toEqual([[]]);
// });