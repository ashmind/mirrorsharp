// WeakMap is an overkill -- using symbols instead
/** @param {{types: import("@babel/types")}} _ */
export default ({ types: t }) => {
    /** @type {WeakMap<import("@babel/types").ClassDeclaration, { uid: string, symbols: Map<string, import("@babel/types").Identifier> }>} */
    const classMap = new WeakMap();

    /** @type {import("@babel/core").Visitor} */
    const visitor = {
        PrivateName(path) {
            const classPath = /** @type {import("@babel/core").NodePath<import("@babel/types").ClassDeclaration>?} */(
                path.findParent(c => c.isClassDeclaration())
            );
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

            const fieldName = path.node.id.name;
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

            const parent = path.parent;
            switch (parent.type) {
                case 'MemberExpression':
                    parent.computed = true;
                    path.replaceWith(symbolName);
                    break;

                case 'ClassPrivateProperty':
                    if (!parent.value) {
                        path.parentPath.remove();
                        break;
                    }

                    // @ts-ignore
                    path.parentPath.replaceWith({
                        ...parent,
                        type: 'ClassProperty',
                        computed: true,
                        key: symbolName
                    });
                    break;

                default:
                    throw new Error(`Unsupported private field context: ${parent.type}`);
            }
        }
    };

    return { visitor };
};