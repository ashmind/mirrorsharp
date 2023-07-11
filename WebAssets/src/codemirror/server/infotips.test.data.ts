export const INFOTIP_EVENTHANDLER = {
    kinds: ['delegate', 'public'],
    sections: [
        {
            kind: 'description',
            parts: [
                { text: 'delegate', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'void', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'System', kind: 'namespace' },
                { text: '.', kind: 'punctuation' },
                { text: 'EventHandler', kind: 'delegate' },
                { text: '(', kind: 'punctuation' },
                { text: 'object', kind: 'keyword' },
                { text: ' ', kind: 'space' },
                { text: 'sender', kind: 'parameter' },
                { text: ',', kind: 'punctuation' },
                { text: ' ', kind: 'space' },
                { text: 'System', kind: 'namespace' },
                { text: '.', kind: 'punctuation' },
                { text: 'EventArgs', kind: 'class' },
                { text: ' ', kind: 'space' },
                { text: 'e', kind: 'parameter' },
                { text: ')', kind: 'punctuation' }
            ]
        },
        {
            kind: 'documentationcomments',
            parts: [
                { text: 'Represents the method that will handle an event that has no event data.', kind: 'text' }
            ]
        }
    ],
    span: { start: 0, length: 0 }
};

export const INFOTIP_TASK_RUN = {
    kinds: [
        'method',
        'public'
    ],
    sections: [
        {
            kind: 'description',
            parts: [
                {
                    text: '(',
                    kind: 'punctuation'
                },
                {
                    text: 'awaitable',
                    kind: 'text'
                },
                {
                    text: ')',
                    kind: 'punctuation'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'Task',
                    kind: 'class'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'Task',
                    kind: 'class'
                },
                {
                    text: '.',
                    kind: 'punctuation'
                },
                {
                    text: 'Run',
                    kind: 'method'
                },
                {
                    text: '(',
                    kind: 'punctuation'
                },
                {
                    text: 'System',
                    kind: 'namespace'
                },
                {
                    text: '.',
                    kind: 'punctuation'
                },
                {
                    text: 'Action',
                    kind: 'delegate'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'action',
                    kind: 'parameter'
                },
                {
                    text: ')',
                    kind: 'punctuation'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: '(',
                    kind: 'punctuation'
                },
                {
                    text: '+',
                    kind: 'punctuation'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: '7',
                    kind: 'text'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'overloads',
                    kind: 'text'
                },
                {
                    text: ')',
                    kind: 'punctuation'
                }
            ]
        },
        {
            kind: 'documentationcomments',
            parts: [
                {
                    text: 'Queues the specified work to run on the thread pool and returns a',
                    kind: 'text'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'Task',
                    kind: 'class'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'object that represents that work.',
                    kind: 'text'
                }
            ]
        },
        {
            kind: 'usage',
            parts: [
                {
                    text: '\r\n',
                    kind: 'linebreak'
                },
                {
                    text: 'Usage:',
                    kind: 'text'
                },
                {
                    text: '\r\n',
                    kind: 'linebreak'
                },
                {
                    text: '  ',
                    kind: 'text'
                },
                {
                    text: 'await',
                    kind: 'keyword'
                },
                {
                    text: ' ',
                    kind: 'space'
                },
                {
                    text: 'Run',
                    kind: 'method'
                },
                {
                    text: '(',
                    kind: 'punctuation'
                },
                {
                    text: '...',
                    kind: 'punctuation'
                },
                {
                    text: ')',
                    kind: 'punctuation'
                },
                {
                    text: ';',
                    kind: 'punctuation'
                }
            ]
        },
        {
            kind: 'exception',
            parts: [
                {
                    text: '\r\n',
                    kind: 'linebreak'
                },
                {
                    text: 'Exceptions:',
                    kind: 'text'
                },
                {
                    text: '\r\n',
                    kind: 'linebreak'
                },
                {
                    text: '  ',
                    kind: 'space'
                },
                {
                    text: 'System',
                    kind: 'namespace'
                },
                {
                    text: '.',
                    kind: 'punctuation'
                },
                {
                    text: 'ArgumentNullException',
                    kind: 'class'
                }
            ]
        }
    ],
    span: {
        start: 17,
        length: 1
    }
};