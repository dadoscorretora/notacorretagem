using DadosCorretora.NotaCorretagem.Extracao;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Cmd {
    public class Program
    {
        public static void Main()
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
            IEnumerable<IEnumerable<TextCell>> paginas = PdfToHtmlReader.Read(xmlDoc);
            foreach (var pagina in paginas)
            {
                var dados = XPBolsa2023.Extrai(pagina);
                if (dados == null)
                {
                    System.Console.Out.WriteLine("Dados não encontrados");
                }
                else
                {
                    foreach(var dado in dados) {
                        var t = dado.Transacao;
                        if (t != null)
                            System.Console.WriteLine($"t;{t.DataPregao};{t.NumeroNota};{t.Folha};{t.Operacao};{t.Titulo};{t.Quantidade};{t.Preco};{t.Valor}");
                        var c = dado.Custo;
                        if (c != null)
                            System.Console.WriteLine($"c;{c.DataPregao};{c.NumeroNota};{c.Folha};{c.CustoTaxaLiquidacao};{c.CustoTaxaRegistro};{c.CustoTotalBolsa};{c.CustoTotalCorretora};{c.IrrfSobreOperacoes}");
                    }
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
                        System.Console.WriteLine($"t,{t.DataPregao};{t.NumeroNota};{t.Folha};{t.Operacao};{t.Titulo};{t.Quantidade};{t.Preco};{t.Valor}");
                    var c = dado.Custo;
                    if (c != null)
                        System.Console.WriteLine($"c;{c.DataPregao};{c.NumeroNota};{c.Folha};{c.CustoTaxaLiquidacao};{c.CustoTaxaRegistro};{c.CustoTotalBolsa};{c.CustoTotalCorretora};{c.IrrfSobreOperacoes}");
               }
            }
        }
    }
}