using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace OwncloudUniversal.Shared.WebDav
{
    /// <summary>
    /// Represents the parser for response's results.
    /// </summary>
    internal static class ResponseParser
    {
        /// <summary>
        /// Parses the disk item.
        /// </summary>
        /// <param name="stream">The response text.</param>
        /// <returns>The  parsed item.</returns>
        public static DavItem ParseItem(Stream stream)
        {
            return ParseItems(stream).FirstOrDefault();
        }

        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Replace("\0x09", string.Empty);
        }

        /// <summary>
        /// Parses the disk items.
        /// </summary>
        /// <param name="stream">The response text.</param>
        /// <returns>The list of parsed items.</returns>
        public static IEnumerable<DavItem> ParseItems(Stream stream)
        {
            var items = new List<DavItem>();
            //var sr = new StreamReader(stream, Encoding.UTF8, false, 4096, false);
            //var content = (sr.ReadToEnd());
            //TextReader r = new StringReader(content);
            XmlReaderSettings s = new XmlReaderSettings();
            s.IgnoreWhitespace = true;
            using (var reader = XmlReader.Create(stream, s))
            {
                DavItem davItemInfo = null;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.LocalName.ToLower())
                        {
                            case "response":
                                davItemInfo = new DavItem();
                                break;
                            case "href":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    var value = reader.Value;
                                    value = value.Replace("#", "%23");
                                    davItemInfo.Href = value;
                                }
                                break;
                            case "creationdate":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    DateTime creationdate;
                                    if (DateTime.TryParse(reader.Value, out creationdate))
                                        davItemInfo.CreationDate = creationdate;
                                }
                                break;
                            case "getlastmodified":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    DateTime lastmodified;
                                    if (DateTime.TryParse(reader.Value, out lastmodified))
                                        davItemInfo.LastModified = lastmodified;
                                }
                                break;
                            case "displayname":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    davItemInfo.DisplayName = reader.Value;
                                }
                                break;
                            case "getcontentlength":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    int contentLength;
                                    if (int.TryParse(reader.Value, out contentLength))
                                        davItemInfo.ContentLength = contentLength;
                                }
                                break;
                            case "getcontenttype":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    davItemInfo.ContentType = reader.Value;
                                }
                                break;
                            case "getetag":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    davItemInfo.Etag = reader.Value;
                                }
                                break;
                            case "iscollection":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    bool isCollection;
                                    if (bool.TryParse(reader.Value, out isCollection))
                                        davItemInfo.IsCollection = isCollection;
                                    int isCollectionInt;
                                    if (int.TryParse(reader.Value, out isCollectionInt))
                                        davItemInfo.IsCollection = isCollectionInt == 1;
                                }
                                break;
                            case "resourcetype":
                                if (!reader.IsEmptyElement)
                                {
                                    reader.Read();
                                    var resourceType = reader.LocalName.ToLower();
                                    if (string.Equals(resourceType, "collection", StringComparison.CurrentCultureIgnoreCase))
                                        davItemInfo.IsCollection = true;
                                }
                                break;
                            case "hidden":
                            case "ishidden":
                                davItemInfo.IsHidden = true;
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName.ToLower() == "response")
                    {
                        // Remove trailing / if the item is not a collection
                        var href = davItemInfo.Href.TrimEnd('/');
                        if (!davItemInfo.IsCollection)
                        {
                            davItemInfo.Href = href;
                        }
                        if (string.IsNullOrEmpty(davItemInfo.DisplayName))
                        {
                            var name = href.Substring(href.LastIndexOf('/') + 1);
                            davItemInfo.DisplayName = WebUtility.UrlDecode(name);
                        }
                        items.Add(davItemInfo);
                    }
                }
                //sr.Dispose();
            }

            return items;
        }


    }
}
