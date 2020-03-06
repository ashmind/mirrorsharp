import type { CompletionItemData } from '../ts/interfaces/protocol';
import { TestDriver } from './test-driver';

// TODO: remove in year 3000 when TC39 finally specs this
// eslint-disable-next-line no-extend-native
(Array.prototype as any).last = (Array.prototype as any).last || function<T>(this: Array<T>) { return this[this.length - 1]; };
declare global {
    interface Array<T> {
        last(): T;
    }
}

test('opening hints requests info for the first hint', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion()
    ]);
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('SI0');
});

test('selecting hint requests info for the selected hint', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion(),
        completion(),
        completion()
    ]);
    await driver.completeBackgroundWork();
    driver.keys.press('down');
    driver.keys.press('down');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('SI2');
});

test('picking hint cancels info request', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([
        completion(),
        completion()
    ]);
    await driver.completeBackgroundWork();
    driver.keys.press('down');
    driver.keys.press('tab');
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.last();
    expect(lastSent).toBe('S1');
});

test('completionDescription message shows info tip', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    driver.receive.completions([completion()]);
    await driver.completeBackgroundWork();
    driver.receive.completionInfo(0, [
        { kind: 'type',  text: 'int' },
        { kind: 'space', text: ' ' },
        { kind: 'local', text: 'x' },
    ]);
    await driver.completeBackgroundWork();

    const tip = getTooltip();
    expect(tip.style.display).toBe('block');
    expect(tip.innerHTML).toBe([
        '<span class="cm-type">int</span>',
        '<span class="cm-space"> </span>',
        '<span class="cm-local">x</span>'
    ].join(''));
});


test('completionDescription message updates info tip if already existed', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    await driver.completeBackgroundWorkAfterEach(
        () => driver.receive.completions([completion(), completion()]),
        () => driver.receive.completionInfo(0, [{ kind: 'test', text: 'old' }]),
        () => driver.keys.press('down'),
        () => driver.receive.completionInfo(1, [{ kind: 'test', text: 'new' }]),
    );

    const tip = getTooltip();
    expect(tip.style.display).toBe('block');
    expect(tip.innerHTML).toBe('<span class="cm-test">new</span>');
});

test('picking hint hides info tip', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });

    await driver.completeBackgroundWorkAfterEach(
        () => driver.receive.completions([completion()]),
        () => driver.receive.completionInfo(0, []),
        () => driver.keys.press('tab')
    );

    const tip = getTooltip();
    expect(tip.style.display).toBe('none');
});

test('info tip has expected position and size', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'c.|' });
    const mockRect = (e: Element, rect: Partial<DOMRect>) => e.getBoundingClientRect = () => rect as DOMRect;

    driver.receive.completions([completion()]);
    await driver.completeBackgroundWork();
    mockRect(document.querySelector('.CodeMirror-hints')!, { right: 150 });
    mockRect(document.querySelector('.CodeMirror-hints .CodeMirror-hint:first-child')!, { top: 300 });
    mockRect(document.documentElement, { width: 200 });

    driver.receive.completionInfo(0, []);
    await driver.completeBackgroundWork();

    const tip = getTooltip();
    expect(tip.style.top).toBe('300px');
    expect(tip.style.left).toBe('150px');
    expect(tip.style.maxWidth).toBe('50px');
});

function completion() {
    return { kinds: [] } as Partial<CompletionItemData> as CompletionItemData;
}

function getTooltip() {
    return document.querySelector('.mirrorsharp-hint-info-tooltip') as HTMLDivElement;
}