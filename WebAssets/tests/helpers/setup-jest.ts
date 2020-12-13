import { toMatchImageSnapshot } from 'jest-image-snapshot';

expect.extend({ toMatchImageSnapshot });

jest.useFakeTimers('modern');
jest.setTimeout(3 * 60 * 1000);