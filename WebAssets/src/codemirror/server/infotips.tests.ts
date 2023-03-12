import { TestDriver, timers } from '../../testing/test-driver-jest';
import { INFOTIP_EVENTHANDLER } from './infotips.test.data';

const mockHoverDependencies = (driver: TestDriver) => {
    const cmView = driver.getCodeMirrorView();
    cmView.posAtCoords = () => 0;
    cmView.coordsAtPos = () => ({ left: 0, right: 0, top: 0, bottom: 0 });
};

const waitForHover = async (driver: TestDriver) => {
    timers.advanceTimersByTime(1000); // actual is 750, but just in case
    await driver.completeBackgroundWork();
};

test('hover requests infotip', async () => {
    const driver = await TestDriver.new({ text: 'test' });
    mockHoverDependencies(driver);

    driver.domEvents.mousemove(
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        driver.getCodeMirrorView().contentDOM.firstChild!
    );
    await waitForHover(driver);

    expect(driver.socket.sent).toContain('I0');
});

test('hover applies received infotip', async () => {
    const driver = await TestDriver.new({ text: 'test' });
    mockHoverDependencies(driver);

    driver.domEvents.mousemove(
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        driver.getCodeMirrorView().contentDOM.firstChild!
    );
    await waitForHover(driver);
    driver.receive.infotip(INFOTIP_EVENTHANDLER);
    await driver.completeBackgroundWork();

    const tooltip = driver.getCodeMirrorView().dom.querySelector('.cm-tooltip');
    expect(tooltip).toBeTruthy();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(tooltip!.innerHTML).toMatchSnapshot();
});