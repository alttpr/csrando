namespace Randomizer;

using System.Reflection;

public static class EnumExtensions
{
    public static T? GetCustomAttribute<T>(this Enum @enum) where T : Attribute
        => @enum.GetType().GetMember(@enum.ToString()).FirstOrDefault()?.GetCustomAttribute<T>(inherit: false);
}
