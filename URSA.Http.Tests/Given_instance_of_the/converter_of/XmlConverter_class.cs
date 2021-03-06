﻿using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class XmlConverter_class : ComplexTypeConverterTest<XmlConverter>
    {
        private const string ContentType = "application/xml";

        protected override string SingleEntityContentType { get { return ContentType; } }

        protected override string MultipleEntitiesContentType { get { return ContentType; } }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_deserializing()
        {
        }

        [TestMethod]
        public override void it_should_not_acknowledge_the_converter_as_a_match_against_incompatible_type_when_serializing()
        {
        }

        protected override string SerializeObject<TI>(TI obj)
        {
            var serializer = new DataContractSerializer(typeof(TI));
            var buffer = new MemoryStream();
            serializer.WriteObject(buffer, obj);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }
    }
}