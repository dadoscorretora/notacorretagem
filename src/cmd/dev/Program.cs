using DadosCorretora.NotaCorretagem.Extracao;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Cmd {
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.Out.WriteLine("Hello World!");
            System.Console.Out.WriteLine(System.IO.Directory.GetCurrentDirectory());
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load("../../../reference/XPINC_NOTA_NEGOCIACAO_B3_2_2022.html");
            List<TextCell> textCellList = PdfToHtmlReader.Read(xmlDoc);
            var dados = XPBolsa2023.Extrai(textCellList);
        }
    }
}