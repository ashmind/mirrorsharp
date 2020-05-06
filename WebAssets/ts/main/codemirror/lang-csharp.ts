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
      //'get set async static': 'modifier',
      Keyword: 'keyword',
      Comment: 'comment',
      //in of await yield void typeof delete instanceof': 'operatorKeyword',
      //'export import let var const function class extends': 'keyword definition',
      //'with debugger from as': 'keyword',
      //TemplateString: 'string#2',
      //'BooleanLiteral Super': 'atom',
      //this: 'self',
      //null: 'null',
      //   VariableName: 'variableName',
      //   VariableDefinition: 'variableName definition',
      //   Label: 'labelName',
      //   PropertyName: 'propertyName',
      //   PropertyNameDefinition: 'propertyName definition',
      //   'PostfixOp UpdateOp': 'updateOperator',
      //   LineComment: 'lineComment',
      //   BlockComment: 'blockComment',
      Number: 'number',
      String: 'string',
      // ArithOp: 'arithmeticOperator',
      // LogicOp: 'logicOperator',
      // BitOp: 'bitwiseOperator',
      // CompareOp: 'compareOperator',
      // RegExp: 'regexp',
      // Equals: 'operator definition',
      // Spread: 'punctuation',
      // 'Arrow :': 'punctuation definition',
      //   '( )': 'paren',
      //   '[ ]': 'squareBracket',
      //   '{ }': 'brace',
      //   '.': 'derefOperator',
      //   ', ;': 'separator'
      Punctuation: 'punctuation'
  })
));

/// Returns an extension that installs the JavaScript syntax provider.
export function csharp() {
    return csharpSyntax.extension;
}