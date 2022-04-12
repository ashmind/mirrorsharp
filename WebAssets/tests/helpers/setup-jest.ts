import './real-timers'; // ensures real timers are captured before fake ones are set up
import { toMatchImageSnapshot } from 'jest-image-snapshot';

expect.extend({ toMatchImageSnapshot });

jest.useFakeTimers('modern');
jest.setTimeout(3 * 60 * 1000);