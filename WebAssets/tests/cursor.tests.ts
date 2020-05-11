import { TestDriver } from './test-driver';
import { EditorSelection, SelectionRange } from '@codemirror/next/state';

test('cursor move is sent to server', async () => {
    const driver = await TestDriver.new({ textWithCursor: 'a|bc' });

    driver.dispatchCodeMirrorTransaction(t => t.setSelection(
        EditorSelection.create([ new SelectionRange(2) ])
    ));
    await driver.completeBackgroundWork();

    const lastSent = driver.socket.sent.filter(c => !c.startsWith('U')).slice(-1)[0];
    expect(lastSent).toBe('M2');
});