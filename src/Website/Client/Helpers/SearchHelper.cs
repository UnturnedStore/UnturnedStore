using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Website.Client.Helpers
{
    public static class SearchHelper
    {
        public const uint DefaultSensitivity = 3;

        public static uint FuzzyMap(string input, string target, uint sensitivity = DefaultSensitivity)
        {
            input = input.ToLower();
            string[] targets = SplitTarget(target);

            foreach (string t in targets)
            {
                if (t[0] != input[0]) continue; // Only targets with the same first character as the input will be checked. Ex: Troll - Tree (Yes), Bomb - Tomb (No)
                                                // This is to prevent weird results were itd match words inside of words. Which yes this compromise does limit the fuzzy search.
                                                // But also at the same time gives way more expected results
                string trSub = t;
                if (input.Length < trSub.Length)
                {
                    trSub = t.Substring(0, input.Length);
                }
                else if (input.Length > trSub.Length)
                {
                    input = input.Substring(0, trSub.Length);
                    if (sensitivity > 0) sensitivity--; // Punish length mismatch. To prevent stuff like TARGET: Hall - Hello
                }

                if (CompareStringDistance(input, trSub, DetermineSensitivity(input.Length, sensitivity))) return (uint)GetStringDistance(input, trSub);
            }

            return uint.MaxValue;
        }

        public static bool FuzzyCompare(string input, string target, uint sensitivity = DefaultSensitivity)
        {
            return FuzzyMap(input, target, sensitivity) < uint.MaxValue;
        }

        private static uint DetermineSensitivity(int Length, uint sensitivity)
        {
            if (Length <= 3) return 0;
            else if (Length >= 6) return sensitivity;
            // Linear Interpolation
            else return (uint)(sensitivity / 3 * Length - sensitivity / 3 * 3);
            // Ex:
            // Length, Sensitivity: 3
            // 2, 0
            // 3, 0
            // 4, 1 - Interpolated
            // 5, 2 - Interpolated
            // 6, 3
            // 7, 3
        }

        // Hamming Distance Algorithm
        // https://www.csharpstar.com/csharp-string-distance-algorithm/
        private static int GetStringDistance(string input, string target)
        {
            if (input.Length != target.Length) throw new Exception($"String length mismatch! INPUT: {input} - TARGET: {target}");
            return input.ToCharArray().Zip(target.ToCharArray(), (i, t) => new { i, t }).Count(x => x.i != x.t);
        }

        private static bool CompareStringDistance(string input, string target, uint sensitivity)
        { 
            return GetStringDistance(input, target) <= sensitivity;
        }

        private static string[] SplitTarget(string target)
        {
            return Regex.Matches(target, @"[A-Z][a-z]+|[A-Z][^a-z]|[ ][a-z]*") // Split at Capital Letters and Spaces - Ex: "RFVanillaUI manager" becomes [rf, vanilla, ui, manager]
                .Select(x => x.Value.ToLower().Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length > 1)
                .ToArray();
        }
    }
}
