using System.Diagnostics;
using Commons.Xml.Relaxng;
using System.Reflection;
using System.Text;
using System.Xml;

namespace AlpineBitsTests;

public class Tests
{
    [Test]
    public void ParseAndCompileAlpineBitsTest()
    {
        _ = ParseAndCompileAlpineBits();
        Assert.Pass();
    }

    [Test]
    public void ParseSuccessTest()
    {
        var request = OtaPingRs;
        var relaxngPattern = ParseAndCompileAlpineBits();
        using var vr = new RelaxngValidatingReader(new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(request))), relaxngPattern);
        while (!vr.EOF) vr.Read();
        Assert.Pass();
    }

    [Test]
    public void ParseFailsTest()
    {
        var request = OtaPingRs.Replace("Status=\"ALPINEBITS_HANDSHAKE\"", null);
        var relaxngPattern = ParseAndCompileAlpineBits();
        using var vr = new RelaxngValidatingReader(new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(request))), relaxngPattern);
        var ex = Assert.Throws<RelaxngException>(() =>
                                        {
                                            while (!vr.EOF) vr.Read();
                                        });
        Console.WriteLine(ex.Message);
        Assert.That(ex.Message, Is.EqualTo("Invalid start tag closing found. LocalName = Warning, NS = http://www.opentravel.org/OTA/2003/05. line 8, column 10"));
    }

    [Test]

    public void ConcurrencyTest()
    {
        // all tests can fail because of parallel execution
        // RelaxngPattern is not thread safe
        // even creating a new instance of RelaxngPattern is not thread safe
        // a lock is needed
        var request = OtaPingRs;
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream("alpinebits.rng");
        var rng = new StreamReader(resourceStream).ReadToEnd();
        Exception? error = null;
        Parallel.ForEach(Enumerable.Range(0, 1000), new ParallelOptions { MaxDegreeOfParallelism = 100 }, (s) =>
                                                                              {
                                                                                  // not thread safe
                                                                                  // even creating a new instance of RelaxngPattern is not thread safe
                                                                                  // lock (rng)
                                                                                  {
                                                                                      using var xmlReader = XmlReader.Create(new StringReader(rng));
                                                                                      var localPattern = RelaxngPattern.Read(xmlReader);
                                                                                      localPattern.Compile();
                                                                                      {
                                                                                          using var vr = new RelaxngValidatingReader(new XmlTextReader(new MemoryStream(Encoding.UTF8.GetBytes(request))), localPattern);
                                                                                          try
                                                                                          {
                                                                                              while (!vr.EOF) vr.Read();
                                                                                          }
                                                                                          catch (Exception e)
                                                                                          {
                                                                                              Console.WriteLine(e.Message);
                                                                                              error = e;
                                                                                              throw;
                                                                                          }
                                                                                      }
                                                                                  }
                                                                              });
        Assert.That(error, Is.Null);
        Console.WriteLine(error?.Message);
    }

    private RelaxngPattern ParseAndCompileAlpineBits()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceStream = assembly.GetManifestResourceStream("alpinebits.rng");
        using var textStreamReader = new StreamReader(resourceStream);
        using var xmlReader = XmlReader.Create(textStreamReader);
        var pattern = RelaxngPattern.Read(xmlReader);
        // need to patch duplicate code attribute in error element of OTA_NotifReportRS
        pattern.Compile();
        return pattern;
    }

    private const string OtaPingRs = """
                                     <?xml version="1.0" encoding="UTF-8"?>
                                     <OTA_PingRS xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                                 xmlns="http://www.opentravel.org/OTA/2003/05"
                                                 xsi:schemaLocation="http://www.opentravel.org/OTA/2003/05 OTA_PingRS.xsd"
                                                 Version="8.000">
                                         <Success/>
                                         <Warnings>
                                             <Warning Type="11" Status="ALPINEBITS_HANDSHAKE">
                                             TODO JSON
                                             </Warning>
                                         </Warnings>
                                         <EchoData>
                                         TODO JSON
                                         </EchoData>
                                     </OTA_PingRS>
                                     """;
}