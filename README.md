# dotnet-dsdl

It is a partial set of DSDL implementation in C#. DSDL is from an ancient set
of schema languages for XML documents.

Seealso: https://en.wikipedia.org/wiki/Document_Schema_Definition_Languages

It was originally developed within the Mono Project. It is rewritten to
become as modern as .NET generics and .NET Standard ~~1.3.~~ 2.0 and 2.1.

## Porting TODO

standalone_tests do not build anymore (mostly because the testSuite.zip is
insufficient and will have to be taken care elsewhere).

## Commons.Xml.Relaxng

### Introduction

RelaxngValidatingReader is an implementation to validate XML with RELAX NG
grammar. You can use full standard grammar components with it.

Currently it supports both xml syntax and compact syntax.

It supports RELAX NG predefined datatypes and XML Schema datatypes (its
DatatypeLibrary is http://www.w3.org/2001/XMLSchema-datatypes ).

All classes which is expected to use are in namespace Commons.Xml.Relaxng,
except for Commons.Xml.Relaxng.Rnc.RncParser class (which is used to parse
compact syntax).
There is another namespace Commons.Xml.Relaxng.Derivative which is not
expected to be used in users custom classes, though the classes are public
as yet.


### Grammar Object Model

RELAX NG grammar structure is represented in abstract RelaxngPattern class
and its derivations. The easiest way to get this instance is to use
static Read() method of RelaxngPattern class:

	RelaxngPattern pattern = RelaxngPattern.Read (
		new XmlTextReader ("relaxng.rng");

You can also use RelaxngPattern class to reuse grammar instance, as
already described (see Grammar Object Model).

And also, RelaxngGrammar and its contents can be created with managed code
like System.Xml.Schema.XmlSchema.


### Compact Syntax Support

Commons.Xml.Relaxng.dll also supports RELAX NG Compact Syntax. To parse
compact syntax file, use Commons.Xml.Relaxng.Rnc.RncParser class:

	RncParser parser = new RncParser (new NameTable ());
	TextReader source = new StreamReader ("relaxng.rnc");
	RelaxngGrammar grammar = parser.Parse (source);


### Validating Reader usage

RelaxngValidatingReader is used to validate XML document with RELAX NG
grammar. The usage is simple:

	XmlReader instance = new XmlTextReader ("instance.xml");
	XmlReader grammar = new XmlTextReader ("my.rng");
	RelaxngValidatingReader reader = 
		new RelaxngValidatingReader (instance, grammar);

Then Read() method (and other reading methods such as ReadElementString())
reads the instance, validating with the supplied grammar.


### Datatype support

Commons.Xml.Relaxng now supports custom datatype library. There are two 
classes from which you have to write classes derived.

* RelaxngDatatype: it represents the actual data type
* RelaxngDatatypeProvider: it provides the way to get RelaxngDatatype from QName and parameters

RelaxngDatatype class has Parse() method which is used for validation.
Basically RelaxngValidatingReader is only aware of that if the typed
elements or attributes are parsable (you can override IsValid() method
to change this behavior) using Parse() method for each datatype.

To enable custom datatype support, first you have to implement concrete
RelaxngDatatype implementations (it only requires to override Parse(), 
Name and NamespaceURI) and then RelaxngDatatypeProvider implementations
(only GetDatatype() is required) to return the datatypes you implemented
as above.

Basically RelaxngDatatypeProvider is designed to allow to contain 
datatypes in different namespaces (e.g. supporting both XML schema
datatypes and default namespace types). You can also use 
RelaxngMergedProvider class to combine existing two or more datatype
providers. However, for ease of datatype search, it is indexed by the
namespace URI (if you want to mix namespace-overrapped providers, you can
implement your own merging provider). 


### Known bugs

RELAX NG implementation should mostly work fine. A known problem is:

* URI string check is not strictly done; especially it does not check
  things described in XLink section 5.4.

## NvdlValidatingReader

### NVDL

NvdlValidatingReader is an implementation of ISO DSDL Part 4
Namespace-based Validation Dispatching Language (NVDL) which is
now FDIS.
http://www.jtc1sc34.org/repository/0694c.htm

NOTE: It is still "untested" implementation and may have limitations
and problems.

By default, NvdlValidatingReader supports RELAX NG, RELAX NG Compact
syntax, W3C XML Schema and built-in NVDL validations.

### Usage

(1) Using built-in RELAX NG support.

	NvdlRules rules = NvdlReader.Read (
		new XmlTextReader ("xhtml2-xforms.nvdl"));
	XmlReader vr = new NvdlValidatingReader (
		new XmlTextReader ("index.html"), rules);

static NvdlReader.Read() method reads argument XmlReader and return
NvdlRules instance.

NvdlValidatingReader is instantiated from a) XmlReader to be validated,
and b) NvdlRules as validating NVDL script.

(2) custom validation support

	NvdlConfig config = new NvdlConfig ();
	config.AddProvider (myOwnSchematronProvider); // [*1]
	config.AddProvider (myOwnExamplotronProvider);
	NvdlRules rules = NvdlReader.Read (
		new XmlTextReader ("myscript.nvdl"));
	XmlReader vr = new NvdlValidatingReader (
		new XmlTextReader ("myinstance.xml"), rules, config);

	NvdlConfig is here used to support "custom validation provider". In
	NVDL script, there could be any schema language referenced. I'll
	describe what validation provider is immediately later.

[*1] Of course Schematron should receive its input as XPathNavigator
or IXPathNavigable, but we could still use ReadSubtree() in .NET 2.0.
Our XPathNavigatorReader (implementation of ReadSubtree()) can be
easily modified and used in 1.x code base.

### NvdlValidationProvider

To support your own validation language, you have to design your
own extension to NvdlValidationProdiver type.

	Abstract NvdlValidationProvider should implement at least one of
	the virtual methods below:

* CreateValidatorGenerator (NvdlValidate validate,
	string schemaType,
	NvdlConfig config)
* CreateValidatorGenerator (XmlReader schema,
	NvdlConfig config)

Each of them returns NvdlValidatorGenerator implementation (will
describe later).

The first one receives MIME type (schemaType) and "validate" NVDL
element. If you don't override it, it treats only "*/*-xml" and thus
creates XmlReader from either schema attribute or schema element
and passes it to another CreateValidatorGenerator() overload.
If this (possibly overriden) method returns null, then this validation
provider does not support the MIME type or the schema document.

The second one is a shorthand method to handle "*/*-xml". By default
it just returns null.

Most of validation providers will only have to override the second
overload. Few providers such as RELAX NG Compact Syntax support will
have to overide the first overload.

### NvdlValidatorGenerator

Abstract NvdlValidatorGenerator.CreateValidator() method is designed
to create XmlReader from input XmlReader.

For example, we have NvdlXsdValidatorGenerator class. It internally
uses XmlValidatingReader which takes XmlReader as its constructor
parameter.

An instance of NvdlValidatorGenerator will be created for each 
"validate" element in the NVDL script. When the validate element
applies (for a PlanElem), it creates validator XmlReader.

