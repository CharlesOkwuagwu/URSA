﻿using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Parses URN based <see cref="Uri" />s.</summary>
    public class HydraUriParser : IUriParser
    {
        /// <inheritdoc />
        public UriParserCompatibility IsApplicable(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if ((uri.Scheme != "urn") && (uri.Scheme != "javascript"))
            {
                return UriParserCompatibility.None;
            }

            string segment = null;
            if ((uri.Scheme == URSA.Reflection.TypeExtensions.JavascriptSymbol) ||
                ((uri.Segments.Length > 0) && ((segment = uri.Segments.First()) != null) && (segment.StartsWith(URSA.Reflection.TypeExtensions.HydraSymbol))))
            {
                return UriParserCompatibility.ExactMatch;
            }

            return UriParserCompatibility.SchemeMatch;
        }

        /// <inheritdoc />
        public string Parse(Uri uri, out string @namespace)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            string language;
            var name = uri.Segments.First();
            int colonIndex;
            if ((colonIndex = name.IndexOf(':')) != -1)
            {
                language = name.Substring(0, colonIndex);
                name = name.Substring(colonIndex + 1);
            }
            else
            {
                language = uri.Scheme;
            }

            switch (language)
            {
                case URSA.Reflection.TypeExtensions.JavascriptSymbol:
                case URSA.Reflection.TypeExtensions.HydraSymbol:
                    break;
                default:
                    name = Regex.Replace(Regex.Replace(name, "[\\/]", "."), "^a-zA-Z0-9_]", String.Empty);
                    break;
            }

            var parts = name.Split('.');
            @namespace = String.Join(".", parts.Take(parts.Length - 1).Select(item => item.ToUpperCamelCase()));
            return parts[parts.Length - 1].ToUpperCamelCase();
        }
    }
}