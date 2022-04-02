import mirrorsharp from 'mirrorsharp';

const params = window.location.search.replace(/^\?/, '').split('&').reduce(function (o, item) {
    const parts = item.split('=');
    o[parts[0]] = parts[1];
    return o;
}, {});
const language = (params['language'] || 'CSharp').replace('Sharp', '#');
const mode = params['mode'] || 'regular';

const textarea = document.getElementsByTagName('textarea')[0];
if (language === 'F#') {
    textarea.value = '[<EntryPoint>]\r\nlet main argv = \r\n    0';
}
else if (mode === 'script') {
    textarea.value = 'var messages = Context.Messages;';
}
else if (language === 'IL') {
    textarea.value = '.class private auto ansi \'<Module>\'\r\n{\r\n}';
}


const ms = mirrorsharp(textarea, {
    serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
    selfDebugEnabled: true,
    language: language
});
if (mode !== 'regular')
    ms.setServerOptions({ 'language': language, 'x-mode': mode });