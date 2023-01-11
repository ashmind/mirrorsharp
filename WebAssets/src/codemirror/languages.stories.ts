import { Language, LANGUAGE_CSHARP, LANGUAGE_FSHARP, LANGUAGE_IL, LANGUAGE_PHP, LANGUAGE_VB } from '../protocol/languages';
import { storyWithDarkTheme } from '../testing/storybook/story-with-dark-theme';
import { testDriverStory } from '../testing/storybook/test-driver-story';
import { TestDriver } from '../testing/test-driver-storybook';
import { CODE_CSHARP, CODE_FSHARP, CODE_IL, CODE_PHP, CODE_VB } from './languages/test.data';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'Languages',
    component: {}
};

const highlightingStory = (language: Language, text: string) => testDriverStory(async () => {
    const driver = await TestDriver.new({
        options: { language }, text
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const CSharp = highlightingStory(LANGUAGE_CSHARP, CODE_CSHARP);
export const CSharpDark = storyWithDarkTheme(CSharp);
export const VisualBasic = highlightingStory(LANGUAGE_VB, CODE_VB);
export const VisualBasicDark = storyWithDarkTheme(VisualBasic);
export const FSharp = highlightingStory(LANGUAGE_FSHARP, CODE_FSHARP);
export const FSharpDark = storyWithDarkTheme(FSharp);
export const IL = highlightingStory(LANGUAGE_IL, CODE_IL);
export const ILDark = storyWithDarkTheme(IL);
export const PHP = highlightingStory(LANGUAGE_PHP, CODE_PHP);
export const PHPDark = storyWithDarkTheme(PHP);