﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP header collection.</summary>
    public class HeaderCollection : IDictionary<string, string>, IEnumerable<Header>
    {
        internal static readonly string Token = "[^\x00-\x20'()<>@,;:\\\"/\\[\\]?={}]+";
        internal static readonly Regex Header = new Regex(String.Format("(?<Name>{0}):(?<Value>(.|(?<=\r)\n(?=[ \t]))*)", Token));
        private IDictionary<string, Header> _headers = new Dictionary<string, Header>(Http.Header.Comparer);

        /// <summary>Gets or sets the 'Content-Length' header's value.</summary>
        /// <remarks>If the header does not exist, value of 0 is returned.</remarks>
        public int ContentLength
        {
            get { return (this[Http.Header.ContentLength] != null ? ((Http.Header<int>)this[Http.Header.ContentLength]).Values.First().Value : default(int)); }
            set
            {
                Http.Header<int> contentLength = (Http.Header<int>)this[Http.Header.ContentLength];
                if (contentLength == null)
                {
                    this[Http.Header.ContentLength] = new Header<int>(Http.Header.ContentLength, value);
                }
                else
                {
                    ((Http.Header<int>)this[Http.Header.ContentLength]).Values.First().Value = value;
                }
            }
        }

        /// <summary>Gets or sets the 'Content-Type' header's value.</summary>
        public string ContentType
        {
            get { return (this[Http.Header.ContentType] != null ? String.Join(",", this[Http.Header.ContentType].Values) : null); }
            set { Set(Http.Header.ContentType, value); }
        }

        /// <summary>Gets or sets the 'Accept' header's value.</summary>
        public string Accept
        {
            get { return (this[Http.Header.Accept] != null ? String.Join(",", this[Http.Header.Accept].Values) : null); }
            set { Set(Http.Header.Accept, value); }
        }

        /// <summary>Gets or sets the 'Content-Disposition' header's value.</summary>
        public string ContentDisposition
        {
            get { return (this[Http.Header.ContentDisposition] != null ? String.Join(",", this[Http.Header.ContentDisposition].Values) : null); }
            set { Set(Http.Header.ContentDisposition, value); }
        }

        /// <summary>Gets the count if headers.</summary>
        public int Count { get { return _headers.Count; } }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, string>>.IsReadOnly { get { return _headers.IsReadOnly; } }

        /// <inheritdoc />
        ICollection<string> IDictionary<string, string>.Values { get { return _headers.Values.Select(value => value.Value).ToList(); } }

        /// <inheritdoc />
        ICollection<string> IDictionary<string, string>.Keys { get { return _headers.Keys; } }

        /// <inheritdoc />
        string IDictionary<string, string>.this[string key]
        {
            get { return _headers[key].Value; }
            set { Set(key, value); }
        }

        /// <summary>Gets or sets the header by it's name.</summary>
        /// <param name="name">Name of the header.</param>
        /// <returns>Instance of the <see cref="Header" /> if the header with given <paramref name="name" /> exists; otherwise <b>null</b>.</returns>
        public Header this[string name]
        {
            get
            {
                Header result;
                if (!_headers.TryGetValue(name, out result))
                {
                    result = null;
                }

                return result;
            }

            set
            {
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }

                if (name.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("name");
                }

                if ((value != null) && (!Http.Header.Comparer.Equals(name, value.Name)))
                {
                    throw new InvalidOperationException(String.Format("Header name '{0}' and actual header '{1}' mismatch.", name, value.Name));
                }

                Remove(name);
                if (value != null)
                {
                    Set(value);
                }
            }
        }

        /// <summary>Tries to parse a given string as a <see cref="HttpHeaderCollection" />.</summary>
        /// <param name="headers">String to be parsed.</param>
        /// <param name="headersCollection">Resulting collection of headers if parsing was successful; otherwise <b>null</b>.</param>
        /// <returns><b>true</b> if the parsing was successful; otherwise <b>false</b>.</returns>
        public static bool TryParse(string headers, out HeaderCollection headersCollection)
        {
            bool result = false;
            headersCollection = null;
            try
            {
                headersCollection = Parse(headers);
                result = true;
            }
            catch
            {
            }

            return result;
        }

        /// <summary>Parses a given string as a <see cref="HeaderCollection" />.</summary>
        /// <param name="headers">String to be parsed.</param>
        /// <returns>Instance of the <see cref="HeaderCollection" />.</returns>
        public static HeaderCollection Parse(string headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            if (headers.Length == 0)
            {
                throw new ArgumentOutOfRangeException("headers");
            }

            HeaderCollection result = new HeaderCollection();
            var matches = Header.Matches(headers);
            foreach (Match match in matches)
            {
                result.Add(Http.Header.Parse(match.Value));
            }

            return result;
        }

        /// <summary>Adds a new header.</summary>
        /// <remarks>If the header already exists, method will merge new value and parameters with the existing ones.</remarks>
        /// <param name="header">Header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> either updated or added.</returns>
        public Header Add(Header header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            Header result;
            if (!_headers.TryGetValue(header.Name, out result))
            {
                _headers[header.Name] = result = header;
            }
            else
            {
                header.Values.ForEach(value => result.Values.AddUnique(value));
            }

            return result;
        }

        /// <summary>Adds a new header.</summary>
        /// <remarks>If the header already exists, method will merge new value and parameters with the existing ones.</remarks>
        /// <param name="name">Name of the header to be added.</param>
        /// <param name="value">Value of the header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> either updated or added.</returns>
        public Header Add(string name, string value)
        {
            return Add(Http.Header.Parse(String.Format("{0}:{1}", name, value)));
        }

        /// <summary>Sets a new header.</summary>
        /// <remarks>If the header already exists, it is replaced with the new one.</remarks>
        /// <param name="header">Header to be set.</param>
        /// <returns>Instance of the <see cref="Header" /> set.</returns>
        public Header Set(Header header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            return _headers[header.Name] = header;
        }

        /// <summary>Sets a new header.</summary>
        /// <remarks>If the header already exists, it is replaced with the new one.</remarks>
        /// <param name="name">Name of the header to be added.</param>
        /// <param name="value">Value of the header to be added.</param>
        /// <returns>Instance of the <see cref="Header" /> set.</returns>
        public Header Set(string name, string value)
        {
            return Set(Http.Header.Parse(String.Format("{0}:{1}", name, value)));
        }

        /// <summary>Removes the header with given name.</summary>
        /// <param name="name">Name of the header to be removed.</param>
        /// <returns><b>true</b> if the header was removed; otherwise <b>false</b>.</returns>
        public bool Remove(string name)
        {
            return _headers.Remove(name);
        }

        /// <summary>Clears the collection.</summary>
        public void Clear()
        {
            _headers.Clear();
        }

        /// <summary>Merges headers.</summary>
        /// <param name="headers">Headers to be merged.</param>
        public void Merge(HeaderCollection headers)
        {
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    Add(header);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<Header> GetEnumerator()
        {
            return _headers.Values.GetEnumerator();
        }

        /// <inheritdoc />
        void IDictionary<string, string>.Add(string name, string value)
        {
            Add(name, value);
        }

        /// <inheritdoc />
        bool IDictionary<string, string>.ContainsKey(string key)
        {
            return _headers.ContainsKey(key);
        }

        /// <inheritdoc />
        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            value = null;
            bool result = false;
            Header header;
            if (_headers.TryGetValue(key, out header))
            {
                value = header.Value;
                result = true;
            }

            return result;
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            Set(item.Key, item.Value);
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            _headers.Clear();
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            Header header = this[item.Key];
            return (header != null) && (Http.Header.Comparer.Equals(header.Value, item.Value));
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            Header header = this[item.Key];
            if (header != null)
            {
                HeaderValue value = header.Values.FirstOrDefault(valueItem => valueItem.Value == item.Value);
                if (value != null)
                {
                    header.Values.Remove(value);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            return _headers.Select(header => new KeyValuePair<string, string>(header.Key, header.Value.Value)).GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return String.Format("{0}\r\n\r\n", String.Join("\r\n", _headers.Values));
        }
    }
}