using System.Diagnostics.CodeAnalysis;
using Maris.Logging.Testing.Xunit;

namespace Microsoft.Extensions.Logging;

/// <summary>
///  Xunit で使用可能な <see cref="ILogger"/> を登録する処理を提供します。
/// </summary>
[SuppressMessage(
    category: "StyleCop.CSharp.ReadabilityRules",
    checkId: "SA1101:PrefixLocalCallsWithThis",
    Justification = "StyleCop bug. see: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3954")]
public static class XunitLoggingBuilderExtensions
{
    extension(ILoggingBuilder builder)
    {
        /// <summary>
        ///  <see cref="ILoggingBuilder"/> に Xunit の <see cref="ILoggerProvider"/> を追加します。
        /// </summary>
        /// <param name="loggerManager">Xunit で利用できる <see cref="ILogger"/> を管理する <see cref="TestLoggerManager"/> 。</param>
        /// <returns><see cref="ILoggingBuilder"/> 。</returns>
        /// <exception cref="ArgumentNullException">
        ///  <list type="bullet">
        ///   <item><see cref="ILoggingBuilder"/> が <see langword="null"/> です。</item>
        ///   <item><paramref name="loggerManager"/> が <see langword="null"/> です。</item>
        ///  </list>
        /// </exception>
        public ILoggingBuilder AddXunitLogging(TestLoggerManager loggerManager)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (loggerManager is null)
            {
                throw new ArgumentNullException(nameof(loggerManager));
            }

            builder.AddProvider(loggerManager.XunitLoggerProvider);
            return builder;
        }
    }
}
