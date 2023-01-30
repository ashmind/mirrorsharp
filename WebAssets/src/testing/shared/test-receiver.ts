import type { ChangeData, ChangesMessage, CompletionItemData, CompletionsMessage, DiagnosticData, InfotipMessage, Message, PartData, ServerOptions, ServerPosition, SignatureData, SignaturesEmptyMessage, SignaturesMessage, SpanData, UnknownMessage } from '../../protocol/messages';
import type { MockSocketController } from './mock-socket';

type SimplifyServerPosition<T> = {
    [P in keyof T]:
        T[P] extends ServerPosition ? (T[P] | number) :
            (T[P] extends object ? SimplifyServerPosition<T[P]> :
                (T[P] extends ReadonlyArray<infer U> ? ReadonlyArray<SimplifyServerPosition<U>> : T[P]));
};

export class TestReceiver<TExtensionServerOptions, TSlowUpdateExtensionData> {
    readonly #socket: MockSocketController;

    constructor(socket: MockSocketController) {
        this.#socket = socket;
    }

    changes(reason: ChangesMessage['reason'], changes: ReadonlyArray<ChangeData> = []) {
        this.#message({ type: 'changes', changes, reason });
    }

    optionsEcho(options: Partial<ServerOptions> & Partial<TExtensionServerOptions> = {}) {
        this.#message({ type: 'optionsEcho', options: options as (ServerOptions & TExtensionServerOptions) });
    }

    /**
     *
    readonly span: SpanData;
    readonly kinds: ReadonlyArray<string>;
    readonly sections: ReadonlyArray<InfotipSectionData>;
     */
    infotip(args: Omit<InfotipMessage, 'type'>) {
        this.#message({ type: 'infotip', ...args });
    }

    completions(
        completions: ReadonlyArray<CompletionItemData> = [],
        other: Partial<Omit<CompletionsMessage, 'completions'|'type'>> = {}
    ) {
        this.#message({ type: 'completions', completions, ...other });
    }

    completionInfo(index: number, parts: ReadonlyArray<PartData>) {
        this.#message({ type: 'completionInfo', index, parts });
    }

    signatures(signatures?: ReadonlyArray<SignatureData>, span?: SimplifyServerPosition<SpanData>) {
        this.#message({ type: 'signatures', signatures, span } as SignaturesMessage | SignaturesEmptyMessage);
    }

    slowUpdate = ((diagnostics: ReadonlyArray<Partial<SimplifyServerPosition<DiagnosticData>>>, x?: TSlowUpdateExtensionData) => {
        this.#message({
            type: 'slowUpdate',
            diagnostics: diagnostics
                .map(d => ({ tags: [], ...d })) as Array<DiagnosticData>,
            x
        });
    }) as void extends TSlowUpdateExtensionData
        ? (diagnostics: ReadonlyArray<Partial<SimplifyServerPosition<DiagnosticData>>>) => void
        : (diagnostics: ReadonlyArray<Partial<SimplifyServerPosition<DiagnosticData>>>, x?: TSlowUpdateExtensionData) => void;

    error(message: string) {
        this.#message({ type: 'error', message });
    }

    #message(message: Partial<Exclude<Message<TExtensionServerOptions, TSlowUpdateExtensionData>, UnknownMessage>>) {
        this.#socket.receive({ data: JSON.stringify(message) });
    }
}