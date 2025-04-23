using System;
using System.Text.RegularExpressions;

namespace Task1_FileSystemVisitorApp;

public class RegexContainer
{
	// Pre-compiled regex to validate folder paths starting with "C:\"
    private static readonly Regex PathFormat = new Regex( @"^([a-zA-Z]:\\(?:[^<>:""/\\|?*\r\n]+\\?)+)$");

    private static readonly Regex YesRegex = new Regex(@"^(yes|y)$");

    public static bool ValidatePathFormat(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        return PathFormat.IsMatch(path);
    }

    public static bool IsValidYes(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return YesRegex.IsMatch(input.Trim());
    }

}
