﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Http;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [TestClass]
    public class RequestHandler_class
    {
        [TestMethod]
        public void it_call_method_on_the_controller()
        {
            SetupEnvironment(true);
            var handler = new RequestHandler();
            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            result.Status.Should().Be(HttpStatusCode.OK);
            result.Should().BeOfType<ObjectResponseInfo<int>>();
            ((ObjectResponseInfo<int>)result).Value.Should().Be(1);
        }

        [TestMethod]
        public void it_should_return_400_Bad_Request_when_argument_is_missing()
        {
            SetupEnvironment();
            var handler = new RequestHandler();
            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            result.Status.Should().Be(HttpStatusCode.BadRequest);
            result.Should().BeOfType<ExceptionResponseInfo>();
            ((ExceptionResponseInfo)result).Value.Should().BeOfType<ArgumentNullException>();
        }

        private void SetupEnvironment(bool useDefaultArguments = false)
        {
            var operation = CreateOperation();
            var arguments = operation.UnderlyingMethod.GetParameters().Select(parameter =>
                (useDefaultArguments ? Activator.CreateInstance(parameter.ParameterType) : null)).ToArray(); 

            ResponseInfo response = null;
            Mock<IController> controller = new Mock<IController>(MockBehavior.Strict);
            controller.SetupGet(instance => instance.Response).Returns(() => response);
            controller.SetupSet(instance => instance.Response = It.IsAny<ResponseInfo>()).Callback<IResponseInfo>(info => response = (ResponseInfo)info);

            int result = 1;
            Mock<IRequestMapping> mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            mapping.SetupGet(instance => instance.Operation).Returns(operation);
            mapping.SetupGet(instance => instance.Target).Returns(controller.Object);
            mapping.Setup(instance => instance.Invoke(arguments)).Returns(result);

            Mock<IDelegateMapper<RequestInfo>> delegateMapper = new Mock<IDelegateMapper<RequestInfo>>(MockBehavior.Strict);
            delegateMapper.As<IDelegateMapper>().Setup(instance => instance.MapRequest(It.IsAny<RequestInfo>())).Returns<RequestInfo>(request => mapping.Object);

            Mock<IArgumentBinder<RequestInfo>> argumentBinder = new Mock<IArgumentBinder<RequestInfo>>();
            argumentBinder.As<IArgumentBinder>().Setup(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()))
                .Returns<IRequestInfo, IRequestMapping>((request, requestMapping) => arguments);

            Mock<IConverter> converter = new Mock<IConverter>(MockBehavior.Strict);
            converter.Setup(instance => instance.CanConvertFrom(typeof(int), It.IsAny<ResponseInfo>())).Returns(CompatibilityLevel.ExactMatch);
            converter.Setup(instance => instance.ConvertFrom<int>(result, It.IsAny<ResponseInfo>()));

            Mock<IComponentProvider> container = new Mock<IComponentProvider>(MockBehavior.Strict);
            container.Setup(instance => instance.Resolve<IDelegateMapper<RequestInfo>>()).Returns(delegateMapper.Object);
            container.Setup(instance => instance.Resolve<IArgumentBinder<RequestInfo>>()).Returns(argumentBinder.Object);
            container.Setup(instance => instance.Register<IConverterProvider>(It.IsAny<IConverterProvider>()));
            container.Setup(instance => instance.Register<IControllerActivator>(It.IsAny<IControllerActivator>()));
            container.Setup(instance => instance.ResolveAll<IConverter>()).Returns(new IConverter[] { converter.Object });
            typeof(UrsaConfigurationSection).GetProperty("ComponentProvider", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .SetValue(null, container.Object);
        }

        private OperationInfo CreateOperation()
        {
            var method = typeof(TestController).GetMethod("Substract");
            var arguments = method.GetParameters().Select(parameter => new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "test", "test")).ToArray();
            return new URSA.Web.Description.Http.OperationInfo(method, Verb.GET, new Uri("/", UriKind.Relative), new Regex(".*"), "test", arguments);
        }
    }
}