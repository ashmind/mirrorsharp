import { Diagnostic, setDiagnostics } from '@codemirror/lint';
import { TestDriver } from '../../testing/test-driver-jest';

const expectSetDiagnostics = (driver: TestDriver, diagnostics: ReadonlyArray<Partial<Diagnostic>>) => {
    expect(driver.codeMirrorTransactions).toHaveLength(1);
    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    const [effect] = driver.codeMirrorTransactions[0]!.effects;
    expect([effect]).toMatchObject(
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        setDiagnostics(driver.getCodeMirrorView().state, diagnostics as ReadonlyArray<Diagnostic>).effects!
    );
};

test('diagnostics are positioned correctly after newlines', async () => {
    const driver = await TestDriver.new({ text: 'a\r\nb' });

    driver.codeMirrorTransactions = [];
    driver.receive.slowUpdate([{
        span: { start: 3, length: 1 }
    }]);

    await driver.completeBackgroundWork();

    expectSetDiagnostics(driver, [{
        from: 2,
        to: 3
    }]);
});

test('diagnostics at the end of document are applied', async () => {
    const driver = await TestDriver.new({ text: 'test' });

    driver.codeMirrorTransactions = [];
    driver.receive.slowUpdate([{
        span: { start: 3, length: 1 }
    }]);

    await driver.completeBackgroundWork();

    expectSetDiagnostics(driver, [{
        from: 3,
        to: 4
    }]);
});

// Happens if slow update is received after a text change
// invalidated some diagnostic locations
test('diagnostics outside document boundaries are not applied', async () => {
    const driver = await TestDriver.new({ text: 'test' });

    driver.codeMirrorTransactions = [];
    driver.receive.slowUpdate([{
        span: { start: 5, length: 1 }
    }]);

    await driver.completeBackgroundWork();

    expectSetDiagnostics(driver, []);
});