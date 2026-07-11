namespace RandomizerTests;

/// <summary>
/// Shared <see cref="Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute"/> names.
/// </summary>
internal static class TestCategories
{
    /// <summary>
    /// Tests that generate multiple full seeds/worlds and are therefore slow. Excluded from the
    /// default run via RunSettings.runsettings (auto-applied through RandomizerTests.csproj).
    /// <para>
    /// To run them, point dotnet test at the opt-in settings file (a command-line settings file
    /// overrides the auto-applied one; a bare <c>--filter</c> would instead be AND-combined with
    /// the default and match nothing):
    /// <c>dotnet test --settings tests/RandomizerTests/Slow.runsettings</c>.
    /// </para>
    /// </summary>
    public const string Slow = "Slow";
}
