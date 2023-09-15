namespace RandomizerTests.Utils;

using System.Reflection;

public class TestUtils
{
    public static string GetLogicTestDisplayNames(MethodInfo methodInfo, object[] values)
    {
        string location = (string)values[0];
        bool expected = (bool)values[1];
        string[] inventory = (string[])values[2];
        string inventory_string = inventory.Any() ? String.Join(',', inventory) : "None";

        return $"{methodInfo.Name}({location}, {expected}, {inventory_string})";
    }
}
