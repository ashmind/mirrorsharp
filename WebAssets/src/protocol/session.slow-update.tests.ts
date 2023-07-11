import { TestDriver } from '../testing/test-driver-jest';

test('slowUpdate is not sent if there is no initial text', async () => {
    const driver = await TestDriver.new({
        skipSocketOpen: true
    });

    driver.socket.open();
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    expect(driver.socket.sent).toEqual([]);
});

test('slowUpdate is sent if there is initial text', async () => {
    const driver = await TestDriver.new({
        text: 'Test',
        skipSocketOpen: true
    });

    driver.socket.open();
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent after initial text even if lint runs before connection is open', async () => {
    const driver = await TestDriver.new({
        text: 'Test',
        skipSocketOpen: true
    });

    await driver.advanceTimeToSlowUpdateAndCompleteWork();
    driver.socket.open();
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent if text is set after initial setup', async () => {
    const driver = await TestDriver.new({
        skipSocketOpen: true
    });

    driver.socket.open();
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    driver.dispatchCodeMirrorTransaction({ changes: { from: 0, insert: 'Test' } });
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
        'U'
    ]);
});

test('slowUpdate is sent only once on reopen if connection is closed', async () => {
    const driver = await TestDriver.new({
        textWithCursor: 'a|',
        skipSocketOpen: true
    });

    await driver.advanceTimeToSlowUpdateAndCompleteWork();
    driver.text.type('b');
    await driver.advanceTimeToSlowUpdateAndCompleteWork();
    driver.text.type('c');
    driver.socket.open();
    await driver.advanceTimeToSlowUpdateAndCompleteWork();

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