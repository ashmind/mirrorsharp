declare module 'keysim' {
    export class Keyboard {
        static readonly US_ENGLISH: Keyboard;

        dispatchEventsForInput(input: string, target: HTMLElement): void;
        dispatchEventsForAction(input: string, target: HTMLElement): void;
    }
}