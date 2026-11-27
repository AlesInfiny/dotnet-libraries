using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Maris.Data.Testing.IntegrationTests.Xunit;

/// <summary>
///  指定したテストクラスを、 xUnit のテストフレームワーク（検出から実行まで）で実行します。
/// </summary>
/// <remarks>
///  <see cref="global::Xunit.v3.BeforeAfterTestAttribute"/> の呼び出し順序など、
///  xUnit のパイプラインに依存する動作を確認するために使用します。
/// </remarks>
internal static class XunitPipeline
{
    /// <summary>
    ///  指定したテストクラスのテストを実行し、結果を返します。
    /// </summary>
    /// <param name="testClass">実行するテストクラス。</param>
    /// <returns>テストの実行結果。</returns>
    public static async Task<IReadOnlyList<XunitTestResult>> RunAsync(Type testClass)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var assembly = testClass.Assembly;
        var configuration = new TestAssemblyConfiguration
        {
            ParallelMode = ParallelMode.None,
        };

        await using var framework = new XunitTestFramework();

        var testCases = new List<ITestCase>();
        var discoverer = framework.GetDiscoverer(assembly);
        await discoverer.Find(
            testCase =>
            {
                testCases.Add(testCase);
                return new ValueTask<bool>(true);
            },
            TestFrameworkOptions.ForDiscovery(configuration),
            [testClass],
            cancellationToken);

        if (testCases.Count == 0)
        {
            throw new InvalidOperationException($"{testClass.FullName} にテストが見つかりません。");
        }

        var sink = new ResultCollectingMessageSink();
        var executor = framework.GetExecutor(assembly);
        await executor.RunTestCases(testCases, sink, TestFrameworkOptions.ForExecution(configuration), cancellationToken);

        if (sink.Errors.Count > 0)
        {
            throw new InvalidOperationException($"テストの実行中にエラーが発生しました。{Environment.NewLine}{string.Join(Environment.NewLine, sink.Errors)}");
        }

        return sink.Results;
    }

    /// <summary>
    ///  テストの実行結果を収集するメッセージシンクです。
    /// </summary>
    private sealed class ResultCollectingMessageSink : IMessageSink
    {
        private readonly object syncRoot = new();
        private readonly Dictionary<string, string> displayNames = [];
        private readonly List<XunitTestResult> results = [];
        private readonly List<string> errors = [];

        public IReadOnlyList<XunitTestResult> Results
        {
            get
            {
                lock (this.syncRoot)
                {
                    return [.. this.results];
                }
            }
        }

        public IReadOnlyList<string> Errors
        {
            get
            {
                lock (this.syncRoot)
                {
                    return [.. this.errors];
                }
            }
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            lock (this.syncRoot)
            {
                switch (message)
                {
                    case ITestStarting starting:
                        this.displayNames[starting.TestUniqueID] = starting.TestDisplayName;
                        break;
                    case ITestPassed passed:
                        this.AddResult(passed, XunitTestOutcome.Passed, []);
                        break;
                    case ITestFailed failed:
                        this.AddResult(failed, XunitTestOutcome.Failed, failed.Messages);
                        break;
                    case ITestSkipped skipped:
                        this.AddResult(skipped, XunitTestOutcome.Skipped, [skipped.Reason]);
                        break;
                    case ITestNotRun notRun:
                        this.AddResult(notRun, XunitTestOutcome.NotRun, []);
                        break;
                    case IErrorMessage error:
                        this.errors.Add(string.Join(Environment.NewLine, error.Messages));
                        break;
                    case IErrorMetadata failure:
                        // テストケースやテストクラスのクリーンアップ失敗など、個々のテスト結果以外の失敗。
                        this.errors.Add(string.Join(Environment.NewLine, failure.Messages));
                        break;
                }
            }

            return true;
        }

        private void AddResult(ITestMessage message, XunitTestOutcome outcome, IEnumerable<string?> messages)
        {
            this.displayNames.TryGetValue(message.TestUniqueID, out var displayName);
            this.results.Add(new XunitTestResult(displayName ?? message.TestUniqueID, outcome, [.. messages.OfType<string>()]));
        }
    }
}
