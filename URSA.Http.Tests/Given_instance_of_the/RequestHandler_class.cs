﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;
using URSA.Web.Tests;

namespace Given_instance_of_the
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RequestHandler_class
    {
        private Mock<IDelegateMapper<RequestInfo>> _delegateMapper;
        private Mock<IArgumentBinder<RequestInfo>> _argumentBinder;
        private object[] _arguments;
        private Mock<IRequestMapping> _mapping;
        private Mock<IResponseComposer> _responseComposer;

        [TestMethod]
        public void it_should_use_a_response_composer_to_return_operation_result()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _responseComposer.Verify(instance => instance.ComposeResponse(It.IsAny<IRequestMapping>(), It.IsAny<object>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_argument_binder_to_prepare_input_parameters()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _argumentBinder.Verify(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()), Times.Once);
        }

        [TestMethod]
        public void it_should_use_delegate_mapper_to_call_the_controller_method()
        {
            var handler = SetupEnvironment(1, true);

            handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            _delegateMapper.Verify(instance => instance.MapRequest(It.IsAny<RequestInfo>()), Times.Once);
        }

        [TestMethod]
        public void it_should_unwrap_async_tasks_result()
        {
            var expected = 1;
            var handler = SetupEnvironmentAsync(expected, true);

            var result = handler.HandleRequest(new RequestInfo(Verb.GET, new Uri("http://temp.uri/api/test"), new MemoryStream()));

            result.Should().BeOfType<ObjectResponseInfo<int>>();
            ((ObjectResponseInfo<int>)result).Value.Should().Be(expected);
        }

        private RequestHandler SetupEnvironmentAsync<T>(T result = default(T), bool useDefaultArguments = false)
        {
            var converter = new Mock<IConverter>(MockBehavior.Strict);
            converter.Setup(instance => instance.ConvertFrom<T>(result, It.IsAny<IResponseInfo>()));
            var converterProvider = new Mock<IConverterProvider>(MockBehavior.Strict);
            converterProvider.Setup(instance => instance.FindBestOutputConverter<T>(It.IsAny<IResponseInfo>())).Returns(converter.Object);
            var requestHandler = SetupEnvironment(Task.FromResult(result), useDefaultArguments);
            _mapping.Setup(instance => instance.Invoke(_arguments)).Returns(result);
            _responseComposer.Setup(instance => instance.ComposeResponse(_mapping.Object, result, _arguments))
                .Returns(() => ObjectResponseInfo<int>.CreateInstance(Encoding.UTF8, (RequestInfo)_mapping.Object.Target.Response.Request, result, converterProvider.Object));
            return requestHandler;
        }

        private RequestHandler SetupEnvironment<T>(T result = default(T), bool useDefaultArguments = false)
        {
            var operation = CreateOperation();
            _arguments = operation.UnderlyingMethod.GetParameters().Select(parameter =>
                (useDefaultArguments ? Activator.CreateInstance(parameter.ParameterType) : null)).ToArray(); 

            ResponseInfo response = null;
            Mock<IController> controller = new Mock<IController>(MockBehavior.Strict);
            controller.SetupGet(instance => instance.Response).Returns(() => response);
            controller.SetupSet(instance => instance.Response = It.IsAny<ResponseInfo>()).Callback<IResponseInfo>(info => response = (ResponseInfo)info);

            _mapping = new Mock<IRequestMapping>(MockBehavior.Strict);
            _mapping.SetupGet(instance => instance.Operation).Returns(operation);
            _mapping.SetupGet(instance => instance.Target).Returns(controller.Object);
            _mapping.Setup(instance => instance.Invoke(_arguments)).Returns(result);

            _delegateMapper = new Mock<IDelegateMapper<RequestInfo>>(MockBehavior.Strict);
            _delegateMapper.Setup(instance => instance.MapRequest(It.IsAny<RequestInfo>())).Returns<RequestInfo>(request => _mapping.Object);

            _argumentBinder = new Mock<IArgumentBinder<RequestInfo>>();
            _argumentBinder.Setup(instance => instance.BindArguments(It.IsAny<RequestInfo>(), It.IsAny<IRequestMapping>()))
                .Returns<IRequestInfo, IRequestMapping>((request, requestMapping) => _arguments);

            _responseComposer = new Mock<IResponseComposer>(MockBehavior.Strict);
            _responseComposer.Setup(instance => instance.ComposeResponse(_mapping.Object, result, _arguments)).Returns((ResponseInfo)null);
            return new RequestHandler(_argumentBinder.Object, _delegateMapper.Object, _responseComposer.Object);
        }

        private OperationInfo<Verb> CreateOperation()
        {
            var method = typeof(TestController).GetMethod("Substract");
            var arguments = method.GetParameters().Select(parameter => (ValueInfo)new ArgumentInfo(parameter, FromQueryStringAttribute.For(parameter), "test", "test"));
            return new OperationInfo<Verb>(method, new Uri("/", UriKind.Relative), "test", new Regex(".*"), Verb.GET, arguments.ToArray());
        }
    }
}