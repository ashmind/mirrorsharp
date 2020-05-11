import { parser } from 'lezer-csharp-simple';
import { LezerSyntax } from '@codemirror/next/syntax';
import { languageData } from '@codemirror/next/state';
import { styleTags } from '@codemirror/next/highlight';

export const csharpSyntax = new LezerSyntax(parser.withProps(
    languageData.add({
        Script: {
            closeBrackets: { brackets: ['(', '[', '{', "'", '"', '`'] },
            commentTokens: { line: '//', block: { open: '/*', close: '*/' } }
        }
    }),
    styleTags({
        Keyword: 'keyword',
        Comment: 'comment',
        Number: 'number',
        String: 'string',
        Punctuation: 'punctuation'
    })
));

/// Returns an extension that installs the JavaScript syntax provider.
export function csharp() {
    return csharpSyntax.extension;
}