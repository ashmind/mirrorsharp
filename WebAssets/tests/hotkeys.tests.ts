// eslint-disable-next-line @typescript-eslint/no-empty-function
test('_', () => {});

// import { TestDriver } from './test-driver';

// test('Tab indents selected block', async () => {
//     const text = multiline(`
//     ┊abc
//     ┊def
//     ┊fgh
//     `);
//     const driver = await TestDriver.new({ text });

//     driver.keys.press('ctrl+a');
//     driver.keys.press('tab');

//     await driver.completeBackgroundWork();

//     expect(driver.mirrorsharp.getText()).toEqual(multiline(`
//     ┊    abc
//     ┊    def
//     ┊    fgh
//     `));
// });

// test('Shift+Tab un-indents selected block', async () => {
//     const text = multiline(`
//     ┊    abc
//     ┊    def
//     ┊    fgh
//     `);
//     const driver = await TestDriver.new({ text });

//     driver.keys.press('ctrl+a');
//     driver.keys.press('shift+tab');

//     await driver.completeBackgroundWork();

//     expect(driver.mirrorsharp.getText()).toEqual(multiline(`
//     ┊abc
//     ┊def
//     ┊fgh
//     `));
// });

// function multiline(string: string) {
//     return string
//         .replace(/ *┊/g, '')
//         .replace(/[\r\n]*\s*$|^\s*[\r\n]+/g, '')
//         .replace(/\r?\n/g, '\r\n');
// }