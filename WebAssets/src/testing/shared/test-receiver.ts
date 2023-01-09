import type { ChangeData, ChangesMessage, CompletionItemData, CompletionsMessage, DiagnosticData, InfotipMessage, Message, PartData, SignaturesMessage, UnknownMessage } from '../../interfaces/protocol';
import type { MockSocketController } from './mock-socket';

export class TestReceiver {
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

    slowUpdate(diagnostics: ReadonlyArray<DiagnosticData>, x?: unknown) {
        this.#message({
            type: 'slowUpdate',
            diagnostics,
            x
        });
    }

    #message = (message: Partial<Exclude<Message<unknown, unknown>, UnknownMessage>>) => {
        this.#socket.receive({ data: JSON.stringify(message) });
    };
}