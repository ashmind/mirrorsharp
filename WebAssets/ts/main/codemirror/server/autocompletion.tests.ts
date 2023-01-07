import { TestDriver } from '../../../testing/test-driver-jest';
import { dispatchMutation } from '../../../testing/helpers/mutation-observer-workaround';
import { completionStatus, currentCompletions, acceptCompletion, moveCompletionSelection } from '@codemirror/autocomplete';

const typeCharacterUsingDOM = (driver: TestDriver, character: string) => {
    driver.domEvents.keydown(character);
    const characterText = document.createTextNode(character);
    const { contentDOM } = driver.getCodeMirrorView();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    contentDOM.querySelector<HTMLElement>('.cm-line')!.appendChild(characterText);
    dispatchMutation(contentDOM, {
        type: 'characterData' as MutationRecordType,
        target: characterText
    });
};

test('completions message shows completion list', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'Test', kinds: ['class'] }]);
    await driver.completeBackgroundWork();

    const state = driver.getCodeMirrorView().state;
    expect(completionStatus(state)).toBe('active');
    expect(currentCompletions(state)).toMatchObject([{
        label: 'Test',
        type: 'class'
    }]);
});

test('applying completion sends expected message', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'Test', kinds: ['method'] }]);
    await driver.ensureCompletionIsReadyForInteraction();
    acceptCompletion(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.slice(-1)[0]).toBe('S0');
});

test('Ctrl+Space requests completion list', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.domEvents.keydown(' ', { ctrlKey: true });
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.slice(-1)[0]).toBe('SF');
});

test('completion change is applied correctly', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'x.|' });

    driver.receive.changes('completion', [{ start: 2, length: 0, text: 'ToString();' }]);
    await driver.completeBackgroundWork();

    const updated = driver.mirrorsharp.getText();
    expect(updated).toBe('x.ToString();');
});

test.each([
    ['x.|', 'x.|', 'a', 'x.a|'],
    ['x.|', 'x.|', 'ab', 'x.ab|'],
    ['x.|', '|x.', 'a', '|x.a'],
    ['x.|', 'x.y|', 'a', 'x.ay|']
])('completion change adjust selection (initial: %p, while waiting: %p, change: %p, expected: %p)', async (
    initialText,
    textWhileWaiting,
    completionText,
    expectedText
) => {
    const driver = await TestDriver.new({ textWithCursor: initialText });
    const initialCursorOffset = driver.mirrorsharp.getCursorOffset();
    driver.setTextWithCursor(textWhileWaiting);

    driver.receive.changes('completion', [{
        start: initialCursorOffset,
        length: 0,
        text: completionText
    }]);
    await driver.completeBackgroundWork();

    expect(driver.getTextWithCursor()).toBe(expectedText);
});

test('completion list is filtered based on initial text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'b|' });

    driver.receive.completions([
        { displayText: 'ab', kinds: [] },
        { displayText: 'bb', kinds: [] },
        { displayText: 'BC', kinds: [] }
    ]);
    await driver.completeBackgroundWork();

    const state = driver.getCodeMirrorView().state;
    expect(currentCompletions(state).map(c => c.label)).toEqual(['bb', 'BC']);
});

test('completion list is filtered based on new typed text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|' });

    driver.receive.completions([
        { displayText: 'aaa', kinds: [] },
        { displayText: 'aba', kinds: [] },
        { displayText: 'ABB', kinds: [] }
    ]);
    await driver.completeBackgroundWork();
    driver.text.type('b');
    await driver.completeBackgroundWork();

    const state = driver.getCodeMirrorView().state;
    expect(currentCompletions(state).map(c => c.label)).toEqual(['aba', 'ABB']);
});

test('completion is applied on Tab', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'ToString', kinds: ['method'] }]);
    await driver.ensureCompletionIsReadyForInteraction();

    driver.domEvents.keydown('Tab');
    await driver.completeBackgroundWork();

    const text = driver.mirrorsharp.getText();
    expect(text).toBe(''); // completion response not received, so text not changed yet
    expect(driver.socket.sent.slice(-1)[0]).toBe('S0');
});

test('completion is applied on (', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'ToString', kinds: ['method'] }], {
        commitChars: '(;'
    });
    await driver.ensureCompletionIsReadyForInteraction();

    typeCharacterUsingDOM(driver, '(');
    await driver.completeBackgroundWork();

    driver.receive.changes('completion', [{ start: 0, length: 0, text: 'ToString' }]);
    await driver.completeBackgroundWork();

    const updated = driver.mirrorsharp.getText();
    expect(driver.socket.sent).toContain('S0');
    expect(updated).toBe('ToString(');
});

test('completion requests info when open', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'ToString', kinds: ['method'] }]);
    await driver.ensureCompletionIsReadyForInteraction();

    expect(driver.socket.sent).toContain('SI0');
});

test('completion requests info when selected', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([
        { displayText: 'Method0', kinds: ['method'] },
        { displayText: 'Method1', kinds: ['method'] }
    ]);
    await driver.ensureCompletionIsReadyForInteraction();

    moveCompletionSelection(true, 'option')(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();

    expect(driver.socket.sent[driver.socket.sent.length - 1]).toBe('SI1');
});

test('completion does not request same info twice', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([
        { displayText: 'Method0', kinds: ['method'] },
        { displayText: 'Method1', kinds: ['method'] }
    ]);
    await driver.ensureCompletionIsReadyForInteraction();

    // -> 0
    moveCompletionSelection(true, 'option')(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();
    // 0 -> 1
    moveCompletionSelection(true, 'option')(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();
    // 1 -> 0
    moveCompletionSelection(false, 'option')(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toMatchObject([
        'SI0',
        'SI1'
    ]);
});

test('completion applies requested info', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'ToString', kinds: ['method'] }]);
    await driver.ensureCompletionIsReadyForInteraction();

    driver.receive.completionInfo(0, [
        { text: 'ToString', kind: 'method' },
        { text: '\r\n', kind: 'linebreak' },
        { text: 'Converts the value of this instance to its equivalent string representation.', kind: 'text' }
    ]);
    await driver.completeBackgroundWork();

    const tooltip = driver.getCodeMirrorView().dom.querySelector('.cm-completionInfo');
    expect(tooltip).not.toBeNull();
    expect(tooltip?.innerHTML).toMatchSnapshot();
});