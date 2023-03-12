import { storyWithDarkTheme } from '../testing/storybook/story-with-dark-theme';
import { testDriverStory } from '../testing/storybook/test-driver-story';
import { TestDriver } from '../testing/test-driver-storybook';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'Connection Loss',
    component: {}
};

export const ConnectionLost = testDriverStory(async () => {
    const driver = await TestDriver.new({
        text: '// Example code'
    });
    driver.socket.close();
    return driver;
});
export const ConnectionLostDark = storyWithDarkTheme(ConnectionLost);