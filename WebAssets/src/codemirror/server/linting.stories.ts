import { userEvent as user } from '@storybook/testing-library';
import { Theme, THEME_DARK, THEME_LIGHT } from '../../interfaces/theme';
import { testDriverStory } from '../../testing/storybook/test-driver-story';
import { TestDriver } from '../../testing/test-driver-storybook';

// eslint-disable-next-line import/no-default-export
export default {
    title: 'Linting',
    component: {}
};

const GutterTooltipTemplate = (theme: Theme = THEME_LIGHT) => {
    const Story = testDriverStory(async () => {
        const driver = await TestDriver.new({ text: 'class X : EventArgs {}', theme });
        driver.receive.slowUpdate([
            {
                id: 'CS0246',
                message: "The type or namespace name 'EventArgs' could not be found (are you missing a using directive or an assembly reference?)",
                severity: 'error',
                tags: [],
                span: {
                    start: 10,
                    length: 9
                },
                actions: [
                    {
                        id: 0,
                        title: 'using System;'
                    },
                    {
                        id: 1,
                        title: "Generate class 'EventArgs'"
                    },
                    {
                        id: 2,
                        title: 'System.EventArgs'
                    }
                ]
            },
            {
                id: 'IDE0040',
                message: 'Accessibility modifiers required',
                severity: 'hidden',
                tags: [],
                span: {
                    start: 6,
                    length: 1
                },
                actions: [
                    {
                        id: 3,
                        title: 'Add accessibility modifiers'
                    }
                ]
            }
        ]);
        await driver.completeBackgroundWork();

        return driver;
    });

    Story.play = async ({ canvasElement, loaded: { driver } }) => {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const gutterMarker = canvasElement.querySelector('.cm-gutter-lint .cm-lint-marker')!;

        user.hover(gutterMarker);
        await driver.advanceTimeToHoverAndCompleteWork();

        driver.disableAllFurtherInteractionEvents();
    };

    return Story;
};

export const GutterTooltip = GutterTooltipTemplate();
export const GutterTooltipDark = GutterTooltipTemplate(THEME_DARK);