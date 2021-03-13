import { tags } from '@codemirror/next/highlight';

// temporary, until CodeMirror 6 implements CSS classes in highlighter
export default [
    { tag: tags.keyword, color: '#0000ff' },
    { tag: tags.number, color: '#000000' },
    { tag: tags.string, color: '#a31515' },
    { tag: tags.comment, color: '#008000' }
];