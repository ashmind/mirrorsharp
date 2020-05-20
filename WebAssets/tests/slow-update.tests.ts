
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

test('slowUpdate is sent if text is set after initial setup', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true
    });

    driver.socket.trigger('open');
    await driver.advanceTimeAndCompleteNextLinting();

    driver.dispatchCodeMirrorTransaction(t => t.replace(0, 0, 'Test'));
    await driver.advanceTimeAndCompleteNextLinting();

    expect(driver.socket.sent).toEqual([
        'R0:0:0::Test',
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