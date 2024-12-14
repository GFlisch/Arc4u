using System.Text.RegularExpressions;

namespace Arc4u.Dependency.Tool;

public static class ClassNameCleaner
{
    private static readonly Regex InvalidCharsRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);

    public static string CleanClassName(string className)
    {
        // Remove invalid characters using the compiled regex
        var cleanedName = InvalidCharsRegex.Replace(className, "");

        // Ensure the first character is not a digit
        if (char.IsDigit(cleanedName[0]))
        {
            cleanedName = "_" + cleanedName;
        }

        return cleanedName;
    }
}
