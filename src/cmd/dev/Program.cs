using DadosCorretora.NotaCorretagem.Extracao;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Cmd {
    public class Program
    {
        public static void Main(string[] args)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load("../../../reference/XPINC_NOTA_NEGOCIACAO_B3_2_2022.html");
            IEnumerable<IEnumerable<TextCell>> pageList = PdfToHtmlReader.Read(xmlDoc);
            foreach(var wordList in pageList)
            {
                var dados = XPBolsa2023.Extrai(wordList);
                if (dados == null)
                {
                    System.Console.Out.WriteLine("Dados não encontrados");
                }
                else
                {
                    System.Console.Out.WriteLine("Dados encontrados");
                }
            }
        }
    }
}