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

    driver.receive.signatures({
        span: { start: 0, length: 12 },
        signatures: SIGNATURES_INDEX_OF
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const SecondSelected = testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '"x".Substring(1, 2|' });

    driver.receive.signatures({
        span: { start: 0, length: 14 },
        signatures: SIGNATURES_SUBSTRING_SECOND_PARAMETER
    });
    await driver.completeBackgroundWork();

    return driver;
});

export const NoneSelected = testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '"x".Substring(1, 2, |' });
    driver.receive.signatures({
        span: { start: 0, length: 16 },
        signatures: SIGNATURES_SUBSTRING_NONE_SELECTED
    });
    await driver.completeBackgroundWork();

    return driver;
});