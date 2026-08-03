using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UIToolkitTransitions.Editor
{
    /// <summary>
    /// Extracts USS class selector names from raw USS text. Editor-only utility.
    /// </summary>
    public static class UssClassParser
    {
        private static readonly Regex BlockCommentRegex =
            new Regex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ClassTokenRegex =
            new Regex(@"\.(?<className>[a-zA-Z_][a-zA-Z0-9_-]*)", RegexOptions.Compiled);

        public static IReadOnlyList<string> ParseClassNames(string ussContent)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(ussContent)) return result;

            string withoutComments = BlockCommentRegex.Replace(ussContent, string.Empty);

            // Class selectors only appear in the selector part of a rule (before '{'),
            // never inside declaration blocks.
            foreach (string rule in withoutComments.Split('}'))
            {
                int blockStart = rule.IndexOf('{');
                string selector = blockStart >= 0 ? rule.Substring(0, blockStart) : rule;

                foreach (Match match in ClassTokenRegex.Matches(selector))
                {
                    string className = match.Groups["className"].Value;
                    if (!result.Contains(className))
                        result.Add(className);
                }
            }

            return result;
        }
    }
}
