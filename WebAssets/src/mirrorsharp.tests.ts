import { language } from '@codemirror/language';
import { Facet } from '@codemirror/state';
import { EditorView } from '@codemirror/view';
import { vbLanguage } from './codemirror/languages/vb';
import { THEME_DARK, THEME_LIGHT } from './interfaces/theme';
import { LANGUAGE_CSHARP, LANGUAGE_VB } from './protocol/languages';
import { TestDriver } from './testing/test-driver-jest';

test('setText replaces document text', async () => {
    const driver = await TestDriver.new({
        text: 'initial'
    });

    driver.mirrorsharp.setText('updated');
    await driver.completeBackgroundWork();

    expect(driver.mirrorsharp.getText()).toBe('updated');
    expect(driver.socket.sent).toContain(`R0:7:0::updated`);
});

test('setLanguage updates language', async () => {
    const driver = await TestDriver.new({ language: LANGUAGE_CSHARP });

    driver.mirrorsharp.setLanguage(LANGUAGE_VB);

    expect(driver.getCodeMirrorView().state.facet(language))
        .toBe(vbLanguage);
    expect(driver.socket.sent).toContain('Olanguage=Visual Basic');
});

test('setTheme updates theme', async () => {
    const driver = await TestDriver.new({ theme: THEME_LIGHT });

    driver.mirrorsharp.setTheme(THEME_DARK);

    expect(driver.getCodeMirrorView().state.facet(EditorView.darkTheme))
        .toBe(true);
    expect(driver.mirrorsharp.getRootElement().classList)
        .toContain('mirrorsharp-theme-dark');
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