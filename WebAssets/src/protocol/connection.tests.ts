import { TestDriver } from '../testing/test-driver-jest';

test('attempts to reopen connection if lost', async () => {
    const driver = await TestDriver.new({});

    driver.socket.close();
    await driver.completeBackgroundWork();

    expect(driver.socket.createdCount).toBe(2);
});