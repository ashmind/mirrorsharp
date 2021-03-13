import { parser } from 'lezer-csharp-simple';
import { LezerLanguage } from '@codemirror/language';
import { styleTags, tags } from '@codemirror/highlight';

export const csharpSyntax = LezerLanguage.define({
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