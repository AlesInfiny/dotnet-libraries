using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Maris.ComponentModel;

/// <summary>
///  任意の <see cref="Enum"/> 型に対する拡張メソッドを提供します。
/// </summary>
public static class EnumExtensions
{
    extension<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        /// <summary>
        ///  列挙型の値に付与された <see cref="DisplayAttribute"/> の名前を取得します。
        ///  属性が存在しない場合は列挙型の名前を返します。
        /// </summary>
        /// <returns>列挙型の表示名。</returns>
        /// <example>
        ///  <para>
        ///   列挙型の定義例と使用例を次に示します。
        ///  </para>
        ///  <code language="csharp">
        ///   <![CDATA[
        ///    using Maris.ComponentModel;
        ///
        ///    public enum Status
        ///    {
        ///        [Display(Name = "Preparation is ready.")]
        ///        Ready = 1,
        ///
        ///        [Display(
        ///            ResourceType = typeof(EnumExtensionsTestResources),
        ///            Name = nameof(EnumExtensionsTestResources.InProgress))]
        ///        InProgress = 2,
        ///
        ///        Done = 3,
        ///    }
        ///
        ///    Console.WriteLine(Status.Ready.GetDisplayName());      // Output: Preparation is ready.
        ///    Console.WriteLine(Status.InProgress.GetDisplayName()); // Output: Work is in progress.
        ///    Console.WriteLine(Status.Done.GetDisplayName());       // Output: Done
        ///   ]]>
        ///  </code>
        /// </example>
        public string GetDisplayName()
        {
            var type = typeof(TEnum);
            var name = Enum.GetName(type, value);
            if (name is null)
            {
                return value.ToString();
            }

            var field = type.GetField(name);
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.GetName() ?? name;
        }
    }
}
