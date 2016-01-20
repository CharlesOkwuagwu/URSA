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

namespace Ns.Hydra
{
    [System.CodeDom.Compiler.GeneratedCode("URSA HTTP client proxy generation tool", "1.0")]
    public partial class CoreClient : Client
    {
        public CoreClient(Uri baseUri) : base(baseUri)
        {
        }

        public System.Collections.Generic.IEnumerable<Ns.Hydra.ICore> GetApiEntryPointDescription()
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/owl+xml",
                "application/ld+json",
                "application/rdf+xml",
                "text/turtle" };
            var contentType = new string[] {
                "application/owl+xml",
                "application/ld+json",
                "application/rdf+xml",
                "text/turtle" };
            var result = Call<System.Collections.Generic.IEnumerable<Ns.Hydra.ICore>>(Verb.OPTIONS, "/api", accept, contentType, uriArguments);
            return result;
        }

        public System.Collections.Generic.IEnumerable<Ns.Hydra.ICore> GetApiEntryPointDescription()
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
            var result = Call<System.Collections.Generic.IEnumerable<Ns.Hydra.ICore>>(Verb.GET, "/api?format=Xml", accept, contentType, uriArguments);
            return result;
        }
    }
}