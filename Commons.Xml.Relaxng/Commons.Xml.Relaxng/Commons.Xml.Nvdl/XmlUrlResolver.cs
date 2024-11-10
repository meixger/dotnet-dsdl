using System;
using System.Net;
using System.Xml;

namespace Commons.Xml
{
	class XmlUrlResolver : XmlResolver
	{
        public override object? GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
        {
            throw new NotImplementedException("TODO - need to decide if resolve file and remote uris");
            // 	var wr = WebRequest.Create (uri);
            // 	return wr.GetResponseAsync ().Result.GetResponseStream ();
        }
    }
}