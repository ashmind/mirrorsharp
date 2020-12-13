import * as babel from '@babel/core';
import babelPluginTransformCommonJS from 'babel-plugin-transform-commonjs';

export default async function transform(path: string) {
    const result = await babel.transformFileAsync(path, {
        plugins: [
            babelPluginTransformCommonJS
        ]
    });

    if (!result?.code)
        throw new Error('Failed to compile CommonJS');

    return result.code;
}