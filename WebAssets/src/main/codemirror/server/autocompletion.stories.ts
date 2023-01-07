import { testDriverStory } from '../../../testing/storybook/test-driver-story';
import { TestDriver } from '../../../testing/test-driver-storybook';

export default {
    title: 'Autocompletion',
    component: {}
};

const completionListStory = (kinds: ReadonlyArray<string>) => testDriverStory(async () => {
    const driver = await TestDriver.new({ textWithCursor: '|' });

    driver.receive.completions(kinds.map(k => ({
        displayText: k,
        kinds: [k]
    })));
    await driver.completeBackgroundWork();

    return driver;
});

export const Completions1 = completionListStory(['class', 'constant', 'delegate', 'enum', 'enummember', 'event', 'extensionmethod']);
export const Completions2 = completionListStory(['field', 'interface', 'keyword', 'local', 'method', 'module', 'namespace']);
export const Completions3 = completionListStory(['parameter', 'property', 'structure', 'typeparameter', 'union']);

export const Info = testDriverStory(async () => {
    const driver = await TestDriver.new({ text: '' });

    driver.receive.completions([{ displayText: 'CompareTo', kinds: ['method'] }]);

    await driver.ensureCompletionIsReadyForInteraction();

    driver.receive.completionInfo(0, [
        { text: 'int', kind: 'keyword' },
        { text: ' ', kind: 'space' },
        { text: 'int', kind: 'keyword' },
        { text: '.', kind: 'punctuation' },
        { text: 'CompareTo', kind: 'method' },
        { text: '(', kind: 'punctuation' },
        { text: 'int', kind: 'keyword' },
        { text: ' ', kind: 'space' },
        { text: 'value', kind: 'parameter' },
        { text: ')', kind: 'punctuation' },
        { text: ' ', kind: 'space' },
        { text: '(', kind: 'punctuation' },
        { text: '+', kind: 'punctuation' },
        { text: ' 1', kind: 'text' },
        { text: ' overload', kind: 'text' },
        { text: ')', kind: 'punctuation' },
        { text: '\r\n', kind: 'linebreak' },
        { text: 'Compares this instance to a specified 32-bit signed integer and returns an indication of their relative values.', kind: 'text' }
    ]);
    await driver.completeBackgroundWork();

    return driver;
});