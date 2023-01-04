import { phpLanguage } from '@codemirror/lang-php';
import { styleTags, tags } from '@lezer/highlight';

export const php = phpLanguage.configure({
    props: [styleTags({
        Keyword: tags.keyword,
        Comment: tags.comment,
        Number: tags.number,
        String: tags.string,
        Punctuation: tags.punctuation
    })]
}).extension;