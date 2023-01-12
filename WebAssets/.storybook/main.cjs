const path = require('path');

module.exports = {
    stories: [
        "../src/**/*.stories.ts"
    ],
    addons: [
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
    webpackFinal(config) {;
        const babelIndex = config.module.rules.findIndex(
            r => r.use?.some(u => u.loader?.includes('babel-loader'))
        );
        if (babelIndex < 0)
            throw new Error("Could not find babel-loader rule to replace");

        // It would be great to keep using Babel, but unfortunately
        // Babel has a bug when transpiling to older ES (see _loop missing a param):
        // https://babel.dev/repl#?browsers=ie%2011&build=&builtIns=false&corejs=false&spec=false&loose=false&code_lz=MYGwhgzhAEAa0G9rQFDIA4CcD2wCmUAFAG5ggCuBAlImstMNgHYQAuDI2E5mB0AvNADaAXQDcdZADNsmaIUYt2AazwBPaAEsm0UhWq169RW11lKAs_ohDVa8ZOOduvCADp05CAAtChGvwAfFaUVBJGAL50URFAA&debug=false&forceAllTransforms=false&shippedProposals=false&circleciRepo=&evaluate=false&fileSize=false&timeTravel=false&sourceType=module&lineWrap=true&presets=env%2Creact%2Cstage-2&prettier=false&targets=&version=7.20.12&externalPlugins=&assumptions=%7B%7D
        // It seems almost impossible to target newer ES instead -- settings do not work
        // as expected -- so it is easier to using a different loader.
        config.module.rules[babelIndex] = {
            test: /\.([cm]?ts|tsx)$/,
            loader: "ts-loader",
            options: {
                configFile: path.resolve(__dirname, '../src/tsconfig.storybook.json')
            }
        };
        config.module.rules.push(
            // https://github.com/storybookjs/storybook/issues/15335#issuecomment-1013136904
            { resolve: { fullySpecified: false } }
        );
        return config;
    }
}