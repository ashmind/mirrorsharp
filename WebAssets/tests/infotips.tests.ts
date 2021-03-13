import { TestDriver, timers } from './test-driver';

const waitForHover = async (driver: TestDriver) => {
    timers.advanceTimersByTime(1000); // actual is 750, but just in case
    await driver.completeBackgroundWork();
};

test('hover requests infotip', async () => {
    const driver = await TestDriver.new({ text: 'a b c' });
    const cmView = driver.getCodeMirrorView();
    cmView.posAtCoords = () => 0;
    cmView.coordsAtPos = () => ({ left: 0, right: 0, top: 0, bottom: 0 });

    driver.domEvents.mousemove(cmView.contentDOM.firstChild!);
    await waitForHover(driver);

    expect(driver.socket.sent).toContain('I0');
});