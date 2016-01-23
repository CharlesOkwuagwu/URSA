//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a URSA HTTP client proxy generation tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Dynamic;
using URSA.Web.Http;

namespace Vocab
{
    [System.CodeDom.Compiler.GeneratedCode("URSA HTTP client proxy generation tool", "1.0")]
    public partial class IProductClient : Client
    {
        public IProductClient(Uri baseUri) : base(baseUri)
        {
        }

        public System.Guid Create(Vocab.IIProduct product)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json" };
            var contentType = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle",
                "application/ld+json" };
            var result = Call<System.Guid>(Verb.POST, "/api/product/#POSTProduct", accept, contentType, uriArguments, product);
            return result;
        }

        public void Update(System.Guid id, Vocab.IIProduct product)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[0];
            var contentType = new string[] {
                "text/turtle",
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml" };
            uriArguments.id = id;
            Call(Verb.PUT, "/api/product/{id}", accept, contentType, uriArguments, product);
        }

        public System.Collections.Generic.IEnumerable<Vocab.IIProduct> List(out System.Int32 totalEntities, System.Int32 skip, System.Int32 take)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            var contentType = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            uriArguments.totalEntities = totalEntities = 0;
            uriArguments.skip = skip;
            uriArguments.take = take;
            var result = Call<System.Collections.Generic.IEnumerable<Vocab.IIProduct>>(Verb.GET, "/api/product{?skip,take}", accept, contentType, uriArguments);
            totalEntities = uriArguments.totalEntities;
            return result;
        }

        public Vocab.IIProduct Get(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "application/ld+json",
                "text/turtle" };
            var contentType = new string[] {
                "application/owl+xml",
                "application/rdf+xml",
                "application/ld+json",
                "text/turtle" };
            uriArguments.id = id;
            var result = Call<Vocab.IIProduct>(Verb.GET, "/api/product/{id}", accept, contentType, uriArguments);
            return result;
        }

        public void Delete(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[0];
            var contentType = new string[] {
                "application/ld+json",
                "application/owl+xml",
                "application/rdf+xml",
                "text/turtle" };
            uriArguments.id = id;
            Call(Verb.DELETE, "/api/product/{id}", accept, contentType, uriArguments);
        }
    }
}