import { action } from '@storybook/addon-actions';
import { MockSocket } from '../shared/mock-socket';

export class MockSocketWithActionLog extends MockSocket {
    constructor() {
        super();
        this.addEventListener(
            'message', e => action('receive')(JSON.parse(e.data))
        );
    }

    override send(message: string): void {
        action('send')(message);
        super.send(message);
    }
}