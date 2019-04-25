/* globals SelfDebug:false, Connection:false, Editor:false */

/**
 * @param {HTMLTextAreaElement} textarea
 * @param {internal.Options} options
 * @returns {public.Instance}
 */
function mirrorsharp(textarea, options) {
    const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
    const connection = new Connection(options.serviceUrl, selfDebug);
    const editor = new Editor(textarea, connection, selfDebug, options);
    /** @type {object} */
    const exports = {};
    for (var key in editor) {
        // @ts-ignore
        exports[key] = editor[key].bind(editor);
    }
    /** @param {public.DestroyOptions} destroyOptions */
    exports.destroy = function(destroyOptions) {
        editor.destroy(destroyOptions);
        connection.close();
    };
    return exports;
}

/* exported mirrorsharp */