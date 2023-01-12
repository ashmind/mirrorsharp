import { TestDriver } from '../testing/test-driver-jest';

test('does not send default options on connection open', async () => {
    const driver = await TestDriver.new({
        language: 'C#',
        keepSocketClosed: true
    });
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual([]);
});

test('sends non-default language on connection open', async () => {
    const driver = await TestDriver.new({
        language: 'Visual Basic',
        keepSocketClosed: true
    });
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Olanguage=Visual Basic']);
});

test('re-sends non-default language on next connection open', async () => {
    const driver = await TestDriver.new({
        language: 'Visual Basic',
        keepSocketClosed: true
    });

    driver.socket.open();
    await driver.completeBackgroundWork();
    driver.socket.sent = [];
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Olanguage=Visual Basic']);
});

test('always sends options before slow update', async () => {
    const driver = await TestDriver.new({
        keepSocketClosed: true,
        text: 'test',
        language: 'Visual Basic'
    });

    await driver.advanceTimeToSlowUpdateAndCompleteWork();
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual([
        'Olanguage=Visual Basic',
        'R0:0:0::test',
        'U'
    ]);
});

test('sends extended options on connection open', async () => {
    const driver = await TestDriver.new({
        serverOptions: { 'x-test': 'value' },
        keepSocketClosed: true
    });
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Ox-test=value,language=C#']);
});

/*test('options echo without a language does not unset language', async () => {
    const driver = await TestDriver.new({
        options: { language: 'C#' }
    });
    driver.receive.optionsEcho({});

    expect((driver.getCodeMirror().getMode() as { name: string }).name).toBe('clike');
});

test('options echo without a language does not unset default language', async () => {
    const driver = await TestDriver.new({});
    driver.receive.optionsEcho({});

    expect((driver.getCodeMirror().getMode() as { name: string }).name).toBe('clike');
});*/

test('options echo without extended option does not unset extended option for next open', async () => {
    const driver = await TestDriver.new({
        serverOptions: { 'x-test': 'value' }
    });
    driver.receive.optionsEcho({});
    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Ox-test=value,language=C#']);
});