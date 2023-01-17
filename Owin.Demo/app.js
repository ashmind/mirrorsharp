import mirrorsharp from 'mirrorsharp-codemirror-6-preview';

const getCode = (language, mode) => {
    if (mode === 'script') {
        return 'var messages = Context.Messages;';
    }
    else if (language == 'C#') {
        return `using System;

        class C {
            const int C2 = 5;
            string f;
            string P { get; set; }
            event EventHandler e;
            event EventHandler E { add {} remove {} }

            C() {
            }

            void M(int p) {
                var l = p;
            }
        }

        class G<T> {
        }`.replace(/(\r\n|\r|\n)/g, '\r\n') // Parcel changes newlines to LF
          .replace(/^        /gm, '');
    }
    else if (language === 'F#') {
        return '[<EntryPoint>]\r\nlet main argv = \r\n    0';
    }
    else if (language === 'IL') {
        return '.class private auto ansi \'<Module>\'\r\n{\r\n}';
    }
}

const getLanguageAndCode = () => {
    const params = window.location.hash.replace(/^\#/, '').split('&').reduce((result, item) => {
        const [key, value] = item.split('=');
        result[key] = value;
        return result;
    }, {});
    const language = (params['language'] || 'CSharp').replace('Sharp', '#');
    const mode = params['mode'] || 'regular';
    const code = getCode(language, mode);

    return { language, mode, code };
}

const initial = getLanguageAndCode();
const ms = mirrorsharp(document.getElementById('editor-container'), {
    serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
    language: initial.language,
    text: initial.code,
    serverOptions: (initial.mode !== 'regular' ? { 'x-mode': initial.mode } : {})
});

window.addEventListener('hashchange', () => {
    const updated = getLanguageAndCode();
    ms.setLanguage(updated.language);
    ms.setServerOptions({ 'x-mode': updated.mode });
    ms.setText(updated.code);
});