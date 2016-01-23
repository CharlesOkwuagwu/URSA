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

namespace URSA.Example.WebApplication.Data
{
    [System.CodeDom.Compiler.GeneratedCode("URSA HTTP client proxy generation tool", "1.0")]
    public partial class PersonClient : Client
    {
        public PersonClient(Uri baseUri) : base(baseUri)
        {
        }

        public System.Guid Create(URSA.Example.WebApplication.Data.IPerson person)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/json",
                "text/xml",
                "application/xml" };
            var contentType = new string[] {
                "application/json",
                "text/xml",
                "application/xml" };
            var result = Call<System.Guid>(Verb.POST, "/api/person/#POSTPerson", accept, contentType, uriArguments, person);
            return result;
        }

        public void Delete(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[0];
            var contentType = new string[] {
                "application/json",
                "application/xml",
                "text/xml" };
            uriArguments.id = id;
            Call(Verb.DELETE, "/api/person/{id}", accept, contentType, uriArguments);
        }

        public System.Collections.Generic.IEnumerable<URSA.Example.WebApplication.Data.Person> List(out System.Int32 totalEntities, System.Int32 take, System.Int32 skip)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/xml",
                "text/xml",
                "application/json" };
            var contentType = new string[] {
                "application/xml",
                "text/xml",
                "application/json" };
            uriArguments.totalEntities = totalEntities = 0;
            uriArguments.take = take;
            uriArguments.skip = skip;
            var result = Call<System.Collections.Generic.IEnumerable<URSA.Example.WebApplication.Data.Person>>(Verb.GET, "/api/person{?skip,take}", accept, contentType, uriArguments);
            totalEntities = uriArguments.totalEntities;
            return result;
        }

        public URSA.Example.WebApplication.Data.Person Get(System.Guid id)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[] {
                "application/json",
                "application/xml",
                "text/xml" };
            var contentType = new string[] {
                "application/json",
                "application/xml",
                "text/xml" };
            uriArguments.id = id;
            var result = Call<URSA.Example.WebApplication.Data.Person>(Verb.GET, "/api/person/{id}", accept, contentType, uriArguments);
            return result;
        }

        public void Update(System.Guid id, URSA.Example.WebApplication.Data.IPerson person)
        {
            dynamic uriArguments = new ExpandoObject();
            var accept = new string[0];
            var contentType = new string[] {
                "application/json",
                "application/xml",
                "text/xml" };
            uriArguments.id = id;
            Call(Verb.PUT, "/api/person/{id}", accept, contentType, uriArguments, person);
        }
    }
}