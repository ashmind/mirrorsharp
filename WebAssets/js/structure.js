/* global module:false, define:false */
(function (root, factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        define([
          'codemirror',
          'codemirror-addon-lint-fix',
          'codemirror/addon/hint/show-hint',
          'codemirror/mode/clike/clike',
          'codemirror/mode/vb/vb',
          'codemirror/mode/php/php'
        ], factory);
    }
    else if (typeof module === 'object' && module.exports) {
        module.exports = factory(
          require('codemirror'),
          require('codemirror-addon-lint-fix'),
          require('codemirror/addon/hint/show-hint'),
          require('codemirror/mode/clike/clike'),
          require('codemirror/mode/vb/vb')
        );
    }
    else {
        root.mirrorsharp = factory(root.CodeMirror);
    }
})(this, function (CodeMirror) { // eslint-disable-line no-unused-vars
    'use strict';

    // include: ./assign.js
    // include: ./self-debug.js
    // include: ./connection.js
    // include: ./hinter.js
    // include: ./signature-tip.js
    // include: ./infotip-renderer.js
    // include: ./editor.js
    // include: ./add-events.js
    // include: ./mirrorsharp.js

    return mirrorsharp; // eslint-disable-line no-undef
});