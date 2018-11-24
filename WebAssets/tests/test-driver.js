const mirrorsharp = require('../dist/mirrorsharp.js');
const Keysim = require('keysim');

jest.useFakeTimers();
(() => {
    // clean JSDOM between tests
    const emptyHTML = document.body.innerHTML;
    afterEach(() => document.body.innerHTML = emptyHTML);
})();

const keyboard = Keysim.Keyboard.US_ENGLISH;

const spliceString = (string, start, length, newString = '') =>
    string.substring(0, start) + newString + string.substring(start + length);

class MockSocket {
    constructor() {
        this.sent = [];
        this.handlers = {};
    }

    send(message) {
        this.sent.push(message);
    }

    trigger(event, e) {
        for (const handler of (this.handlers[event] || [])) {
            handler(e);
        }
    }

    addEventListener(event, handler) {
        (this.handlers[event] = this.handlers[event] || []).push(handler);
    }
}

class MockTextRange {
    getBoundingClientRect() {}
    getClientRects() { return []; }
}
global.document.body.createTextRange = () => new MockTextRange();

class TestKeys {
    constructor(input, getCursor) {
        this.input = input;
        this.getCursor = getCursor;
    }

    type(text) {
        const input = this.input;
        input.focus();
        input.value = spliceString(input.value, this.getCursor(), 0, text);
        keyboard.dispatchEventsForInput(text, input);
    }

    backspace(count) {
        const input = this.input;
        for (let i = 0; i < count; i++) {
            input.value = spliceString(input.value, this.getCursor() - 1, 1);
            keyboard.dispatchEventsForAction('backspace', this.input);
        }
    }

    press(keys) {
        keyboard.dispatchEventsForAction(keys, this.input);
    }
}

class TestReceiver {
    constructor(socket) {
        this.socket = socket;
    }

    changes(changes = [], reason = '') {
        this.socket.trigger('message', { data: JSON.stringify({type: 'changes', changes, reason}) });
    }

    optionsEcho(options = {}) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'optionsEcho', options}) });
    }

    completions(completions = [], { span = {}, commitChars = null, suggestion = null } = {}) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'completions', completions, span, commitChars, suggestion}) });
    }

    completionInfo(index, parts) {
        this.socket.trigger('message', { data: JSON.stringify({type: 'completionInfo', index, parts}) });
    }
}

class TestDriver {
    getCodeMirror() {
        return this.cm;
    }

    async completeBackgroundWork() {
        jest.runOnlyPendingTimers();
        await new Promise(resolve => resolve());
        jest.runOnlyPendingTimers();
    }

    async completeBackgroundWorkAfterEach(...actions) {
        for (const action of actions) {
            action();
            await this.completeBackgroundWork();
        }
    }
}

TestDriver.new = async (options = {}) => {
    const driver = new TestDriver();
    const initial = getInitialState(options);

    const initialTextarea = document.createElement('textarea');
    initialTextarea.value = initial.text || '';
    document.body.appendChild(initialTextarea);

    const socket = new MockSocket();
    driver.socket = socket;
    global.WebSocket = function() { return socket; };

    driver.mirrorsharp = mirrorsharp(initialTextarea, options.options || {});

    delete global.WebSocket;

    const cm = driver.mirrorsharp.getCodeMirror();
    driver.cm = cm;
    if (initial.cursor)
        cm.setCursor(cm.posFromIndex(initial.cursor));

    driver.socket.trigger('open');
    await driver.completeBackgroundWork();
    const input = cm.getWrapperElement().querySelector('textarea');
    driver.keys = new TestKeys(input, () => cm.indexFromPos(cm.getCursor()));
    driver.receive = new TestReceiver(socket);

    jest.runOnlyPendingTimers();
    driver.socket.sent = [];
    return driver;
};

function getInitialState(options) {
    let {text, cursor} = options;
    if (options.textWithCursor) {
        text = options.textWithCursor.replace('|', '');
        cursor = options.textWithCursor.indexOf('|');
    }
    return {text, cursor};
}

module.exports = TestDriver;