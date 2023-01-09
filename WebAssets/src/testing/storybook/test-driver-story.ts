import type { Story } from '@storybook/html';
import type { TestDriver } from '../test-driver-storybook';

// https://github.com/microsoft/TypeScript/issues/49656#issuecomment-1164655820
type BetterOmit<T, K extends keyof T> =  { [P in keyof T as Exclude<P, K>]: T[P] };
type PlayFunction = NonNullable<Story['play']>;

type TestDriverStory = BetterOmit<Story, 'play'> & {
    play?: (args: BetterOmit<Parameters<PlayFunction>[0], 'loaded'> & {
        loaded: { driver: TestDriver };
    }) => Promise<void>
};

export const testDriverStory = (story: () => Promise<TestDriver>) => {
    const syncStory: Story = (_: unknown, { loaded: { driver } }) => {
        return (driver as TestDriver).mirrorsharp.getRootElement();
    };
    syncStory.loaders = [
        async () => ({ driver: await story() })
    ];
    return syncStory as TestDriverStory;
};