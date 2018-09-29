const TestDriver = require('./test-driver.js');

test('options echo without a language does not unset language', async () => {
    const driver = await TestDriver.new({
        options: { language: 'C#' }
    });
    driver.receive.optionsEcho({});

    expect(driver.cm.getMode().name).toBe('clike');
});