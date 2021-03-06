﻿/*globals xsd, rdf, rdfs, owl, guid, hydra, ursa, odata */
(function() {
    "use strict";

    window.xsd = new String("http://www.w3.org/2001/XMLSchema#"); //jshint ignore:line
    xsd.string = xsd + "string";
    xsd.boolean = xsd + "boolean";
    xsd.byte = xsd + "byte";
    xsd.unsignedByte = xsd + "unsignedByte";
    xsd.short = xsd + "short";
    xsd.unsignedShort = xsd + "unsignedShort";
    xsd.int = xsd + "int";
    xsd.unsignedInt = xsd + "unsignedInt";
    xsd.long = xsd + "long";
    xsd.unsignedLong = xsd + "unsignedLong";
    xsd.integer = xsd + "integer";
    xsd.nonPositiveInteger = xsd + "nonPositiveInteger";
    xsd.nonNegativeInteger = xsd + "nonNegativeInteger";
    xsd.positiveInteger = xsd + "positiveInteger";
    xsd.negativeInteger = xsd + "negativeInteger";
    xsd.float = xsd + "float";
    xsd.double = xsd + "double";
    xsd.decimal = xsd + "decimal";
    xsd.dateTime = xsd + "dateTime";
    xsd.time = xsd + "time";
    xsd.date = xsd + "date";
    xsd.gYear = xsd + "gYear";
    xsd.gMonth = xsd + "gMonth";
    xsd.gDay = xsd + "gDay";
    xsd.gYearMonth = xsd + "gYearMonth";

    window.rdf = new String("http://www.w3.org/1999/02/22-rdf-syntax-ns#"); //jshint ignore:line
    rdf.subject = rdf + "subject";
    rdf.first = rdf + "first";
    rdf.last = rdf + "last";
    rdf.Property = rdf + "Property";
    rdf.List = rdf + "List";

    window.rdfs = new String("http://www.w3.org/2000/01/rdf-schema#"); //jshint ignore:line
    rdfs.Class = rdfs + "Class";
    rdfs.subClassOf = rdfs + "subClassOf";
    rdfs.range = rdfs + "range";
    rdfs.label = rdfs + "label";
    rdfs.comment = rdfs + "comment";

    window.owl = new String("http://www.w3.org/2002/07/owl#"); //jshint ignore:line
    owl.onProperty = owl + "onProperty";
    owl.minCardinality = owl + "minCardinality";
    owl.maxCardinality = owl + "maxCardinality";
    owl.allValuesFrom = owl + "allValuesFrom";
    owl.InverseFunctionalProperty = owl + "InverseFunctionalProperty";
    owl.Restriction = owl + "Restriction";
    owl.Thing = owl + "Thing";

    window.hydra = new String("http://www.w3.org/ns/hydra/core#"); //jshint ignore:line
    hydra.Resource = hydra + "Resource";
    hydra.Class = hydra + "Class";
    hydra.Operation = hydra + "Operation";
    hydra.SupportedProperty = hydra + "SupportedProperty";
    hydra.Link = hydra + "Link";
    hydra.TemplatedLink = hydra + "TemplatedLink";
    hydra.IriTemplate = hydra + "IriTemplate";
    hydra.IriTemplateMapping = hydra + "IriTemplateMapping";
    hydra.ApiDocumentation = hydra + "ApiDocumentation";
    hydra.CreateResourceOperation = hydra + "CreateResourceOperation";
    hydra.ReplaceResourceOperation = hydra + "ReplaceResourceOperation";
    hydra.DeleteResourceOperation = hydra + "DeleteResourceOperation";
    hydra.Collection = hydra + "Collection";
    hydra.PartialCollectionView = hydra + "PartialCollectionView";
    hydra.entrypoint = hydra + "entrypoint";
    hydra.property = hydra + "property";
    hydra.supportedProperty = hydra + "supportedProperty";
    hydra.supportedOperation = hydra + "supportedOperation";
    hydra.supportedClass = hydra + "supportedClass";
    hydra.readable = hydra + "readable";
    hydra.writeable = hydra + "writeable";
    hydra.required = hydra + "required";
    hydra.expects = hydra + "expects";
    hydra.returns = hydra + "returns";
    hydra.method = hydra + "method";
    hydra.statusCode = hydra + "statusCode";
    hydra.operation = hydra + "operation";
    hydra.template = hydra + "template";
    hydra.mapping = hydra + "mapping";
    hydra.variable = hydra + "variable";
    hydra.title = hydra + "title";
    hydra.description = hydra + "description";
    hydra.manages = hydra + "manages";
    hydra.member = hydra + "member";
    hydra.totalItems = hydra + "totalItems";

    window.ursa = new String("http://alien-mcl.github.io/URSA/vocabulary#"); //jshint ignore:line
    ursa.mediaType = ursa + "mediaType";

    window.odata = new String("http://docs.oasis-open.org/odata/odata/v4.0/"); //jshint ignore:line
    odata.skip = odata + "$skip";
    odata.top = odata + "$top";
    odata.filter = odata + "$filter";

    window.shacl = new String("http://www.w3.org/ns/shacl#"); //jshint ignore:line

    window.guid = new String("http://openguid.net/rdf#"); //jshint ignore:line
    guid.guid = guid + "guid";
}());