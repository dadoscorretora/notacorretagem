using DadosCorretora.NotaCorretagem.Extracao;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Cmd {
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(Environment.CurrentDirectory);

            var localX = "../../../reference/XPINC_NOTA_NEGOCIACAO_B3_2_2022.html";
            var localB = "../../../../../a.html";

            if (System.IO.File.Exists(localX))
                TestXp(localX);
            if (System.IO.File.Exists(localB))
                TestBtg(localB);
        }

        public static void TestXp(string nomeArquivo)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(nomeArquivo);
            IEnumerable<IEnumerable<TextCell>> pageList = PdfToHtmlReader.Read(xmlDoc);
            foreach (var wordList in pageList)
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

        public static void TestBtg(string nomeArquivo)
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(nomeArquivo);
            var paginas = PdfToHtmlReader.Read(doc);
            foreach (var pagina in paginas)
            {
                var dados = BtgBolsa2023.Extrai(pagina).ToList();
                foreach (var dado in dados)
                {
                    var t = dado.Transacao;
                    if (t != null)
                        System.Console.WriteLine($"t {t.DataPregao} {t.NumeroNota} {t.Folha} {t.Operacao} {t.Titulo} {t.Quantidade} {t.Preco} {t.Valor}");
                }
            }
        }
    }
}