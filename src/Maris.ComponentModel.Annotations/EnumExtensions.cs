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
        ///  列挙体の値に付与された <see cref="DisplayAttribute"/> の名前を取得します。
        ///  属性が存在しない場合は列挙体の名前を返します。
        /// </summary>
        /// <returns>Display 名または Enum 名。</returns>
        /// <example>
        ///  <code language="csharp">
        ///   <![CDATA[
        ///    aaa;
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
