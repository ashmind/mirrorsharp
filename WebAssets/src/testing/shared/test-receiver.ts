import type { ChangeData, ChangesMessage, CompletionItemData, CompletionsMessage, DiagnosticData, InfotipMessage, Message, PartData, SignaturesMessage, UnknownMessage } from '../../protocol/messages';
import type { MockSocketController } from './mock-socket';

export class TestReceiver<TSlowUpdateExtensionData = void> {
    readonly #socket: MockSocketController;

    constructor(socket: MockSocketController) {
        this.#socket = socket;
    }

    changes(reason: ChangesMessage['reason'], changes: ReadonlyArray<ChangeData> = []) {
        this.#message({ type: 'changes', changes, reason });
    }

    optionsEcho(options = {}) {
        this.#message({ type: 'optionsEcho', options });
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

    signatures(message: Omit<SignaturesMessage, 'type'>) {
        this.#message({ type: 'signatures', ...message });
    }

    slowUpdate = ((diagnostics: ReadonlyArray<Partial<DiagnosticData>>, x?: TSlowUpdateExtensionData) => {
        this.#message({
            type: 'slowUpdate',
            diagnostics: diagnostics as Array<DiagnosticData>,
            x
        });
    }) as void extends TSlowUpdateExtensionData
        ? (diagnostics: ReadonlyArray<Partial<DiagnosticData>>) => void
        : (diagnostics: ReadonlyArray<Partial<DiagnosticData>>, x?: TSlowUpdateExtensionData) => void;

    error(message: string) {
        this.#message({ type: 'error', message });
    }

    #message(message: Partial<Exclude<Message<unknown, TSlowUpdateExtensionData>, UnknownMessage>>) {
        this.#socket.receive({ data: JSON.stringify(message) });
    }
}