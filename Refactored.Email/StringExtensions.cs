using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Refactored.Email
{
    internal static class StringExtensions
    {
        internal static string Replace(this string s, string oldValue, string newValue, StringComparison comparisonType)
        {
            if (s == null)
                return null;

            if (String.IsNullOrEmpty(oldValue))
                return s;

            StringBuilder result = new StringBuilder(Math.Min(4096, s.Length));
            int pos = 0;

            while (true)
            {
                int i = s.IndexOf(oldValue, pos, comparisonType);
                if (i < 0)
                    break;

                result.Append(s, pos, i - pos);
                result.Append(newValue);

                pos = i + oldValue.Length;
            }
            result.Append(s, pos, s.Length - pos);

            return result.ToString();
        }

        /// <summary>Expands the url with a standard full url based on the baseUrl.</summary>
        /// <param name="url"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        internal static string ExpandUrl(this string url, string baseUrl)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }

            if (url.StartsWith("~/"))
            {
                return url.Replace("~/", $"{baseUrl}/");
            }
            else if (url.StartsWith("/http:") || url.StartsWith("/https:"))
            {
                return url.Substring(1);
            }
            else if (url.StartsWith("/"))
            {
                return $"{baseUrl}{url}";
            }
            else
            {
                return $"{baseUrl}/{url}";
            }

        }

        /// <summary>
        /// Expands out local urls (those not containing any server/scheme parts) to full urls.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        internal static string ExpandUrls(this string content, string baseUrl)
        {
            foreach (Match match in new Regex("(?<fullHref>(href=(?<quote>\"|'))(?<url>[^\"|']+)(\\k<quote>))").Matches(content))
            {
                string url = match.Groups["url"].Value;
                string fullHref = match.Groups["fullHref"].Value;

                string newValue = url.ExpandUrl(baseUrl);
                string replacement = fullHref.Replace(url, newValue);

                if (content.Contains(fullHref) && !(url == newValue))
                {
                    content = Regex.Replace(content, Regex.Escape(fullHref), replacement, RegexOptions.IgnoreCase);
                }
            }
            return content;
        }

    }
}
