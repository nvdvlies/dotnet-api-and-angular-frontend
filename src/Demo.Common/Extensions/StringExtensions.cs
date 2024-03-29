﻿using System.Text.RegularExpressions;

namespace Demo.Common.Extensions;

public static class StringExtensions
{
    public static string PascalToKebabCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return Regex.Replace(
                value,
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                "-$1",
                RegexOptions.Compiled)
            .Trim()
            .ToLower();
    }
}
