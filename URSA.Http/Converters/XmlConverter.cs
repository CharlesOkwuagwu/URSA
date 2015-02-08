﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts XML messages into objects</summary>
    public class XmlConverter : IConverter
    {
        /// <summary>Defines an 'text/xml' media type.</summary>
        public const string TextXml = "text/xml";

        /// <summary>Defines an 'application/xml' media type.</summary>
        public const string ApplicationXml = "application/xml";

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var requestInfo = (RequestInfo)request;
            var result = CompatibilityLevel.TypeMatch;
            var contentType = requestInfo.Headers[Header.Accept];
            if ((contentType != null) && ((contentType.Values.Any(value => (value.Value == TextXml) || (value.Value == ApplicationXml)))))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            object result = ConvertTo(typeof(T), request);
            return (result == null ? default(T) : (T)result);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            XmlSerializer serializer = new XmlSerializer(expectedType);
            return serializer.Deserialize(request.Body);
        }

        /// <inheritdoc />
        public T ConvertTo<T>(string body)
        {
            return (T)ConvertTo(typeof(T), body);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, string body)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (body == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(expectedType);
            return serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(body)));
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var result = CompatibilityLevel.TypeMatch;
            var responseInfo = (ResponseInfo)response;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Any(value => (value.Value == ApplicationXml) || (value.Value == TextXml))))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            ConvertFrom(typeof(T), instance, response);
        }

        /// <inheritdoc />
        public void ConvertFrom(Type givenType, object instance, IResponseInfo response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var responseInfo = (ResponseInfo)response;
            responseInfo.Headers.ContentType = responseInfo.Request.Headers[Header.Accept].Values.First(value =>
                (value.Value == ApplicationXml) || (value.Value == TextXml)).Value;
            if (instance != null)
            {
                if (!givenType.IsAssignableFrom(instance.GetType()))
                {
                    throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
                }

                var serializer = new XmlSerializer(givenType);
                serializer.Serialize(responseInfo.Body, instance);
            }
        }
    }
}