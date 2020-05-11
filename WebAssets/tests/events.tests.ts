// eslint-disable-next-line @typescript-eslint/no-empty-function
test('_', () => {});

// import { TestDriver } from './test-driver';

// test('slowUpdateWait is triggered on first change', async () => {
//     const slowUpdateWait = jest.fn();
//     const driver = await TestDriver.new({ options: { on: { slowUpdateWait } } });

//     driver.keys.type('x');
//     await driver.completeBackgroundWork();

//     expect(slowUpdateWait.mock.calls).toEqual([[]]);
// });