module.exports = {
    stories: [
        "../src/**/*.stories.ts"
    ],
    addons: [
        "@storybook/addon-links",
        "@storybook/addon-viewport",
        "@storybook/addon-measure",
        "@storybook/addon-outline",
        "@storybook/addon-actions",
        "@storybook/addon-interactions"
    ],
    core: {
        builder: 'webpack5'
    },
    features: {
        buildStoriesJson: true
    },
    webpackFinal(config) {
        // https://github.com/storybookjs/storybook/issues/15335#issuecomment-1013136904
        config.module.rules.push({
            resolve: { fullySpecified: false },
        })
        return config;
    }
}