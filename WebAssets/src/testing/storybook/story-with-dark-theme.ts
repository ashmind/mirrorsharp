import { THEME_DARK } from '../../interfaces/theme';
import { TestDriver } from '../test-driver-storybook';
import type { TestDriverStory } from './test-driver-story';

export const storyWithDarkTheme = (story: TestDriverStory) => {
    const darkStory: TestDriverStory = (...args) => story(...args);
    if (story.loaders?.length !== 1)
        throw new Error('Unsupported loader count');
    darkStory.loaders = [
        async (...args) => {
            TestDriver.nextTheme = THEME_DARK;
            // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
            const result = await story.loaders![0]!(...args);
            TestDriver.nextTheme = null;
            return result;
        }
    ];
    darkStory.play = story.play;
    return darkStory;
};