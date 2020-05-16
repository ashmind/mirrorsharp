import { TestDriver } from './test-driver';

test('attempts to reopen connection if lost', async () => {
    const driver = await TestDriver.new({});

    driver.socket.trigger('close');
    await driver.completeBackgroundWork();

    expect(driver.socket.createdCount).toBe(2);
});