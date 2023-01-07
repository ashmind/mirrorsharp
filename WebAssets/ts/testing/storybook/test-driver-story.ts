import type { Story } from '@storybook/html';
import type { TestDriver } from '../test-driver-storybook';

export const testDriverStory = (story: () => Promise<TestDriver>) => {
    const syncStory: Story = (_: unknown, { loaded: { driver } }) => {
        return (driver as TestDriver).mirrorsharp.getRootElement();
    };
    syncStory.loaders = [
        async () => ({ driver: await story() })
    ];
    return syncStory;
};