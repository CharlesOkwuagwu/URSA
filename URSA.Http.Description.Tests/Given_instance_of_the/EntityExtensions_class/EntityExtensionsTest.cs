﻿using System;
using RomanticWeb;
using RomanticWeb.Configuration;
using RomanticWeb.DotNetRDF;
using RomanticWeb.Entities;
using URSA.Configuration;
using URSA.Web.Http.Description.Tests.Data;
using VDS.RDF;
using System.Linq;
using NUnit.Framework;

namespace Given_instance_of_the.EntityExtensions_class
{
    [TestFixture]
    public abstract class EntityExtensionsTest
    {
        protected ITripleStore SourceStore { get; private set; }

        protected ITripleStore TargetStore { get; private set; }

        protected IEntityContext SourceEntityContext { get; private set; }

        protected IEntityContext TargetEntityContext { get; private set; }

        protected IProduct SourceInstance { get; private set; }

        protected IProduct TargetInstance { get; set; }

        [SetUp]
        public virtual void Setup()
        {
            var metaGraphUri = ConfigurationSectionHandler.Default.Factories.Cast<FactoryElement>().First(factory => factory.Name == DescriptionConfigurationSection.Default.DefaultStoreFactoryName).MetaGraphUri;
            ITripleStore store;
            SourceEntityContext = SetupEntityContext(metaGraphUri, out store);
            SourceStore = store;
            TargetEntityContext = SetupEntityContext(metaGraphUri, out store);
            TargetStore = store;
            SourceInstance = SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/product/"));
            SourceInstance.Name = "Test";
            SourceInstance.Price = 0;
            SourceInstance.Key = Guid.NewGuid();
            SourceInstance.RelatedProduct = SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/related-product/"));
            SourceInstance.Similar.Add(SourceEntityContext.Create<IProduct>(new EntityId("http://temp.uri/similar-product/")));
            SourceEntityContext.Commit();
        }

        private IEntityContext SetupEntityContext(Uri metaGraphUri, out ITripleStore store)
        {
            store = new TripleStore();
            store.Add(new Graph() { BaseUri = metaGraphUri });
            return new EntityContextFactory()
                .WithMetaGraphUri(metaGraphUri)
                .WithDefaultOntologies()
                .WithMappings(load => load.FromAssemblyOf<IProduct>())
                .WithDotNetRDF(store)
                .CreateContext();
        }
    }
}