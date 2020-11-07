import { TestDriver } from './test-driver';
import { completionStatus, currentCompletions, acceptCompletion } from '@codemirror/next/autocomplete';

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
    jest.advanceTimersByTime(100);
    acceptCompletion(driver.getCodeMirrorView());
    await driver.completeBackgroundWork();

    expect(driver.socket.sent.slice(-1)[0]).toBe('S0');
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
        { displayText: 'bc', kinds: [] }
    ]);
    await driver.completeBackgroundWork();

    const state = driver.getCodeMirrorView().state;
    expect(currentCompletions(state).map(c => c.label)).toEqual(['bb', 'bc']);
});

test('completion list is filtered based on new typed text', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|' });

    driver.receive.completions([
        { displayText: 'aaa', kinds: [] },
        { displayText: 'aba', kinds: [] },
        { displayText: 'abb', kinds: [] }
    ]);
    await driver.completeBackgroundWork();
    driver.text.type('b');
    await driver.completeBackgroundWork();

    const state = driver.getCodeMirrorView().state;
    expect(currentCompletions(state).map(c => c.label)).toEqual(['aba', 'abb']);
});