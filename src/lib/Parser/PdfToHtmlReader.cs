using System.Xml;

namespace DadosCorretora.NotaCorretagem.Parser
{

    public class PdfToHtmlReader
    {
        public static List<List<TextCell>> Read(XmlDocument xmlDoc)
        {
            if (xmlDoc == null || !xmlDoc.HasChildNodes)
            {
                throw new System.Exception("Xml document is invalid or has no child nodes.");
            }
            var pageList = xmlDoc.GetElementsByTagName("page");
            if (pageList == null)
            {
                throw new System.Exception("No page nodes found.");
            }
            var textCellList = ReadPageList(pageList);
            return textCellList;
        }

        private static List<List<TextCell>> ReadPageList(XmlNodeList pageList)
        {
            List<List<TextCell>> textCellList = new();
            foreach (XmlElement pageNode in pageList)
            {
                if (pageNode == null || !pageNode.HasChildNodes)
                {
                    throw new System.Exception("The page node is invalid or has no child nodes.");
                }
                var wordList = pageNode.GetElementsByTagName("word");
                List<TextCell> readWordList = ReadWordList(wordList);
                textCellList.Add(readWordList);
            }
            return textCellList;
        }

        private static List<TextCell> ReadWordList(XmlNodeList wordList)
        {
            List<TextCell> readWordList = new();
            foreach (XmlElement wordNode in wordList)
            {
                if (wordNode == null)
                {
                    throw new System.Exception("No word nodes found.");
                }
                static decimal fGetDecimal(XmlElement element, string attributeName)
                {
                    var attribute = element.GetAttribute(attributeName);
                    if (attribute == null)
                    {
                        throw new System.Exception($"The attribute {attributeName} is invalid or empty.");
                    }
                    return decimal.Parse(attribute, System.Globalization.CultureInfo.InvariantCulture);
                }
                var textCell = new TextCell
                {
                    XMax = fGetDecimal(wordNode, "xMax"),
                    XMin = fGetDecimal(wordNode, "xMin"),
                    YMax = fGetDecimal(wordNode, "yMax"),
                    YMin = fGetDecimal(wordNode, "yMin"),
                    Text = wordNode.InnerText
                };
                readWordList.Add(textCell);
            }
            return readWordList.ReadingOrder();
        }
    }
}