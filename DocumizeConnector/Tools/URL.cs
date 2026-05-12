using DocumizeConnector.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DocumizeConnector.Tools
{
    internal class URL
    {
        private static string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            var cleaned = input.ToLowerInvariant();

            // Replace any whitespace with hyphens
            cleaned = Regex.Replace(cleaned, @"\s+", "-");

            // Remove invalid characters (keep a-z, 0-9, and hyphens)
            cleaned = Regex.Replace(cleaned, @"[^a-z0-9\-]", string.Empty);

            // Collapse multiple hyphens into one
            cleaned = Regex.Replace(cleaned, @"\-{2,}", "-");

            // Trim leading/trailing hyphens
            cleaned = cleaned.Trim('-');

            return cleaned;
        }

        public static string UrlFromSpace(string base_url, Space space)
        {
            return base_url + "/s/" + space.ID + "/" + CleanString(space.Name);
        }

        public static string UrlFromDocument(string base_url, Space space, Document doc)
        {
            return UrlFromSpace(base_url, space) + "/d/" + doc.ID + "/" + CleanString(doc.Title);
        }
    }
}
