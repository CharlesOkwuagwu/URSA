﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequestInfo_class
    {
        private const string Body = "some body";

        private static readonly IDictionary<string, string> Headers = new Dictionary<string, string>()
        {
            { "Content-Type", "text/plain; charset=utf-8" },
            { "Content-Length", Body.Length.ToString() }
        };

        private static readonly string Message = String.Format(
            "{0}\r\n\r\n{1}",
            String.Join("\r\n", Headers.Select(header => String.Format("{0}: {1}", header.Key, header.Value))),
            Body);

        private static readonly Uri Uri = new Uri("http://temp.uri/");

        private RequestInfo _request;

        [TestMethod]
        public void it_should_parse_request()
        {
            _request.Should().NotBeNull();
        }

        [TestMethod]
        public void it_should_parse_request_headers()
        {
            _request.Headers.Should().HaveCount(Headers.Count);
            _request.Headers[Headers.First().Key].Should().NotBeNull();
            _request.Headers[Headers.First().Key].Values.Should().HaveCount(1);
            _request.Headers[Headers.First().Key].Values.First().Value.Should().Be(Headers.First().Value.Split(';')[0]);
            _request.Headers[Headers.First().Key].Values.First().Parameters.Should().HaveCount(1);
            _request.Headers[Headers.First().Key].Values.First().Parameters.First().Name.Should().Be(Headers.First().Value.Split(';')[1].Split('=')[0].Trim());
            _request.Headers[Headers.First().Key].Values.First().Parameters.First().Value.Should().Be(Headers.First().Value.Split(';')[1].Split('=')[1]);
            _request.Headers[Headers.Last().Key].Should().NotBeNull();
            _request.Headers[Headers.Last().Key].Values.Should().HaveCount(1);
            _request.Headers[Headers.Last().Key].Values.First().Value.Should().Be(Headers.Last().Value);
        }

        [TestMethod]
        public void it_should_parse_request_body()
        {
            new StreamReader(_request.Body).ReadToEnd().Should().Be(Body);
        }

        [TestInitialize]
        public void Setup()
        {
            _request = RequestInfo.Parse(Verb.GET, Uri, Message);
        }

        [TestCleanup]
        public void Teardown()
        {
            _request = null;
        }
    }
}