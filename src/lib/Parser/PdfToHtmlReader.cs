using System.Xml;

namespace DadosCorretora.NotaCorretagem.Parser
{

    public class PdfToHtmlReader
    {      
        public static List<TextCell> Read(XmlDocument xmlDoc)
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
            var textCellList = new List<TextCell>();
            ReadPageList(pageList, textCellList);
            textCellList = textCellList
                .OrderBy(t => t.YMin) // Ordena de cima para baixo
                .ThenBy(t => t.XMin).ToList(); // Da direita para esqueda
            return textCellList;
        }

        private static void ReadPageList(XmlNodeList pageList, List<TextCell> textCellList)
        {
            foreach (var pageNode in pageList)
            {
                var page = pageNode as XmlElement;
                if (page == null || !page.HasChildNodes)
                {
                    throw new System.Exception("The page node is invalid or has no child nodes.");
                }
                var wordList = page.GetElementsByTagName("word");
                ReadWordList(textCellList, wordList);
            }
        }

        private static void ReadWordList(List<TextCell> textCellList, XmlNodeList wordList)
        {
            foreach (var wordNode in wordList)
            {
                var word = wordNode as XmlElement;
                if (word == null)
                {
                    throw new System.Exception("No word nodes found.");
                }
                var fGetDecimal = (XmlElement element, string attributeName) =>
                {
                    var attribute = element.GetAttribute(attributeName);
                    if (attribute == null)
                    {
                        throw new System.Exception($"The attribute {attributeName} is invalid or empty.");
                    }
                    return decimal.Parse(attribute, System.Globalization.CultureInfo.InvariantCulture);
                };
                var textCell = new TextCell();
                textCell.XMax = fGetDecimal(word, "xMax");
                textCell.XMin = fGetDecimal(word, "xMin");
                textCell.YMax = fGetDecimal(word, "yMax");
                textCell.YMin = fGetDecimal(word, "yMin");
                textCell.Text = word.InnerText;
                textCellList.Append(textCell);
            }
        }
    }
}