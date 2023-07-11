const { dirname } = require('path');
const { getStoryContext } = require('@storybook/test-runner');
const { toMatchImageSnapshot } = require('jest-image-snapshot');

/** @type {import('@storybook/test-runner').TestRunnerConfig} */
const config = {
    setup() {
        expect.extend({ toMatchImageSnapshot });
    },

    async postRender(page, context) {
        const { id } = context;
        const fileName = (await getStoryContext(page, context)).parameters?.fileName;
        if (!fileName) throw new Error('File name not set for the story.');

        const image = await page.screenshot({ animations: 'disabled' });

        expect(image).toMatchImageSnapshot({
            customSnapshotsDir: `${dirname(fileName)}/__image_snapshots__`,
            customSnapshotIdentifier: id
        });
    }
};

module.exports = config;