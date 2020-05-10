import { TestDriver } from './test-driver';

test('does not send default options on connection open', async () => {
    const driver = await TestDriver.new({
        options: { language: 'C#' }
    });
    driver.socket.trigger('open');
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual([]);
});

test('sends non-default language on connection open', async () => {
    const driver = await TestDriver.new({
        options: { language: 'Visual Basic' }
    });
    driver.socket.trigger('open');
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Olanguage=Visual Basic']);
});

test('sends extended options on connection open', async () => {
    const driver = await TestDriver.new({
        options: {
            initialServerOptions: { 'x-test': 'value' }
        }
    });
    driver.socket.trigger('open');
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
        options: {
            initialServerOptions: { 'x-test': 'value' }
        }
    });
    driver.receive.optionsEcho({});
    driver.socket.trigger('open');
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toEqual(['Ox-test=value,language=C#']);
});