//
// RelaxngReaderTests.cs
//
// Authors:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2003 Atsushi Enomoto
//

using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using NUnit.Framework;

namespace MonoTests.Commons.Xml.Relaxng
{
	[TestFixture]
	public class RelaxngReaderTests
	{
#pragma warning disable NUnit1032
        RelaxngReader reader;
#pragma warning restore NUnit1032

        [SetUp]
		public void SetUp ()
		{
		}
		
		private void loadGrammarFromUrl (string url)
		{
			reader = new RelaxngReader (new XmlTextReader (url));
		}
		
		[Test]
		public void SimpleRead ()
		{
			loadGrammarFromUrl ("Test/XmlFiles/SimpleElementPattern1.rng");
			RelaxngPattern p = reader.ReadPattern ();

			Assert.AreEqual (RelaxngPatternType.Element, p.PatternType);
		}

		[Test]
		public void CompileRelaxngGrammar ()
		{
			loadGrammarFromUrl ("Test/XmlFiles/relaxng.rng");
			RelaxngPattern p = reader.ReadPattern ();

			Assert.AreEqual (RelaxngPatternType.Grammar, p.PatternType);

			p.Compile ();
		}

		[Test]
		public void Bug347945 ()
		{
			string rng = @"
<element name='x' xmlns='http://relaxng.org/ns/structure/1.0'>
  <interleave>
    <element name='y'><text/></element>
    <element name='z'><text/></element>
  </interleave>
</element>";
			RelaxngPattern p = RelaxngPattern.Read (new XmlTextReader (rng, XmlNodeType.Document, null));
		}
	}
}
