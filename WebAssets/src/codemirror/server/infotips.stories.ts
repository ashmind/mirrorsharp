import { userEvent as user, within } from '@storybook/testing-library';
import { testDriverStory } from '../../testing/storybook/test-driver-story';
import { TestDriver } from '../../testing/test-driver-storybook';
import { INFOTIP_EVENTHANDLER } from './infotips.test.data';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'QuickInfo',
    component: {}
};

export const Simple = testDriverStory(async () => {
    const driver = await TestDriver.new({ text: 'EventHandler e;' });
    await driver.completeBackgroundWork();
    return driver;
});
Simple.play = async ({ canvasElement, loaded: { driver } }) => {
    const canvas = within(canvasElement);

    const code = await canvas.findByText('EventHandler', { exact: false });
    const cmView = driver.getCodeMirrorView();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const coords = cmView.coordsAtPos(cmView.posAtDOM(code, 0))!;
    user.hover(code, {
        clientX: Math.ceil(coords.left),
        clientY: Math.ceil(coords.top)
    });

    await driver.advanceTimeToHoverAndCompleteWork();
    driver.receive.infotip(INFOTIP_EVENTHANDLER);

    await driver.completeBackgroundWork();
    driver.disableAllFurtherInteractionEvents();
};