import { setDiagnostics } from '@codemirror/lint';
import { TestDriver } from '../../testing/test-driver-jest';

// Happens if slow update is received after a text change
// invalidated some diagnostic locations
test('diagnostics outside document boundaries are not applied', async () => {
    const driver = await TestDriver.new({ text: 'test' });

    driver.codeMirrorTransactions = [];
    driver.receive.slowUpdate([
        {
            span: { start: 5, length: 1 },
            severity: 'warning',
            message: 'Test warning'
        },
        {
            span: { start: 1, length: 1 },
            severity: 'error',
            message: 'Test error'
        }
    ]);

    await driver.completeBackgroundWork();

    expect(driver.codeMirrorTransactions).toHaveLength(1);
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const [effect] = driver.codeMirrorTransactions[0]!.effects;
    expect([effect]).toEqual(
        setDiagnostics(driver.getCodeMirrorView().state, [
            {
                from: 1,
                to: 2,
                severity: 'error',
                message: 'Test error'
            }
        ]).effects
    );
});