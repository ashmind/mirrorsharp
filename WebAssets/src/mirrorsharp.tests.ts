import { language } from '@codemirror/language';
import { Facet } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { vbLanguage } from './codemirror/languages/vb';
import { THEME_DARK, THEME_LIGHT } from './main/theme';
import type { MirrorSharpDiagnostic, MirrorSharpSlowUpdateResult } from './mirrorsharp';
import { LANGUAGE_CSHARP, LANGUAGE_DEFAULT, LANGUAGE_VB } from './protocol/languages';
import { TestDriver } from './testing/test-driver-jest';

test('configuration text is sent with expected newlines', async () => {
    const driver = await TestDriver.new({
        text: 'line 1\r\nline 2',
        skipSocketOpen: true
    });

    driver.socket.open();
    await driver.completeBackgroundWork();

    expect(driver.socket.sent).toContain(`R0:0:0::line 1\r\nline 2`);
});

test('setText replaces document text', async () => {
    const driver = await TestDriver.new({
        text: 'initial'
    });

    driver.mirrorsharp.setText('updated 1\r\nupdated 2');
    await driver.completeBackgroundWork();

    expect(driver.mirrorsharp.getText()).toBe('updated 1\r\nupdated 2');
    expect(driver.socket.sent).toContain(`R0:7:0::updated 1\r\nupdated 2`);
});

test('setLanguage updates language', async () => {
    const driver = await TestDriver.new({ language: LANGUAGE_CSHARP });

    driver.mirrorsharp.setLanguage(LANGUAGE_VB);

    expect(driver.getCodeMirrorView().state.facet(language))
        .toBe(vbLanguage);
    expect(driver.socket.sent).toContain('Olanguage=Visual Basic');
});

test('setLanguage does not send language if same as default', async () => {
    const driver = await TestDriver.new({});

    driver.mirrorsharp.setLanguage(LANGUAGE_DEFAULT);

    expect(driver.socket.sent).toEqual([]);
});

test('setLanguage does not send language if same as current', async () => {
    const driver = await TestDriver.new({ language: LANGUAGE_VB });

    driver.mirrorsharp.setLanguage(LANGUAGE_VB);

    expect(driver.socket.sent).toEqual([]);
});

test('setTheme updates theme', async () => {
    const driver = await TestDriver.new({ theme: THEME_LIGHT });

    driver.mirrorsharp.setTheme(THEME_DARK);

    expect(driver.getCodeMirrorView().state.facet(EditorView.darkTheme))
        .toBe(true);
    expect(driver.mirrorsharp.getRootElement().classList)
        .toContain('mirrorsharp--theme-dark');
});

test('setServiceUrl connects to new URL', async () => {
    const driver = await TestDriver.new({});
    driver.mirrorsharp.setServiceUrl('new-url');

    await driver.completeBackgroundWork();

    expect(driver.socket.url).toBe('new-url');
    expect(driver.socket.createdCount).toBe(2);
    expect(driver.socket.readyState).toBe(WebSocket.OPEN);
});

test('setServiceUrl does not connect to new URL if disconnected is requested', async () => {
    const driver = await TestDriver.new({});
    driver.mirrorsharp.setServiceUrl('new-url', { disconnected: true });

    await driver.completeBackgroundWork();

    expect(driver.socket.createdCount).toBe(1);
    expect(driver.socket.readyState).toBe(WebSocket.CLOSED);
});

test('connect after disconnected setServiceUrl connects to new URL', async () => {
    const driver = await TestDriver.new({});
    driver.mirrorsharp.setServiceUrl('new-url', { disconnected: true });

    await driver.completeBackgroundWork();
    driver.mirrorsharp.connect();

    expect(driver.socket.url).toBe('new-url');
    expect(driver.socket.createdCount).toBe(2);
    expect(driver.socket.readyState).toBe(WebSocket.OPEN);
});

test('configuration extensions are added to CodeMirror', async () => {
    const facet = Facet.define<string>();

    const driver = await TestDriver.new({
        codeMirror: {
            extensions: [ facet.of('test') ]
        }
    });

    expect(driver.getCodeMirrorView().state.facet(facet))
        .toEqual(['test']);
});

test('textChange is called for setText', async () => {
    const textChange = jest.fn<void, [() => string]>();
    const driver = await TestDriver.new({
        text: 'initial',
        on: { textChange }
    });

    driver.mirrorsharp.setText('updated');
    await driver.completeBackgroundWork();

    expect(textChange).toBeCalledTimes(1);
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(textChange.mock.calls[0]![0]()).toBe('updated');
});

test('textChange is called when typing', async () => {
    const textChange = jest.fn<void, [() => string]>();
    const driver = await TestDriver.new({
        textWithCursor: 'initial|',
        on: { textChange }
    });

    driver.text.type('test');
    await driver.completeBackgroundWork();

    expect(textChange).toBeCalledTimes('test'.length);
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    expect(textChange.mock.calls[0]![0]()).toBe('initialtest');
});

test('connectionChange is called when immediate connection is opened', async () => {
    const connectionChange = jest.fn<void, [string]>();
    await TestDriver.new({ on: { connectionChange } });

    expect(connectionChange.mock.calls).toEqual([['open']]);
});

test('connectionChange is called when delayed connection is opened', async () => {
    const connectionChange = jest.fn<void, [string]>();
    const driver = await TestDriver.new({ on: { connectionChange }, disconnected: true });

    // eslint-disable-next-line no-debugger
    driver.mirrorsharp.connect();
    await driver.completeBackgroundWork();

    expect(connectionChange.mock.calls).toEqual([['open']]);
});

test('connectionChange is called when connection is lost', async () => {
    const connectionChange = jest.fn<void, [string]>();
    const driver = await TestDriver.new({ on: { connectionChange } });

    driver.socket.close();

    expect(connectionChange.mock.calls).toEqual([
        ['open'],
        ['loss']
    ]);
});

test('slowUpdateWait is called while waiting for slow update result', async () => {
    const slowUpdateWait = jest.fn<void, []>();
    const driver = await TestDriver.new<void, string>({
        text: '_',
        on: { slowUpdateWait }
    });

    await driver.advanceTimeToSlowUpdateAndCompleteWork();

    // Expecting two calls: one on open and one on timer
    expect(slowUpdateWait).toBeCalledTimes(2);
});

test('slowUpdateResult is called with results of slow update', async () => {
    const slowUpdateResult = jest.fn<void, [MirrorSharpSlowUpdateResult<string>]>();
    const diagnostics = [{
        id: 'test-id',
        severity: 'error',
        message: 'test-message'
    }] as const satisfies ReadonlyArray<Partial<MirrorSharpDiagnostic>>;
    const driver = await TestDriver.new<void, string>({
        on: { slowUpdateResult }
    });

    driver.receive.slowUpdate(diagnostics.map(({ id, severity, message }) => ({
        id, severity, message,
        span: { start: 0, length: 0 },
        tags: []
    })), 'test-x-result');

    expect(slowUpdateResult.mock.calls).toEqual([
        [{ diagnostics, extensionResult: 'test-x-result' }]
    ]);
});

test('serverError is called when receiving an error', async () => {
    const serverError = jest.fn<void, [string]>();
    const driver = await TestDriver.new({ on: { serverError } });

    driver.receive.error('test-error');

    expect(serverError.mock.calls).toEqual([['test-error']]);
});