import { TestDriver } from './test-driver';
import { dispatchMutation } from './helpers/mutation-observer-workaround';
import { completionStatus, currentCompletions, acceptCompletion } from '@codemirror/next/autocomplete';

const ensureCompletionIsReadyForInteraction = () => jest.advanceTimersByTime(100);

const typeCharacterUsingDOM = (driver: TestDriver, character: string) => {
    driver.keys.keydown(character);
    const characterText = document.createTextNode(character);
    const { contentDOM } = driver.getCodeMirrorView();
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
    await driver.completeBackgroundWork();
    ensureCompletionIsReadyForInteraction();
    acceptCompletion(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.slice(-1)[0]).toBe('S0');
});

test('Ctrl+Space requests completion list', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.keys.keydown(' ', { ctrlKey: true });
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

test.each([
    [1, ['class', 'constant', 'delegate', 'enum', 'enummember', 'event', 'extensionmethod']],
    [2, ['field', 'interface', 'keyword', 'local', 'method', 'module', 'namespace']],
    [3, ['parameter', 'property', 'structure', 'typeparameter', 'union']]
])('completion list is rendered correctly (%p)', async (_, kinds) => {
    const driver = await TestDriver.new({ textWithCursor: '|' });

    driver.receive.completions(kinds.map(k => ({
        displayText: k,
        kinds: [k]
    })));
    await driver.completeBackgroundWork();

    const rendered = await driver.render();

    expect(rendered).toMatchImageSnapshot();
});

test('completion is applied on Tab', async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'ToString', kinds: ['method'] }]);
    await driver.completeBackgroundWork();
    ensureCompletionIsReadyForInteraction();

    driver.keys.keydown('Tab');
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
    await driver.completeBackgroundWork();
    ensureCompletionIsReadyForInteraction();

    typeCharacterUsingDOM(driver, '(');
    await driver.completeBackgroundWork();

    driver.receive.changes('completion', [{ start: 0, length: 0, text: 'ToString' }]);
    await driver.completeBackgroundWork();

    const updated = driver.mirrorsharp.getText();
    expect(driver.socket.sent).toContain('S0');
    expect(updated).toBe('ToString(');
});