import type { Story } from '@storybook/html';
import type { TestDriver } from '../test-driver-storybook';

// https://github.com/microsoft/TypeScript/issues/49656#issuecomment-1164655820
type BetterOmit<T, K extends keyof T> = { [P in keyof T as Exclude<P, K>]: T[P] }
    & (T extends ((...args: infer TArgs) => infer TResult) ? (...args: TArgs) => TResult : object);
type PlayFunction = NonNullable<Story['play']>;

export type TestDriverStory = BetterOmit<Story, 'play'> & {
    play?: (args: BetterOmit<Parameters<PlayFunction>[0], 'loaded'> & {
        loaded: { driver: TestDriver };
    }) => Promise<void>
};

export const testDriverStory = (asyncStory: () => Promise<TestDriver>) => {
    const story: Story = (_: unknown, { loaded: { driver } }) => {
        return (driver as TestDriver).mirrorsharp.getRootElement();
    };
    story.loaders = [
        async () => ({ driver: await asyncStory() })
    ];
    return story as TestDriverStory;
};