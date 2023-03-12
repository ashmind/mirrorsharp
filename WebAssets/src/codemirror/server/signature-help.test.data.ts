import type { SignatureData } from '../../protocol/messages';

export const SIGNATURES_INDEX_OF: ReadonlyArray<SignatureData> = [
    {
        selected: true,
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'char',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ],
        info: {
            parts: [
                {
                    text: 'Reports the zero-based index of the first occurrence of the specified Unicode character in this string.',
                    kind: 'text'
                }
            ],
            parameter: {
                name: 'value',
                parts: [
                    {
                        text: 'A Unicode character to seek.',
                        kind: 'text'
                    }
                ]
            }
        }
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'char',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'char',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
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
                text: 'StringComparison',
                kind: 'enum'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'comparisonType',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
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
                text: 'StringComparison',
                kind: 'enum'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'comparisonType',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'char',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'count',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'count',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
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
                text: 'StringComparison',
                kind: 'enum'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'comparisonType',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'IndexOf',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'string',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'value',
                kind: 'parameter',
                selected: true
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'count',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
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
                text: 'StringComparison',
                kind: 'enum'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'comparisonType',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    }
];

export const SIGNATURES_SUBSTRING_SECOND_PARAMETER: ReadonlyArray<SignatureData> = [
    {
        parts: [
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'Substring',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        selected: true,
        parts: [
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'Substring',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword',
                selected: true
            },
            {
                text: ' ',
                kind: 'space',
                selected: true
            },
            {
                text: 'length',
                kind: 'parameter',
                selected: true
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ],
        info: {
            parts: [
                {
                    text: 'Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.',
                    kind: 'text'
                }
            ],
            parameter: {
                name: 'length',
                parts: [
                    {
                        text: 'The number of characters in the substring.',
                        kind: 'text'
                    }
                ]
            }
        }
    }
];

export const SIGNATURES_SUBSTRING_NONE_SELECTED: ReadonlyArray<SignatureData> = [
    {
        parts: [
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'Substring',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    },
    {
        parts: [
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'string',
                kind: 'keyword'
            },
            {
                text: '.',
                kind: 'punctuation'
            },
            {
                text: 'Substring',
                kind: 'method'
            },
            {
                text: '(',
                kind: 'punctuation'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'startIndex',
                kind: 'parameter'
            },
            {
                text: ',',
                kind: 'punctuation'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'int',
                kind: 'keyword'
            },
            {
                text: ' ',
                kind: 'space'
            },
            {
                text: 'length',
                kind: 'parameter'
            },
            {
                text: ')',
                kind: 'punctuation'
            }
        ]
    }
];