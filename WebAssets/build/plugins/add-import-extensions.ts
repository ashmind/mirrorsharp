import { Node, NodePath, PluginObj, types } from "@babel/core";

const rewrite = <T extends Node & { source?: types.StringLiteral | null }>(
    path: NodePath<T>,
    _new: (source: types.StringLiteral) => T
) => {
    const { source } = path.node;
    if (!source)
        return;

    if (!source.value.startsWith('.'))
        return;

    if (source.value.endsWith('.js'))
        return;

    path.replaceWith(_new(types.stringLiteral(source.value + ".js")));
};

export const addImportExtensions = {
    name: 'add-import-extensions',
    visitor: {
        ImportDeclaration(path) {
            rewrite(path, source => types.importDeclaration(
                path.node.specifiers, source
            ));
        },

        ExportNamedDeclaration(path) {
            rewrite(path, source => types.exportNamedDeclaration(
                path.node.declaration, path.node.specifiers, source
            ));
        },

        ExportAllDeclaration(path) {
            rewrite(path, source => types.exportAllDeclaration(source));
        }
    }
} as PluginObj;