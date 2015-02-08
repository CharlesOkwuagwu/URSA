﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Provides an HTTP related argument binding facility.</summary>
    public class ArgumentBinder : IArgumentBinder<RequestInfo>
    {
        private IEnumerable<IParameterSourceArgumentBinder> _parameterSourceArgumentBinders;

        /// <summary>Initializes a new instance of the <see cref="ArgumentBinder" /> class.</summary>
        /// <param name="parameterSourceArgumentBinders">Parameter source argument binders.</param>
        public ArgumentBinder(IEnumerable<IParameterSourceArgumentBinder> parameterSourceArgumentBinders)
        {
            if (parameterSourceArgumentBinders == null)
            {
                throw new ArgumentNullException("parameterSourceArgumentBinders");
            }

            _parameterSourceArgumentBinders = parameterSourceArgumentBinders;
        }

        /// <inheritdoc />
        public object[] BindArguments(IRequestInfo request, IRequestMapping requestMapping)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!(request is RequestInfo))
            {
                throw new InvalidOperationException(String.Format("Requests of type '{0}' are not supported.", request.GetType()));
            }

            return BindArguments((RequestInfo)request, requestMapping);
        }

        /// <inheritdoc />
        public object[] BindArguments(RequestInfo request, IRequestMapping requestMapping)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (requestMapping == null)
            {
                throw new ArgumentNullException("requestMapping");
            }

            ArrayList result = new ArrayList();
            var parameters = requestMapping.Operation.Arguments;
            IDictionary<RequestInfo, RequestInfo[]> multipartBodies = new Dictionary<RequestInfo, RequestInfo[]>();
            int index = 0;
            foreach (var argument in requestMapping.Operation.Arguments)
            {
                object value;
                if (!argument.Parameter.IsOut)
                {
                    value = BindArgument(request, requestMapping, argument, index, multipartBodies);
                    if ((value == (argument.Parameter.ParameterType.IsValueType ? Activator.CreateInstance(argument.Parameter.ParameterType) : null)) &&
                        (argument.Parameter.DefaultValue != null))
                    {
                        value = argument.Parameter.DefaultValue;
                    }
                }
                else
                {
                    value = (argument.Parameter.ParameterType.IsValueType ? Activator.CreateInstance(argument.Parameter.ParameterType) : null);
                }

                result.Add(value);
                index++;
            }

            return result.ToArray();
        }

        private object BindArgument(RequestInfo request, IRequestMapping requestMapping, ArgumentInfo parameter, int index, IDictionary<RequestInfo, RequestInfo[]> multipartBodies)
        {
            Type parameterSourceArgumentBinderType = typeof(IParameterSourceArgumentBinder<>).MakeGenericType(parameter.Source.GetType());
            IParameterSourceArgumentBinder argumentBinder =
                (from binder in _parameterSourceArgumentBinders
                 where (parameterSourceArgumentBinderType.IsAssignableFrom(binder.GetType())) ||
                    (binder.GetType().GetInterfaces().Any(type => parameterSourceArgumentBinderType.IsAssignableFrom(type)))
                 select binder).FirstOrDefault();

            if (argumentBinder != null)
            {
                ArgumentBindingContext context = (ArgumentBindingContext)typeof(ArgumentBindingContext<>)
                    .MakeGenericType(parameter.Source.GetType())
                    .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .First()
                    .Invoke(new object[] {
                        (RequestInfo)request,
                        (RequestMapping)requestMapping,
                        parameter.Parameter,
                        index,
                        parameter.Source,
                        multipartBodies });
                return argumentBinder.GetArgumentValue(context);
            }

            return null;
        }
    }
}