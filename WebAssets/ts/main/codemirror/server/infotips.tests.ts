import { TestDriver, timers } from '../../../testing/test-driver-jest';

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

const infotip = {
    kinds: ['delegate', 'public'],
    sections: [
        {
            kind: 'description',
            parts: [
                { text: 'delegate', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'void', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'System', kind: 'namespace' },
                { text: '.', kind: 'punctuation' },
                { text: 'EventHandler', kind: 'delegate' },
                { text: '(', kind: 'punctuation' },
                { text: 'object', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'sender', kind: 'parameter' },
                { text: ',', kind: 'punctuation' },
                { text: ' ', kind: 'space' },
                { text: 'System', kind: 'namespace' },
                { text: '.', kind: 'punctuation' },
                { text: 'EventArgs', kind: 'class' },
                { text: ' ', kind: 'space' },
                { text: 'e', kind: 'parameter' },
                { text: ')', kind: 'punctuation' }
            ]
        },
        {
            kind: 'documentationcomments',
            parts: [
                { text: 'Represents the method that will handle an event that has no event data.', kind: 'text' }
            ]
        }
    ],
    span: { start: 0, length: 0 }
};

test('hover applies received infotip', async () => {
    const driver = await TestDriver.new({ text: 'test' });
    mockHoverDependencies(driver);

    driver.domEvents.mousemove(
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        driver.getCodeMirrorView().contentDOM.firstChild!
    );
    await waitForHover(driver);
    driver.receive.infotip(infotip);
    await driver.completeBackgroundWork();

    const tooltip = driver.getCodeMirrorView().dom.querySelector('.cm-tooltip');
    expect(tooltip).toBeTruthy();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(tooltip!.innerHTML).toMatchSnapshot();
});