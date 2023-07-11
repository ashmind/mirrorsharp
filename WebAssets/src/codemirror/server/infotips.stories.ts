import { userEvent as user, within } from '@storybook/testing-library';
import { storyWithDarkTheme } from '../../testing/storybook/story-with-dark-theme';
import { testDriverStory } from '../../testing/storybook/test-driver-story';
import { TestDriver } from '../../testing/test-driver-storybook';
import { INFOTIP_TASK_RUN } from './infotips.test.data';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'QuickInfo',
    component: {}
};

export const Simple = testDriverStory(() => TestDriver.new({ text: 'Task M() => Task.Run(() => {});' }));
Simple.play = async ({ canvasElement, loaded: { driver } }) => {
    const canvas = within(canvasElement);

    const code = await canvas.findByText('Run', { exact: false });
    const cmView = driver.getCodeMirrorView();
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const coords = cmView.coordsAtPos(INFOTIP_TASK_RUN.span.start)!;
    user.hover(code, {
        clientX: Math.ceil(coords.left),
        clientY: Math.ceil(coords.top)
    });

    await driver.advanceTimeToHoverAndCompleteWork();
    driver.receive.infotip(INFOTIP_TASK_RUN);

    await driver.completeBackgroundWork();
    driver.disableAllFurtherInteractionEvents();
};

export const Dark = storyWithDarkTheme(Simple);