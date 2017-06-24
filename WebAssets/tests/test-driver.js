const nextTickPromise = require('./next-tick-promise.js');
const mirrorsharp = require('../mirrorsharp.js');
const Keysim = require('keysim');

const keyboard = Keysim.Keyboard.US_ENGLISH;

class MockSocket {
    constructor() {
        this.sent = [];
    }

    send(message) {
        this.sent.push(message);
    }

    addEventListener(event, handler) {
        if (event === 'open')
            process.nextTick(() => handler());
    }
}

class MockTextRange {
    getBoundingClientRect() {}
    getClientRects() { return []; }
}
global.document.body.createTextRange = () => new MockTextRange();

class TestTyper {
    constructor(input) {
        this.input = input;
    }

    text(text) {
        keyboard.dispatchEventsForInput(text, this.input);
    }

    backspace(count) {
        for (let i = 0; i < count; i++) {
            keyboard.dispatchEventsForAction('backspace', this.input);
        }
    }
}

class TestDriver {
    getCodeMirror() {
        return this.cm;
    }
}

TestDriver.new = async options => {
    const driver = new TestDriver();
    const initial = getInitialState(options);

    const initialTextarea = document.createElement('textarea');
    initialTextarea.value = initial.text || '';
    document.body.appendChild(initialTextarea);

    const socket = new MockSocket();
    driver.socket = socket;
    global.WebSocket = function() { return socket; };

    driver.mirrorsharp = mirrorsharp(initialTextarea, {});

    delete global.WebSocket;

    await nextTickPromise();
    const cm = driver.mirrorsharp.getCodeMirror();
    driver.cm = cm;
    if (initial.cursor)
        cm.setCursor(cm.posFromIndex(initial.cursor));

    const input = cm.getWrapperElement().querySelector('textarea');
    driver.type = new TestTyper(input);
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