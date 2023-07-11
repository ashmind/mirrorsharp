import { userEvent as user } from '@storybook/testing-library';
import { lineSeparator } from '../../protocol/line-separator';
import { storyWithDarkTheme } from '../../testing/storybook/story-with-dark-theme';
import { testDriverStory } from '../../testing/storybook/test-driver-story';
import { TestDriver } from '../../testing/test-driver-storybook';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'Diagnostics',
    component: {}
};

export const All = testDriverStory(async () => {
    const driver = await TestDriver.new({
        text: ['info', 'warning', 'error', 'hidden', 'unnecessary'].join(lineSeparator)
    });

    driver.receive.slowUpdate([
        { id: 'I1', message: 'info', severity: 'info', tags: [], span: { start: 0, length: 4 } },
        { id: 'W1', message: 'warning', severity: 'warning', tags: [], span: { start: 6, length: 7 } },
        { id: 'E1', message: 'error', severity: 'error', tags: [], span: { start: 15, length: 5 } },
        { id: 'H1', message: 'hidden', severity: 'hidden', tags: [], span: { start: 22, length: 6 } },
        { id: 'U1', message: 'unnecessary', severity: 'info', tags: ['unnecessary'], span: { start: 30, length: 11 } }
    ]);
    await driver.completeBackgroundWork();

    return driver;
});
export const AllDark = storyWithDarkTheme(All);

export const GutterTooltip = testDriverStory(async () => {
    const driver = await TestDriver.new({ text: 'class X : EventArgs {}' });
    driver.receive.slowUpdate([
        {
            id: 'CS0246',
            message: "The type or namespace name 'EventArgs' could not be found (are you missing a using directive or an assembly reference?)",
            severity: 'error',
            tags: [],
            span: { start: 10, length: 9 },
            actions: [
                { id: 0, title: 'using System;' },
                { id: 1, title: "Generate class 'EventArgs'" },
                { id: 2, title: 'System.EventArgs' }
            ]
        },
        {
            id: 'IDE0040',
            message: 'Accessibility modifiers required',
            severity: 'hidden',
            tags: [],
            span: { start: 6, length: 1 },
            actions: [{ id: 3, title: 'Add accessibility modifiers' }]
        }
    ]);
    await driver.completeBackgroundWork();

    return driver;
});
GutterTooltip.play = async ({ canvasElement, loaded: { driver } }) => {
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const gutterMarker = canvasElement.querySelector('.cm-gutter-lint .cm-lint-marker')!;

    user.hover(gutterMarker);
    await driver.advanceTimeToHoverAndCompleteWork();

    driver.disableAllFurtherInteractionEvents();
};
export const GutterTooltipDark = storyWithDarkTheme(GutterTooltip);