import * as babel from '@babel/core';
import babelPluginTransformCommonJS from 'babel-plugin-transform-commonjs';

export default async function transform(path: string) {
    const result = await babel.transformFileAsync(path, {
        plugins: [
            {
                visitor: {
                    CallExpression: path => {
                        const callee = path.node.callee;
                        if (!babel.types.isIdentifier(callee) || callee.name !== 'require')
                            return;

                        // inside && or if
                        const parent = path.findParent(p => (p.isLogicalExpression() && p.node.operator === '&&') || p.isIfStatement());
                        if (!parent)
                            return;

                        callee.name = '__conditional_require_unsupported_by_mirrorsharp_browser_tests';
                    }
                }
            },
            babelPluginTransformCommonJS
        ]
    });

    if (!result?.code)
        throw new Error('Failed to compile CommonJS');

    return result.code;
}