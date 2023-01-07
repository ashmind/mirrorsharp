module.exports = {
    stories: [
        "../src/**/*.stories.ts"
    ],
    addons: [
        "@storybook/addon-links",
        "@storybook/addon-viewport",
        "@storybook/addon-measure",
        "@storybook/addon-outline"
    ],
    core: {
        builder: 'webpack5'
    },
    features: {
        buildStoriesJson: true
    }
}