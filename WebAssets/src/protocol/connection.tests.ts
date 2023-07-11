import { installMockSocket, MockSocket } from '../testing/shared/mock-socket';
import { TestDriver } from '../testing/test-driver-jest';
import { Connection } from './connection';

test('attempts to reopen socket if closed', async () => {
    const driver = await TestDriver.new({});

    driver.socket.close();
    await driver.completeBackgroundWork();

    expect(driver.socket.createdCount).toBe(2);
});

test('does not attempt to reopen socket if explicitly closed', async () => {
    const socket = installMockSocket(new MockSocket()).mock;
    const connection = new Connection('_', { closed: false });

    connection.close();
    socket.close();

    jest.advanceTimersByTime(1000);

    expect(socket.createdCount).toBe(1);
});

test('attempts to reopen socket at expected intervals', async () => {
    const driver = await TestDriver.new({});

    const retryIntervals = [] as Array<number>;
    let last = {
        createdCount: driver.socket.createdCount,
        retryTime: 0
    };

    driver.socket.close();
    let time = 0;
    for (; time < 180; time += 1) {
        jest.advanceTimersByTime(1000);
        const { createdCount } = driver.socket;
        if (createdCount > last.createdCount) {
            retryIntervals.push(time - last.retryTime);
            last = { createdCount, retryTime: time };
        }
        if (driver.socket.readyState === MockSocket.OPEN)
            driver.socket.close();
    }

    expect(retryIntervals).toEqual([0, 1, 2, 4, 8, 16, 32, 60]);
});