﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities;
using RomanticWeb.Vocabularies;
using URSA.Reflection;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.CodeGen;
using URSA.Web.Http.Description.Entities;
using URSA.Web.Http.Description.Hydra;
using URSA.Web.Http.Description.Owl;
using URSA.Web.Http.Description.Rdfs;
using URSA.Web.Http.Description.Reflection;
using IClass = URSA.Web.Http.Description.Hydra.IClass;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a type description building facility.</summary>
    public class HydraCompliantTypeDescriptionBuilder : ITypeDescriptionBuilder
    {
        private static readonly IDictionary<Type, Uri> TypeDescriptions = XsdUriParser.Types.Concat(OGuidUriParser.Types).ToDictionary(item => item.Key, item => item.Value);

        private static readonly Uri[] SupportedMediaTypeProfiles = { EntityConverter.Hydra };

        private readonly IXmlDocProvider _xmlDocProvider;

        /// <summary>Initializes a new instance of the <see cref="HydraCompliantTypeDescriptionBuilder"/> class.</summary>
        /// <param name="xmlDocProvider">The XML documentation provider.</param>
        public HydraCompliantTypeDescriptionBuilder(IXmlDocProvider xmlDocProvider)
        {
            if (xmlDocProvider == null)
            {
                throw new ArgumentNullException("xmlDocProvider");
            }

            _xmlDocProvider = xmlDocProvider;
        }

        /// <inheritdoc />
        public IEnumerable<Uri> SupportedProfiles { get { return SupportedMediaTypeProfiles; } }

        /// <inheritdoc />
        public IClass BuildTypeDescription(DescriptionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            bool requiresRdf;
            return BuildTypeDescription(context, out requiresRdf);
        }

        /// <inheritdoc />
        public IClass BuildTypeDescription(DescriptionContext context, out bool requiresRdf)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.ContainsType(context.Type))
            {
                return context.BuildTypeDescription(out requiresRdf);
            }

            if (System.Reflection.TypeExtensions.IsEnumerable(context.Type))
            {
                if (context.Type.IsList())
                {
                    return CreateListDefinition(context, out requiresRdf, context.Type.IsGenericList());
                }

                return CreateCollectionDefinition(context, out requiresRdf, context.Type.IsGenericEnumerable());
            }

            requiresRdf = false;
            Type itemType = context.Type.GetItemType();
            if (TypeDescriptions.ContainsKey(itemType))
            {
                return BuildDatatypeDescription(context, TypeDescriptions[itemType]);
            }

            var classUri = itemType.MakeUri();
            if (typeof(IEntity).IsAssignableFrom(itemType))
            {
                classUri = context.ApiDocumentation.Context.Mappings.MappingFor(itemType).Classes.Select(item => item.Uri).FirstOrDefault() ?? classUri;
                requiresRdf = true;
            }

            IClass result = context.ApiDocumentation.Context.Create<IClass>(classUri);
            result.Label = itemType.MakeTypeName(false, true);
            result.Description = _xmlDocProvider.GetDescription(itemType);
            if (typeof(EntityId).IsAssignableFrom(itemType))
            {
                context.Describe(result, requiresRdf);
                return result;
            }

            context.Prescribe(result, requiresRdf);
            SetupProperties(context.ForType(itemType), result);
            context.Describe(result, requiresRdf);
            return result;
        }

        /// <inheritdoc />
        public EntityId GetSupportedPropertyId(PropertyInfo property, Type declaringType = null)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            return new EntityId(property.MakeUri(declaringType ?? property.DeclaringType));
        }

        /// <inheritdoc />
        public IClass SubClass(DescriptionContext context, IClass @class, Type contextTypeOverride = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (@class == null)
            {
                throw new ArgumentNullException("class");
            }

            IClass result = context.ApiDocumentation.Context.Create<IClass>(@class.CreateBlankId());
            result.SubClassOf.Add(@class);
            return result;
        }

        private static IClass BuildDatatypeDescription(DescriptionContext context, Uri uri)
        {
            var definition = context.ApiDocumentation.Context.Create<IClass>(new EntityId(uri));
            definition.Label = (uri.Fragment.Length > 1 ? uri.Fragment.Substring(1) : uri.Segments.Last());
            return definition;
        }

        private void SetupProperties(DescriptionContext context, IClass @class)
        {
            if (context.IsTypeComplete(context.Type))
            {
                if (@class == context[context.Type])
                {
                    return;
                }

                foreach (var property in context[context.Type].SupportedProperties)
                {
                    @class.SupportedProperties.Add(property);
                }
            }
            else
            {
                var properties = context.Type.GetProperties(typeof(IEntity));
                foreach (var property in properties)
                {
                    @class.SupportedProperties.Add(BuildSupportedProperty(context, @class, context.Type, property));
                }
            }
        }

        private IClass CreateListDefinition(DescriptionContext context, out bool requiresRdf, bool isGeneric = true)
        {
            Type itemType;
            var result = CreateCollectionDefinition(context, out requiresRdf, out itemType, isGeneric);
            if (!isGeneric)
            {
                return result;
            }

            var memberType = (context.ContainsType(itemType) ? context[itemType] : BuildTypeDescription(context.ForType(itemType), out requiresRdf));
            result.SubClassOf.Add(context.ApiDocumentation.Context.Create<IClass>(Rdf.List));
            result.SubClassOf.Add(result.CreateRestriction(Rdf.first, memberType));
            result.SubClassOf.Add(result.CreateRestriction(Rdf.rest, result));
            return result;
        }

        private IClass CreateCollectionDefinition(DescriptionContext context, out bool requiresRdf, bool isGeneric = true)
        {
            Type itemType;
            return CreateCollectionDefinition(context, out requiresRdf, out itemType, isGeneric);
        }

        private IClass CreateCollectionDefinition(DescriptionContext context, out bool requiresRdf, out Type itemType, bool isGeneric = true)
        {
            var result = CreateEnumerableDefinition(context, context.ApiDocumentation.Context.Mappings.MappingFor<ICollection>().Classes.First().Uri, out requiresRdf, out itemType, isGeneric);
            if (!isGeneric)
            {
                return result;
            }

            var memberType = (context.ContainsType(itemType) ? context[itemType] : BuildTypeDescription(context.ForType(itemType), out requiresRdf));
            result.SubClassOf.Add(result.CreateRestriction(context.ApiDocumentation.Context.Mappings.MappingFor<ICollection>().PropertyFor("Members").Uri, memberType));
            return result;
        }

        private IClass CreateEnumerableDefinition(DescriptionContext context, Uri baseType, out bool requiresRdf, out Type itemType, bool isGeneric = true)
        {
            itemType = null;
            requiresRdf = false;
            if (!isGeneric)
            {
                return context.ApiDocumentation.Context.Create<IClass>(baseType);
            }

            itemType = context.Type.GetItemType();
            IClass result = context.ApiDocumentation.Context.Create<IClass>(context.Type.MakeUri());
            result.Label = context.Type.MakeTypeName(false, true);
            result.Description = _xmlDocProvider.GetDescription(context.Type);
            result.SubClassOf.Add(context.ApiDocumentation.Context.Create<IClass>(baseType));
            requiresRdf |= context.RequiresRdf(itemType);
            context.Describe(result, requiresRdf);
            return result;
        }

        private ISupportedProperty BuildSupportedProperty(DescriptionContext context, IClass @class, Type declaringType, PropertyInfo property)
        {
            var propertyId = GetSupportedPropertyId(property, declaringType);
            var propertyUri = (!typeof(IEntity).IsAssignableFrom(context.Type) ? @class.Id.Uri.AddName(property.Name) :
                context.ApiDocumentation.Context.Mappings.MappingFor(context.Type).PropertyFor(property.Name).Uri);
            var result = context.ApiDocumentation.Context.Create<ISupportedProperty>(propertyId);
            result.Readable = property.CanRead;
            result.Writeable = property.CanWrite;
            result.Required = (property.PropertyType.IsValueType) || (property.GetCustomAttribute<RequiredAttribute>() != null);
            var isKeyProperty = (property.GetCustomAttribute<KeyAttribute>() != null) ||
                (property.ImplementsGeneric(typeof(IControlledEntity<>), "Key"));
            result.Property = (isKeyProperty ?
                context.ApiDocumentation.Context.Create<IInverseFunctionalProperty>(propertyUri) :
                context.ApiDocumentation.Context.Create<IProperty>(propertyUri));
            result.Property.Label = property.Name;
            result.Property.Description = _xmlDocProvider.GetDescription(property);
            result.Property.Domain.Add(@class);
            IClass propertyType;
            var itemPropertyType = (System.Reflection.TypeExtensions.IsEnumerable(property.PropertyType) ? property.PropertyType : property.PropertyType.GetItemType());
            if (!context.ContainsType(itemPropertyType))
            {
                bool requiresRdf;
                var childContext = context.ForType(itemPropertyType);
                propertyType = BuildTypeDescription(childContext, out requiresRdf);
                childContext.Describe(propertyType, requiresRdf);
            }
            else
            {
                propertyType = context[itemPropertyType];
            }

            result.Property.Range.Add(propertyType);
            if ((System.Reflection.TypeExtensions.IsEnumerable(property.PropertyType)) && (property.PropertyType != typeof(byte[])))
            {
                return result;
            }

            @class.SubClassOf.Add(@class.CreateRestriction(result.Property, 1u));
            return result;
        }
    }
}