using System.Diagnostics.CodeAnalysis;
using Maris.Logging.Testing.Xunit;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///  <see cref="TestLoggerManager"/> で管理できる <see cref="ILogger"/> を登録する処理を提供します。
/// </summary>
[SuppressMessage(
    category: "StyleCop.CSharp.ReadabilityRules",
    checkId: "SA1101:PrefixLocalCallsWithThis",
    Justification = "StyleCop bug. see: https://github.com/DotNetAnalyzers/StyleCopAnalyzers/issues/3954")]
public static class TestLoggerServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///  テストで使用するログ出力の構成をします。
        /// </summary>
        /// <param name="loggerManager">Xunit のテストクラスで使用する <see cref="TestLoggerManager"/> 。</param>
        /// <returns><see cref="IServiceCollection"/> 。</returns>
        /// <exception cref="ArgumentNullException">
        ///  <list type="bullet">
        ///   <item><see cref="IServiceCollection"/> が <see langword="null"/> です。</item>
        ///   <item><paramref name="loggerManager"/> が <see langword="null"/> です。</item>
        ///  </list>
        /// </exception>
        public IServiceCollection AddTestLogging(TestLoggerManager loggerManager)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (loggerManager is null)
            {
                throw new ArgumentNullException(nameof(loggerManager));
            }

            services.AddLogging(builder =>
            {
                builder.AddXunitLogging(loggerManager);
                builder.AddFakeLogging(loggerManager);
            });

            return services;
        }
    }
}
