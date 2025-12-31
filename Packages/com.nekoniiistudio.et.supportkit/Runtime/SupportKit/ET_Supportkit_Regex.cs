using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ET.SupportKit.RegexExtension
{
    public static class ERegex
    {
        public static string RemovePrefix(this string input, string prefix)
        {
            string pattern = "^" + Regex.Escape(prefix);
            return Regex.Replace(input, pattern, "");
        }
        public static string RemoveSuffix(this string input, string suffix)
        {
            string pattern = Regex.Escape(suffix) + "$";
            return Regex.Replace(input, pattern, "");
        }
        /// <summary>
        /// Use a regular expression to match "g card game" and replace it with "gCardGame"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCamelStyle(this string input)
        {
            return Regex.Replace(input, @"(?:_| )(\w)", match => match.Groups[1].Value.ToUpper());
        }
        public static string ToSnakeStyle(this string input)
        {
            // Use regex to insert underscores before capital letters and convert to lowercase
            return Regex.Replace(input, "([a-z0-9])([A-Z])|\\s", "$1_$2").ToLower();
        }
    }
}