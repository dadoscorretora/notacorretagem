using System.Net.Quic;
using System.Text;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class BtgBolsa2023
{
    public static IEnumerable<Dados> Extrai(IEnumerable<TextCell> pagina)
    {
        // Cabeçalho

        var linha = pagina.LineOfText("Nr. nota");
        var headNota = linha.HeaderOf("Nr. nota");
        var headFolh = linha.HeaderOf("Folha");
        var headData = linha.HeaderOf("Data pregão");

        linha = pagina.LineBelow(linha);
        var infoNota = headNota.Intersect(linha).Single().Text;
        var infoFolh = headFolh.Intersect(linha).Single().Text;
        var infoData = headData.Intersect(linha).Single().Text;

        if (infoNota.Length == 0 | infoFolh.Length == 0 || infoData.Length == 0)
            yield break;

        // Negócios

        linha = pagina.LineOfText("Q");

        var headOpera = linha.HeaderOf("C/V");
        var headEspec = linha.HeaderOf("Especificação do título");
        var headObser = linha.HeaderOf("Obs. (*)");
        var headQuant = linha.HeaderOf("Quantidade");
        var headPreco = linha.HeaderOf("Preço / Ajuste");
        var headValor = linha.HeaderOf("Valor Operação / Ajuste");
        var headSinal = linha.HeaderOf("D/C");

        while (true)
        {
            var transacao = new Dados.DadosTransacao
            {
                NumeroNota = infoNota,
                Folha = infoFolh,
                DataPregao = infoData
            };

            linha = pagina.LineBelow(linha);
            if (linha.LineOfText("Resumo dos Negócios").Any() || linha.LineOfText("Resumo Financeiro").Any())
                break;

            var infoOpera = headOpera.Intersect(linha).Single().Text;
            var infoEspec = headEspec.Intersect(linha).InnerText();
            var infoObser = headObser.Intersect(linha).SingleOrDefault()?.Text ?? "";
            var infoQuant = headQuant.Intersect(linha).Single().Text;
            var infoPreco = headPreco.Intersect(linha).Single().Text;
            var infoValor = headValor.Intersect(linha).Single().Text;
            var infoSinal = headSinal.Intersect(linha).Single().Text;

            transacao.Operacao = infoOpera;
            transacao.Titulo = infoEspec;
            transacao.Observacao = infoObser;
            transacao.Quantidade = infoQuant;
            transacao.Preco = infoPreco;
            transacao.Valor = infoValor;
            transacao.Sinal = infoSinal;

            yield return new Dados { Transacao = transacao };
        }

        // Custo
        {
            var celulaTitulo = pagina.LineOfText("Resumo Financeiro").First();
            var blocoCustos = pagina.AllStraightBelow(celulaTitulo);

            var custLiq = blocoCustos.LineOfText("Taxa de liquidação").Skip(3).InnerText();
            var custReg = blocoCustos.LineOfText("Taxa de Registro").Skip(3).InnerText();
            var custBls = blocoCustos.LineOfText("Total Bovespa / Soma").Skip(4).InnerText();
            var custCrr = blocoCustos.LineOfText("Total corretagem / Despesas").Skip(4).InnerText();
            var custIrf = blocoCustos.LineOfText("I.R.R.F. s/ operações, base R$").Skip(6).InnerText(); // skip+1

            if (custCrr.Contains("CONTINUA"))
                custCrr = "";

            if (custLiq.Length + custReg.Length + custBls.Length + custCrr.Length + custIrf.Length > 0)
            {
                var custo = new Dados.DadosCusto
                {
                    NumeroNota = infoNota,
                    Folha = infoFolh,
                    DataPregao = infoData,
                    CustoTaxaLiquidacao = custLiq,
                    CustoTaxaRegistro = custReg,
                    CustoTotalBolsa = custBls,
                    CustoTotalCorretora = custCrr,
                    IrrfSobreOperacoes = custIrf
                };

                yield return new Dados { Custo = custo };
            }
        }

        // Day trade

        if (pagina.LineOfText("Detalhamento do Day Trade:").Any())
        {
            var limiteAcima = pagina.LineOfText("Resumo dos Negócios").First();
            var limiteDireita = pagina.LineOfText("Resumo Financeiro").First();

            var quadro = pagina.Below(limiteAcima)
                               .Left(limiteDireita)
                               .ToList();

            var dt = new Dados.DadosDaytrade
            {
                NumeroNota = infoNota,
                Folha = infoFolh,
                DataPregao = infoData
            };

            dt.Base = CustoDt("Day Trade: Base", quadro);
            dt.Irrf = CustoDt("IRRF Projeção", quadro);
            dt.Bruto = CustoDt("Valor Bruto:", quadro);
            dt.Corretagem = CustoDt("Corretagem:", quadro);
            dt.Emolumento = CustoDt("Emolumentos:", quadro);
            dt.TaxaLiquidacao = CustoDt("Taxa de Liquidação:", quadro);
            dt.TaxaRegistro = CustoDt("Taxa de Registro:", quadro);
            dt.TaxaAna = CustoDt("Taxa Ana:", quadro);

            yield return new Dados { Daytrade = dt };
        }
    }

    private static string CustoDt(string prefixo, IEnumerable<TextCell> cells)
    {
        var linha = cells.LineOfText(prefixo).InnerText();
        var buffer = new StringBuilder(16);
        // Avança para depois do prefixo
        var pos = linha.IndexOf(prefixo);
        if (pos < 0)
            throw new FormatException("Esperado '" + prefixo + "'.");
        pos += prefixo.Length;
        // ws
        while (linha[pos] == ' ')
            pos++;
        // Possível sinal
        if (linha[pos] == '-')
        {
            buffer.Append('-');
            pos++;
        }
        // Sifrão
        if (linha[pos] == 'R' && linha[pos + 1] == '$')
            pos += 2;
        else
            throw new FormatException("Esperado 'R$'.");
        // ws
        while (linha[pos] == ' ')
            pos++;
        // finalmente, os números
        while (pos < linha.Length)
        {
            var chr = linha[pos];
            if ((chr >= '0' && chr <= '9') || chr == ',')
            {
                buffer.Append(chr);
                pos++;
            }
            else
                break;
        }
        return buffer.ToString();
    }

    // Retorno principal, texto

    public class Dados
    {
        public DadosTransacao? Transacao;
        public DadosCusto? Custo;
        public DadosDaytrade? Daytrade;

        public class DadosTransacao
        {
            public string NumeroNota = "";
            public string Folha = "";
            public string DataPregao = "";

            public string Operacao = "";
            public string Titulo = "";
            public string Observacao = "";
            public string Quantidade = "";
            public string Preco = "";
            public string Valor = "";
            public string Sinal = "";
        }
        public class DadosCusto
        {
            public string NumeroNota = "";
            public string Folha = "";
            public string DataPregao = "";

            public string CustoTaxaLiquidacao = "";
            public string CustoTaxaRegistro = "";
            public string CustoTotalBolsa = "";
            public string CustoTotalCorretora = "";
            public string IrrfSobreOperacoes = "";
        }
        public class DadosDaytrade
        {
            public string NumeroNota = "";
            public string Folha = "";
            public string DataPregao = "";

            public string Base = "";
            public string Irrf = "";
            public string Bruto = "";
            public string Corretagem = "";
            public string Emolumento = "";
            public string TaxaLiquidacao = "";
            public string TaxaRegistro = "";
            public string TaxaAna = "";
        }
    }
}