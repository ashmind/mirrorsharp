declare module CodeMirror {
    export interface Editor {
        setOption(key: 'infotip', value: InfotipOptions): void;
        infotipUpdate(args: { data: any, range: { from: Pos, to: Pos } }): void;
    }

    interface EditorConfiguration {
        infotip?: AsyncInfotipOptions;
    }

    type InfotipOptions = AsyncInfotipOptions;

    export interface AsyncInfotipOptions {
        async: true;
        delay: number;
        getInfo: (cm: Editor, position: CodeMirror.Pos) => void;
        render: (parent: HTMLElement, data: any) => void;
    }
}