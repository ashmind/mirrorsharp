import { parser } from 'lezer-csharp-simple';
import { LRLanguage } from '@codemirror/language';
import { styleTags, tags } from '@lezer/highlight';

export const csharpSyntax = LRLanguage.define({
    parser: parser.configure({
        props: [styleTags({
            Keyword: tags.keyword,
            Comment: tags.comment,
            Number: tags.number,
            String: tags.string,
            Punctuation: tags.punctuation
        })]
    }),
    languageData: {
        closeBrackets: { brackets: ['(', '[', '{', "'", '"', '`'] },
        commentTokens: { line: '//', block: { open: '/*', close: '*/' } }
    }
});

/// Returns an extension that installs the JavaScript syntax provider.
export function csharp() {
    return csharpSyntax.extension;
}