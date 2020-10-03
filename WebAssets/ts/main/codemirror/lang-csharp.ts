import { parser } from 'lezer-csharp-simple';
import { LezerSyntax } from '@codemirror/next/syntax';
import { styleTags } from '@codemirror/next/highlight';

export const csharpSyntax = LezerSyntax.define(
    parser.withProps(
        styleTags({
            Keyword: 'keyword',
            Comment: 'comment',
            Number: 'number',
            String: 'string',
            Punctuation: 'punctuation'
        })
    ),
    {
        languageData: {
            closeBrackets: { brackets: ['(', '[', '{', "'", '"', '`'] },
            commentTokens: { line: '//', block: { open: '/*', close: '*/' } }
        }
    }
);

/// Returns an extension that installs the JavaScript syntax provider.
export function csharp() {
    return csharpSyntax.extension;
}