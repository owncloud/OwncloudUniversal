using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OwncloudUniversal.WebDav
{
    internal static class XmlParser
    {
        public static List<DavItem> ParsePropfind(Stream stream)
        {
            List<DavItem> davItems = new List<DavItem>();
            XNamespace namepace = "DAV:" ;
            XDocument doc = XDocument.Load(stream);
            var elements = from response in doc.Descendants(namepace + "response") select response;
            foreach (var xElement in elements)
            {
                var davItem = new DavItem();
                davItem.Href = xElement.Element(namepace + "href")?.Value;
                davItem.Etag = xElement.Descendants(namepace + "gettag").FirstOrDefault()?.Value;
                davItem.ChangeKey = xElement.Descendants(namepace + "getetag").FirstOrDefault()?.Value;
                davItem.Size = Convert.ToUInt64(xElement.Descendants(namepace + "getcontentlength").FirstOrDefault()?.Value);
                davItem.LastModified = Convert.ToDateTime(xElement.Descendants(namepace + "getlastmodified").FirstOrDefault()?.Value);
                davItem.DisplayName = xElement.Descendants(namepace + "displayname").FirstOrDefault()?.Value;
                davItem.ContentType = xElement.Descendants(namepace + "getcontenttype").FirstOrDefault()?.Value;

                davItem.IsCollection = (bool)!xElement.Descendants(namepace + "resourcetype").FirstOrDefault()?.IsEmpty;
                davItems.Add(davItem);
            }
            return davItems;
        }
    }
}