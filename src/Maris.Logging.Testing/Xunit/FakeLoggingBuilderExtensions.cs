using System.Diagnostics.CodeAnalysis;
using Maris.Logging.Testing.Xunit;
using Microsoft.Extensions.Logging.Testing;

namespace Microsoft.Extensions.Logging;

/// <summary>
///  <see cref="TestLoggerManager"/> で使用可能な <see cref="FakeLoggerProvider"/> を登録する処理を提供します。
/// </summary>
[SuppressMessage(
    category: "StyleCop.CSharp.ReadabilityRules",
    checkId: "SA1101:PrefixLocalCallsWithThis",
    Justification = "StyleCop bug. see: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3954")]
public static class FakeLoggingBuilderExtensions
{
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        ///  <see cref="ILoggingBuilder"/> に <see cref="FakeLoggerProvider"/> を追加します。
        /// </summary>
        /// <param name="loggerManager"><see cref="FakeLoggerProvider"/> を管理する <see cref="TestLoggerManager"/> 。</param>
        /// <returns><see cref="ILoggingBuilder"/> 。</returns>
        /// <exception cref="ArgumentNullException">
        ///  <list type="bullet">
        ///   <item><see cref="ILoggingBuilder"/> が <see langword="null"/> です。</item>
        ///   <item><paramref name="loggerManager"/> が <see langword="null"/> です。</item>
        ///  </list>
        /// </exception>
        public ILoggingBuilder AddFakeLogging(TestLoggerManager loggerManager)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (loggerManager is null)
            {
                throw new ArgumentNullException(nameof(loggerManager));
            }

            builder.AddProvider(loggerManager.FakeLoggerProvider);
            return builder;
        }
    }
}
