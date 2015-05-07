﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RomanticWeb;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using URSA.Web;
using URSA.Web.Description.Http;
using URSA.Web.Http;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Model;
using URSA.Web.Http.Description.Owl;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace URSA.CodeGen
{
    /// <summary>Provides a basic implementation of the <see cref="IClassGenerator" />.</summary>
    public class HydraClassGenerator : IClassGenerator
    {
        private static readonly string EntityClassTemplate = new StreamReader(typeof(HydraClassGenerator).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.CodeGen.Templates.Entity.cs")).ReadToEnd();
        private static readonly string ClientClassTemplate = new StreamReader(typeof(HydraClassGenerator).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.CodeGen.Templates.Client.cs")).ReadToEnd();
        private static readonly string ResponseClassTemplate = new StreamReader(typeof(HydraClassGenerator).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.CodeGen.Templates.Response.cs")).ReadToEnd();
        private static readonly string PropertyTemplate = new StreamReader(typeof(HydraClassGenerator).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.CodeGen.Templates.Property.cs")).ReadToEnd();
        private static readonly string OperationTemplate = new StreamReader(typeof(HydraClassGenerator).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.CodeGen.Templates.Operation.cs")).ReadToEnd();
        private static readonly IDictionary<IResource, string> Namespaces = new ConcurrentDictionary<IResource, string>();
        private static readonly IDictionary<IResource, string> Names = new ConcurrentDictionary<IResource, string>();

        private readonly IEnumerable<IUriParser> _uriParsers;

        /// <summary>Initializes a new instance of the <see cref="HydraClassGenerator"/> class.</summary>
        /// <param name="uriParsers">The URI parsers.</param>
        public HydraClassGenerator(IEnumerable<IUriParser> uriParsers)
        {
            if (uriParsers == null)
            {
                throw new ArgumentNullException("uriParsers");
            }

            _uriParsers = uriParsers;
        }

        /// <inheritdoc />
        public IDictionary<string, string> CreateCode(IClass supportedClass)
        {
            var @namespace = CreateNamespace(supportedClass);
            var name = CreateName(supportedClass);
            var result = new Dictionary<string, string>();
            var includes = new StringBuilder(256);
            var properties = AnalyzeProperties(supportedClass);
            var operations = AnalyzeOperations(supportedClass, @namespace + "." + name, result);
            result[name + ".cs"] = String.Format(EntityClassTemplate, @namespace, name, properties.Replace("public new ", "public "), properties.Replace("        public ", "        "));
            result[name + "Client.cs"] = String.Format(ClientClassTemplate, @namespace, name, operations);
            return result;
        }

        /// <inheritdoc />
        public string CreateNamespace(IResource resource)
        {
            if (resource.GetTypes().Any(type => type.Uri.AbsoluteUri == resource.Context.Mappings.MappingFor<IDatatypeDefinition>().Classes.First().Uri.AbsoluteUri))
            {
                resource = resource.AsEntity<IDatatypeDefinition>().Datatype;
            }

            string result;
            if (Namespaces.TryGetValue(resource, out result))
            {
                return result;
            }

            ParseUri(resource);
            return Namespaces[resource];
        }

        /// <inheritdoc />
        public string CreateName(IResource resource)
        {
            if (resource.GetTypes().Any(type => type.Uri.AbsoluteUri == resource.Context.Mappings.MappingFor<IDatatypeDefinition>().Classes.First().Uri.AbsoluteUri))
            {
                resource = resource.AsEntity<IDatatypeDefinition>().Datatype;
            }

            string result;
            if (Names.TryGetValue(resource, out result))
            {
                return result;
            }

            ParseUri(resource);
            return Names[resource];
        }

        private string CreateName(IClass @class)
        {
            if (@class.SubClassOf.Any(superClass => 
                (AbsoluteUriComparer.Default.Equals(superClass.Id.Uri, Rdf.List) ||
                (AbsoluteUriComparer.Default.Equals(superClass.Id.Uri, Rdf.Seq)) || 
                (AbsoluteUriComparer.Default.Equals(superClass.Id.Uri, Rdf.Bag)))))
            {
                return CreateName((IResource)@class);
            }

            return (!String.IsNullOrEmpty(@class.Label) ? @class.Label : CreateName((IResource)@class));
        }

        /// <inheritdoc />
        private string CreateName(IOperation operation, string method)
        {
            string result = operation.Label;
            if (operation.Method.Count > 1)
            {
                result = ControllerDescriptionBuilder<IController>.PopularNameMappings
                    .Where(item => item.Value.ToString() == method).Select(item => item.Key).FirstOrDefault() ?? method.ToUpperCamelCase();
            }

            if (String.IsNullOrEmpty(result))
            {
                result = CreateName((IResource)operation);
            }

            return result;
        }

        private void ParseUri(IResource resource)
        {
            var uriParser = (from parser in _uriParsers
                             orderby parser.IsApplicable(resource.Id.Uri) descending
                             select parser).FirstOrDefault();

            if (uriParser == null)
            {
                throw new InvalidOperationException(String.Format("Cannot find a suitable parser for resource uri '{0}'.", resource.Id.Uri));
            }

            string @namespace;
            Names[resource] = uriParser.Parse(resource.Id.Uri, out @namespace);
            Namespaces[resource] = @namespace;
        }

        private string AnalyzeProperties(IClass supportedClass)
        {
            var properties = new StringBuilder(512);
            foreach (var property in supportedClass.SupportedProperties)
            {
                var propertyName = (!String.IsNullOrEmpty(property.Property.Label) ? property.Property.Label : CreateName(property.Property.AsEntity<IResource>()));
                var propertyTypeNamespace = "System";
                var propertyTypeName = "Object";
                if (property.Property.Range.Any())
                {
                    var propertyType = property.Property.Range.First().AsEntity<IResource>();
                    propertyTypeNamespace = CreateNamespace(propertyType);
                    propertyTypeName = CreateName(propertyType);
                }

                var typeName = String.Format("{0}.{1}", propertyTypeNamespace, propertyTypeName);
                var restriction = supportedClass.SubClassOf.OfType<IRestriction>().FirstOrDefault(item => item.OnProperty.Id == property.Property.Id);
                if (restriction == null)
                {
                    typeName = String.Format("System.Collections.Generic.IEnumerable<{0}>", typeName);
                }

                properties.AppendFormat(
                    PropertyTemplate,
                    propertyName,
                    property.WriteOnly ? String.Empty : " get" + (property.ReadOnly ? " " : String.Empty) + ";",
                    property.ReadOnly ? String.Empty : " set; ",
                    typeName,
                    (propertyName == "Id" ? "new " : String.Empty));
            }

            return properties.ToString();
        }

        private string AnalyzeOperations(IClass supportedClass, string supportedClassFullName, IDictionary<string, string> classes)
        {
            var operations = new StringBuilder(1024);
            var supportedOperations = (from operation in supportedClass.SupportedOperations
                                       select new KeyValuePair<IOperation, IIriTemplate>(operation, null))
                .Union(
                    from quad in supportedClass.Context.Store.Quads.ToList()
                    where (quad.Subject.IsUri) && (quad.Object.IsUri) && (AbsoluteUriComparer.Default.Equals(quad.Subject.Uri, supportedClass.Id.Uri)) &&
                        (quad.PredicateIs(supportedClass.Context, supportedClass.Context.Mappings.MappingFor<ITemplatedLink>().Classes.First().Uri))
                    let templatedLink = supportedClass.Context.Load<ITemplatedLink>(quad.Predicate.ToEntityId())
                    from operation in templatedLink.Operations
                    select new KeyValuePair<IOperation, IIriTemplate>(operation, supportedClass.Context.Load<IIriTemplate>(quad.Object.ToEntityId())));
            foreach (var operationDescriptor in supportedOperations)
            {
                AnalyzeOperation(supportedClass, supportedClassFullName, operationDescriptor.Key, operationDescriptor.Value, operations, classes);
            }

            return operations.ToString();
        }

        private void AnalyzeOperation(IClass supportedClass, string supportedClassFullName, IOperation operation, IIriTemplate template, StringBuilder operations, IDictionary<string, string> classes)
        {
            foreach (var method in operation.Method)
            {
                bool singleValueExpected = false;
                var bodyArguments = new StringBuilder(256);
                var uriArguments = new StringBuilder(256);
                var parameters = new StringBuilder(256);
                var mediaTypes = new StringBuilder(256);
                var returns = "void";
                var operationName = CreateName(operation, method);
                var uri = operation.Id.Uri.ToRelativeUri().ToString();
                var returnedType = String.Empty;
                var isReturns = String.Empty;
                if (template != null)
                {
                    uri = template.Template;
                    AnalyzeTemplate(template.Mappings, parameters, uriArguments, out singleValueExpected);
                }

                singleValueExpected |= (method == Verb.POST.ToString()) && (operation.Returns.Count() == 1) &&
                    (operation.Expects.Count() == 1) && (operation.Expects.First().Id == supportedClass.Id) &&
                    (operation.Returns.Join(
                        supportedClass.GetUniqueIdentifierType(), 
                        outer => (outer.Is(supportedClass.Context.Mappings.MappingFor<IDatatypeDefinition>().Classes.First().Uri) ? outer.AsEntity<IDatatypeDefinition>().Datatype.Id : outer.Id),
                        inner => (inner.Is(supportedClass.Context.Mappings.MappingFor<IDatatypeDefinition>().Classes.First().Uri) ? inner.AsEntity<IDatatypeDefinition>().Datatype.Id : inner.Id), 
                        (outer, inner) => inner).Any());
                AnalyzeBody(supportedClassFullName, operation.Expects, parameters, bodyArguments);
                if (parameters.Length > 2)
                {
                    parameters.Remove(parameters.Length - 2, 2);
                }

                if (operation.Returns.Any())
                {
                    isReturns = "return ";
                    returns = AnalyzeResult(supportedClassFullName, operationName, operation.Returns, classes, singleValueExpected);
                    returnedType = String.Format("<{0}>", returns);
                }

                if (operation.MediaTypes.Any())
                {
                    mediaTypes.AppendFormat(
                        "            var mediaTypes = new string[] {{\r\n                {0}}};",
                        String.Join(",\r\n                ", operation.MediaTypes.Select(mediaType => String.Format("\"{0}\"", mediaType))));
                }
                else
                {
                    mediaTypes.Append("            var mediaTypes = new string[0];");
                }

                var isStatic = ((operation.Expects.Any(expected => expected == supportedClass)) && (method == "POST") ? " static" : String.Empty);
                operations.AppendFormat(OperationTemplate, isStatic, returns, operationName, parameters, method, uri, uriArguments, bodyArguments, isReturns, returnedType, mediaTypes);
            }
        }

        private void AnalyzeTemplate(IEnumerable<IIriTemplateMapping> mappings, StringBuilder parameters, StringBuilder uriArguments, out bool expectSingleValue)
        {
            expectSingleValue = false;
            foreach (var mapping in mappings)
            {
                var variableName = mapping.Variable.ToLowerCamelCase();
                IResource expected = null;
                if (mapping.Property != null)
                {
                    if (mapping.Property.GetTypes().Any(type => AbsoluteUriComparer.Default.Equals(type.Uri, mapping.Context.Mappings.MappingFor<IInverseFunctionalProperty>().Classes.First().Uri)))
                    {
                        expectSingleValue = true;
                    }

                    if (mapping.Property.Range.Any())
                    {
                        expected = mapping.Property.Range.First().AsEntity<IResource>();
                    }
                }

                var @namespace = (expected != null ? CreateNamespace(expected) : "System");
                var name = (expected != null ? CreateName(expected) : "Object");
                parameters.AppendFormat("{0}.{1} {2}, ", @namespace, name, variableName);
                uriArguments.AppendFormat("            uriArguments.{0} = {0};{1}", variableName, Environment.NewLine);
            }
        }

        private void AnalyzeBody(string supportedClassFullName, IEnumerable<IResource> expects, StringBuilder parameters, StringBuilder bodyArguments)
        {
            foreach (var expected in expects)
            {
                string variableName = expected.Label.ToLowerCamelCase();
                string @namespace;
                string name = AnalyzeType(supportedClassFullName, expected, out @namespace);
                parameters.AppendFormat("{0}.{1} {2}, ", @namespace, name, variableName);
                bodyArguments.AppendFormat(", {0}", variableName);
            }
        }

        private string AnalyzeResult(string supportedClassFullName, string operationName, IEnumerable<IResource> returns, IDictionary<string, string> classes, bool singleValueExpected)
        {
            string result;
            string @namespace;
            string name;
            if (returns.Count() > 1)
            {
                StringBuilder includes = new StringBuilder();
                result = String.Format("{0}Result", operationName);
                if (includes.ToString().IndexOf(result) != -1)
                {
                    return result;
                }

                var properties = new StringBuilder(256);
                foreach (var returned in returns)
                {
                    name = AnalyzeType(supportedClassFullName, returned, out @namespace);
                    properties.AppendFormat(PropertyTemplate, name, " get;", String.Empty, @namespace, name);
                }

                includes.AppendFormat(ResponseClassTemplate, result, properties);
                classes[result + ".cs"] = includes.ToString();
            }
            else
            {
                name = AnalyzeType(supportedClassFullName, returns.First(), out @namespace);
                result = String.Format("{0}.{1}", @namespace, name);
                if (!singleValueExpected)
                {
                    result = String.Format("System.Collections.Generic.IEnumerable<{0}>", result);
                }
            }

            return result;
        }

        private string AnalyzeType(string supportedClassFullName, IResource type, out string @namespace)
        {
            IClass @class = (type.Is(Rdfs.Class) ? type.AsEntity<IClass>() : null);
            if (@class == null)
            {
                @namespace = CreateNamespace(type);
                return CreateName(type);
            }

            @namespace = CreateNamespace(@class);
            var name = CreateName(@class);
            if (@namespace + "." + name == supportedClassFullName)
            {
                name = "I" + name;
            }

            if (@class.Is(Rdf.List))
            {
                @namespace = typeof(IList).Namespace;
                return typeof(IList).Name;
            }

            IResource itemRestriction;
            if (!@class.IsGenericRdfList(out itemRestriction))
            {
                return name;
            }

            @namespace = typeof(IList<>).Namespace;
            string itemType = "System.Object";
            if ((itemRestriction == null) || (itemRestriction == @class))
            {
                return String.Format("{0}<{1}>", typeof(IList<>).Name, itemType);
            }

            if (itemRestriction.Is(Rdfs.Class))
            {
                string itemTypeNamespace;
                itemType = AnalyzeType(supportedClassFullName, itemRestriction.AsEntity<IClass>(), out itemTypeNamespace);
                itemType = String.Format("{0}.{1}", itemTypeNamespace, itemType);
            }
            else
            {
                itemType = String.Format("{0}.{1}", CreateNamespace(itemRestriction), CreateName(itemRestriction));
            }

            return String.Format("{0}<{1}>", typeof(IList<>).Name, itemType);
        }
    }
}