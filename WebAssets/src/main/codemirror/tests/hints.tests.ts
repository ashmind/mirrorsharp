// eslint-disable-next-line @typescript-eslint/no-empty-function
test('_', () => {});

// import type { CompletionItemData } from '../ts/interfaces/protocol';
// import { TestDriver } from './test-driver';

// test('opening hints requests info for the first hint', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     driver.receive.completions([
//         completion()
//     ]);
//     await driver.completeBackgroundWork();

//     const lastSent = driver.socket.sent.slice(-1)[0];
//     expect(lastSent).toBe('SI0');
// });

// test('selecting hint requests info for the selected hint', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     driver.receive.completions([
//         completion(),
//         completion(),
//         completion()
//     ]);
//     await driver.completeBackgroundWork();
//     driver.keys.press('down');
//     driver.keys.press('down');
//     await driver.completeBackgroundWork();

//     const lastSent = driver.socket.sent.slice(-1)[0];
//     expect(lastSent).toBe('SI2');
// });

// test('picking hint cancels info request', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     driver.receive.completions([
//         completion(),
//         completion()
//     ]);
//     await driver.completeBackgroundWork();
//     driver.keys.press('down');
//     driver.keys.press('tab');
//     await driver.completeBackgroundWork();

//     const lastSent = driver.socket.sent.slice(-1)[0];
//     expect(lastSent).toBe('S1');
// });

// test('completionDescription message shows info tip', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     driver.receive.completions([completion()]);
//     await driver.completeBackgroundWork();
//     driver.receive.completionInfo(0, [
//         { kind: 'type',  text: 'int' },
//         { kind: 'space', text: ' ' },
//         { kind: 'local', text: 'x' }
//     ]);
//     await driver.completeBackgroundWork();

//     const tip = getTooltip();
//     expect(tip.style.display).toBe('block');
//     expect(tip.innerHTML).toBe([
//         '<span class="cm-type">int</span>',
//         '<span class="cm-space"> </span>',
//         '<span class="cm-local">x</span>'
//     ].join(''));
// });


// test('completionDescription message updates info tip if already existed', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     await driver.completeBackgroundWorkAfterEach(
//         () => driver.receive.completions([completion(), completion()]),
//         () => driver.receive.completionInfo(0, [{ kind: 'test', text: 'old' }]),
//         () => driver.keys.press('down'),
//         () => driver.receive.completionInfo(1, [{ kind: 'test', text: 'new' }])
//     );

//     const tip = getTooltip();
//     expect(tip.style.display).toBe('block');
//     expect(tip.innerHTML).toBe('<span class="cm-test">new</span>');
// });

// test('picking hint hides info tip', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });

//     await driver.completeBackgroundWorkAfterEach(
//         () => driver.receive.completions([completion()]),
//         () => driver.receive.completionInfo(0, []),
//         () => driver.keys.press('tab')
//     );

//     const tip = getTooltip();
//     expect(tip.style.display).toBe('none');
// });

// test('info tip has expected position and size', async () => {
//     const driver = await TestDriver.new({ textWithCursor: 'c.|' });
//     const mockRect = (e: Element, rect: Partial<DOMRect>) => e.getBoundingClientRect = () => rect as DOMRect;

//     driver.receive.completions([completion()]);
//     await driver.completeBackgroundWork();

//     const hints = document.querySelector('.CodeMirror-hints') as HTMLElement;
//     const selected = document.querySelector('.CodeMirror-hints .CodeMirror-hint:first-child')!;

//     Object.defineProperty(hints, 'offsetTop', { value: 50 });
//     mockRect(hints, { top: 100, right: 150 });
//     mockRect(selected, { top: 300 });
//     mockRect(document.documentElement, { width: 200 });

//     driver.receive.completionInfo(0, []);
//     await driver.completeBackgroundWork();

//     const tip = getTooltip();

//     // 300 (boundingRect top) - 100 (parent boundingRect top) = 200
//     // 200 + 50 (parent offsetTop) = 250
//     expect(tip.style.top).toBe('250px');

//     expect(tip.style.left).toBe('150px');
//     expect(tip.style.maxWidth).toBe('50px');
// });

// function completion() {
//     return { kinds: [] } as Partial<CompletionItemData> as CompletionItemData;
// }

// function getTooltip() {
//     return document.querySelector('.mirrorsharp-hint-info-tooltip') as HTMLDivElement;
// }