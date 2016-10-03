using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Client
{
    class Prompts
    {
        class KeywordPrompt
        {
            string[] m_keywords;
            string m_default;
            static Regex s_reg = new Regex(".*\\[(?<keywords>.*)\\]\\<(?<default>.*)\\>$");
            public KeywordPrompt(string prompt)
            {
                var match = s_reg.Match(prompt);
                if (!match.Success)
                    throw new ArgumentException();
                m_keywords = match.Groups["keywords"].Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                m_default = match.Groups["default"].Value;
                string dummy;
                if (string.IsNullOrEmpty(m_default) || !TryMatch(m_default, out dummy))
                    throw new ArgumentException();
            }
            public bool TryMatch(string str, out string match)
            {
                if (string.IsNullOrEmpty(str))
                {
                    match = m_default;
                    return true;
                }
                match = null;
                var matches = m_keywords.Where(s => s.StartsWith(str, StringComparison.CurrentCultureIgnoreCase));
                if (matches.Count() != 1)
                    return false;
                match = matches.First();
                return true;
            }
        }
        public static string PromptForKeyword(string promptString)
        {
            var prompt = new KeywordPrompt(promptString);
            while (true)
            {
                Console.Write("{0}:", promptString);
                var resp = Console.ReadLine();
                string ret;
                if (prompt.TryMatch(resp, out ret))
                    return ret;
            }
        }
        public static int PromptForNumber(string promptString, int min, int max)
        {
            while (true)
            {
                Console.Write("{0}[{1}..{2}]:", promptString, min, max);
                var resp = Console.ReadLine();
                int res;
                if (int.TryParse(resp, out res))
                {
                    if (res >= min && res <= max)
                        return res;
                }
            }
        }
    }
}
