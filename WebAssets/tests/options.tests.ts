import { TestDriver } from './test-driver';

test('options echo without a language does not unset language', async () => {
    const driver = await TestDriver.new({
        options: { language: 'C#' }
    });
    driver.receive.optionsEcho({});

    expect(driver.getCodeMirror().getMode().name).toBe('clike');
});