import { LRLanguage } from '@codemirror/language';
import { styleTags, tags } from '@lezer/highlight';
import { parser } from 'lezer-csharp-simple';

const csharpSyntax = LRLanguage.define({
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

export const csharp = csharpSyntax.extension;