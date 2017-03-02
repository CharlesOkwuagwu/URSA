﻿using System;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using URSA.Security;

namespace Given_instance_of
{
    [TestFixture]
    public class SecuritySpecificationInfo_class
    {
        private SecuritySpecificationInfo _securitySpecification;

        [Test]
        public void it_should_add_a_claim()
        {
            _securitySpecification.Add(ClaimTypes.Name);

            _securitySpecification[ClaimTypes.Name].Should().BeEmpty();
        }

        [Test]
        public void it_should_add_a_claim_with_value()
        {
            var expected = "test";

            _securitySpecification.Add(ClaimTypes.Name, expected);

            _securitySpecification[ClaimTypes.Name].Should().Contain(expected);
        }

        [Test]
        public void it_should_remove_a_claim()
        {
            _securitySpecification.Add(ClaimTypes.Name);
            _securitySpecification.Remove(ClaimTypes.Name);

            _securitySpecification[ClaimTypes.Name].Should().BeNull();
        }

        [Test]
        public void it_should_remove_a_claims_with_value()
        {
            var expected = "test";

            _securitySpecification.Add(ClaimTypes.Name, expected);
            _securitySpecification.Remove(ClaimTypes.Name, expected);

            _securitySpecification[ClaimTypes.Name].Should().BeNull();
        }

        [Test]
        public void it_should_check_allowances_correctly()
        {
            _securitySpecification.Add(ClaimTypes.Name, "test");

            _securitySpecification.Matches(new BasicClaimBasedIdentity("test")).Should().BeTrue();
        }

        [Test]
        public void it_should_throw_when_no_claim_type_is_passed_when_getting_claims()
        {
            _securitySpecification.Invoking(instance => { var test = instance[null]; }).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_claim_type_is_passed_when_adding_claims()
        {
            _securitySpecification.Invoking(instance => instance.Add(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_claim_type_is_passed_when_removing_claims()
        {
            _securitySpecification.Invoking(instance => instance.Remove(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [SetUp]
        public void Setup()
        {
            _securitySpecification = new SecuritySpecificationInfo();
        }

        [TearDown]
        public void Teardown()
        {
            _securitySpecification = null;
        }
    }
}