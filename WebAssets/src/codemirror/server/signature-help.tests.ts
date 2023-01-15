import { getTooltip, showTooltip, Tooltip } from '@codemirror/view';
import { TestDriver } from '../../testing/test-driver-jest';
import { SIGNATURES_INDEX_OF } from './signature-help.test.data';

test('signature help message shows signature help', async () => {
    const driver = await TestDriver.new({ text: '_' });

    driver.receive.signatures({
        span: { start: 0, length: 1 },
        signatures: SIGNATURES_INDEX_OF }
    );
    await driver.completeBackgroundWork();

    const view = driver.getCodeMirrorView();
    const tooltips = view.state.facet(showTooltip).filter(t => t) as ReadonlyArray<Tooltip>;
    expect(tooltips.length).toBe(1);
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const tooltipView = getTooltip(view, tooltips[0]!)!;
    expect(tooltipView.dom.innerHTML).toMatchSnapshot();
});

test('signature help message with empty signatures hides signature help', async () => {
    const driver = await TestDriver.new({ text: '_' });

    driver.receive.signatures({
        span: { start: 0, length: 1 },
        signatures: SIGNATURES_INDEX_OF }
    );
    await driver.completeBackgroundWork();
    driver.receive.signatures({});

    const view = driver.getCodeMirrorView();
    const tooltips = view.state.facet(showTooltip).filter(t => t) as ReadonlyArray<Tooltip>;
    expect(tooltips).toEqual([]);
});

test('Ctrl+Shift+Space requests signature list', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.domEvents.keydown(' ', { ctrlKey: true, shiftKey: true });
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.slice(-1)[0]).toBe('PF');
});