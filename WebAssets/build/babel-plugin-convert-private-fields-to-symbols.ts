import type { ClassDeclaration, ClassProperty, Identifier } from '@babel/types';
import type { Visitor, NodePath, types } from '@babel/core';


// WeakMap is an overkill -- using symbols instead
export default ({ types: t }: { types: typeof types }) => {
    const classMap = new WeakMap<ClassDeclaration, { uid: string, symbols: Map<string, Identifier> }>();
    const createSymbol = (path: NodePath, id: Identifier) => {
        const classPath = path.findParent(c => c.isClassDeclaration()) as NodePath<ClassDeclaration>|undefined;
        if (!classPath)
            throw new Error('Unsupported private field outside of a class');

        let classData = classMap.get(classPath.node);
        if (!classData) {
            classData = {
                uid: classPath.scope.generateUidIdentifierBasedOnNode(classPath.node).name,
                symbols: new Map()
            };
            classMap.set(classPath.node, classData);
        }

        const fieldName = id.name;
        let symbolName = classData.symbols.get(fieldName);
        if (!symbolName) {
            symbolName = classPath.scope.generateUidIdentifier(classData.uid + '_' + fieldName);
            classData.symbols.set(fieldName, symbolName);
            classPath.scope.push({
                id: symbolName,
                init: t.callExpression(t.identifier('Symbol'), [
                    t.stringLiteral(`#${fieldName}`)
                ]),
                kind: 'const'
            });
        }
        return symbolName;
    };

    const visitor = {
        ClassPrivateMethod(path) {
            const symbolName = createSymbol(path, path.node.key.id);
            path.replaceWith(t.classMethod(
                'method',
                symbolName,
                path.node.params,
                path.node.body,
                true,
                path.node.static,
                path.node.generator,
                path.node.async
            ));
        },

        PrivateName(path) {
            const symbolName = createSymbol(path, path.node.id);
            const parent = path.parent;
            switch (parent.type) {
                case 'MemberExpression':
                    parent.computed = true;
                    path.replaceWith(symbolName);
                    break;

                case 'ClassPrivateMethod':
                    parent.computed = true;
                    path.replaceWith(symbolName);
                    break;

                case 'ClassPrivateProperty':
                    if (!parent.value) {
                        path.parentPath.remove();
                        break;
                    }

                    path.parentPath.replaceWith({
                        ...parent,
                        type: 'ClassProperty',
                        computed: true,
                        key: symbolName
                    } as ClassProperty);
                    break;

                default:
                    throw new Error(`Unsupported private field context: ${parent.type}`);
            }
        }
    } as Visitor;

    return { visitor };
};