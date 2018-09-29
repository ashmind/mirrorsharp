/* globals SelfDebug:false, Connection:false, Editor:false */

function mirrorsharp(textarea, options) {
    const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
    const connection = new Connection(options.serviceUrl, selfDebug);
    const editor = new Editor(textarea, connection, selfDebug, options);
    const exports = {};
    for (var key in editor) {
        exports[key] = editor[key].bind(editor);
    }
    exports.destroy = function(destroyOptions) {
        editor.destroy(destroyOptions);
        connection.close();
    };
    return exports;
}

/* exported mirrorsharp */