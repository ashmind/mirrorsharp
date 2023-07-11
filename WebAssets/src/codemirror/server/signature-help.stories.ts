import { storyWithDarkTheme } from '../../testing/storybook/story-with-dark-theme';
import { testDriverStory } from '../../testing/storybook/test-driver-story';
import { TestDriver } from '../../testing/test-driver-storybook';
import { SIGNATURES_INDEX_OF, SIGNATURES_SUBSTRING_NONE_SELECTED, SIGNATURES_SUBSTRING_SECOND_PARAMETER } from './signature-help.test.data';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'Signature Help',
    component: {}
};

export const Simple = testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '"x".IndexOf(|' });

    driver.receive.signatures(SIGNATURES_INDEX_OF, {
        start: 0, length: 12
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const SecondSelected = testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '"x".Substring(1, 2|' });

    driver.receive.signatures(SIGNATURES_SUBSTRING_SECOND_PARAMETER, {
        start: 0, length: 14
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const NoneSelected = testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '"x".Substring(1, 2, |' });
    driver.receive.signatures(SIGNATURES_SUBSTRING_NONE_SELECTED, {
        start: 0, length: 16
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const Dark = storyWithDarkTheme(Simple);