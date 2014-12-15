using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using de.l3s.boilerpipe;
using de.l3s.boilerpipe.extractors;
using de.l3s.boilerpipe.sax;
using HtmlAgilityPack;

namespace WPFCentralServer
{
    public class LinksExtractor
    {
        private static string DocumentReadibility(byte[] input)
        {   // extrahuje hlavnu cast webovej stranky (input)
            string result = null;
            try
            {
                if (input != null)
                {
                    org.xml.sax.InputSource is1 = new org.xml.sax.InputSource(new java.io.ByteArrayInputStream(input));
                    org.xml.sax.InputSource is2 = new org.xml.sax.InputSource(new java.io.ByteArrayInputStream(input));

                    HTMLHighlighter hh = HTMLHighlighter.newExtractingInstance();
                    BoilerpipeExtractor extractor = CommonExtractors.CANOLA_EXTRACTOR; // pouzitie Canola Extractor
                    de.l3s.boilerpipe.sax.BoilerpipeSAXInput sax = new BoilerpipeSAXInput(is1); // nacitanie vstupu
                    de.l3s.boilerpipe.document.TextDocument td = sax.getTextDocument(); // ziskanie TextDocument

                    extractor.process(td);  // spracovanie extractorom
                    result = hh.process(td, is2);   // ziskanie vysledku po extrakcii
                }
            }
            catch { }
            return result ?? "";
        }

        public static List<string> GetFirstLinks(byte[] document, string url, int count, Predicate<string> blacklist = null)
        {   // vrati zvoleny pocet prvych hlavnych odkazov na stranke (document)
            // moze pouzit vyradit zakazane odkazy (predikat blacklist)
            List<string> ret = null;
            try
            {
                string readable = DocumentReadibility(document);  // extrahuje hlavnu cast textu
                if (!String.IsNullOrWhiteSpace(readable))
                {
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(readable);   // nacitanie HTML dokumentu
                    HtmlNode docNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body");
                    url = url.TrimEnd('/');
                    Uri uri = new Uri(url); // adresa stranky, ktoru spracuvame

                    ret = docNode
                        .SelectNodes("//a/@href")   // vyber odkazov z dokumentu a ich ocistenie
                        .Select(a => Regex.Replace(a.GetAttributeValue("href", ""), "#.*$", "").TrimEnd('/'))
                        .Select(a => new Uri(uri, a)) // ak su odkazy relativne, ziskame absolutne
                        .Where(u => !u.Equals(uri))   // ak odkazuje na povodnu stranku
                        .Take(count)   // maximalny zvoleny pocet
                        .Select(u => u.AbsoluteUri) // ziskanie odkazu
                        .Where(a => blacklist == null || blacklist(a) == false) // overenie ci nie je zakazana
                        .ToList();
                }
            }
            catch { }

            return ret ?? new List<string>();
        }
    }
}
