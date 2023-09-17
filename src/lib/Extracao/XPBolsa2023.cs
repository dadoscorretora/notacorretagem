using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class XPBolsa2023
{
    public static IEnumerable<Dados> Extrai(IEnumerable<TextCell> pagina)
    {
        var linhaRotulosNota = pagina.LineOfText("Nr. nota Folha Data pregão");

        var headNrNota = linhaRotulosNota.HeaderOf("Nr. nota");
        var headFolha = linhaRotulosNota.HeaderOf("Folha");
        var headDataPregao = linhaRotulosNota.HeaderOf("Data pregão");

        var linhaInfoNota = pagina.LineBelow(linhaRotulosNota);

        var nrNota = headNrNota.Intersect(linhaInfoNota).Single().Text;
        var nrFolha = headFolha.Intersect(linhaInfoNota).Single().Text;
        var dataPregao = headDataPregao.Intersect(linhaInfoNota).Single().Text;

        if (nrNota.Length == 0 | nrFolha.Length == 0 || dataPregao.Length == 0)
            yield break;

        var COL_Q = "Q";
        var COL_CV = "C/V";
        var COL_ESPECIFICACAO_TITULO = "Especificação do título";
        var COL_OBS = "Obs. (*)";
        var COL_QUANTIDADE = "Quantidade";
        var COL_PRECO_AJUSTE = "Preço / Ajuste";
        var COL_VALOR_OPERACAO_AJUSTE = "Valor Operação / Ajuste";
        var COL_DC = "D/C";

        var linha = pagina.LineOfText(COL_Q);

        var headOperacao = linha.HeaderOf(COL_CV);
        var headEspecificacao = linha.HeaderOf(COL_ESPECIFICACAO_TITULO);
        var headObservacao = linha.HeaderOf(COL_OBS);
        var headQuantidade = linha.HeaderOf(COL_QUANTIDADE);
        var headPreco = linha.HeaderOf(COL_PRECO_AJUSTE);
        var headValor = linha.HeaderOf(COL_VALOR_OPERACAO_AJUSTE);
        var headSinal = linha.HeaderOf(COL_DC);

        while (true)
        {
            var transacao = new Dados.DadosTransacao
            {
                NumeroNota = nrNota,
                Folha = nrFolha,
                DataPregao = dataPregao
            };

            linha = pagina.LineBelow(linha);
            if (linha.LineOfText("Resumo dos Negócios").Any() || linha.LineOfText("Resumo Financeiro").Any())
                break;

            var operacao = headOperacao.Intersect(linha).Single().Text;
            var especificacao = headEspecificacao.Intersect(linha).InnerText();
            var observacao = headObservacao.Intersect(linha).SingleOrDefault()?.Text ?? "";
            var quantidade = headQuantidade.Intersect(linha).Single().Text;
            var preco = headPreco.Intersect(linha).Single().Text;
            var valor = headValor.Intersect(linha).Single().Text;
            var sinal = headSinal.Intersect(linha).Single().Text;

            transacao.Operacao = operacao;
            transacao.Titulo = especificacao;
            transacao.Observacao = observacao;
            transacao.Quantidade = quantidade;
            transacao.Preco = preco;
            transacao.Valor = valor;
            transacao.Sinal = sinal;

            yield return new Dados { Transacao = transacao };
        }

        {
            var celulaTituloCustos = pagina.LineOfText("Resumo Financeiro").First();

            var blocoCustos = pagina.AllStraightBelow(celulaTituloCustos);

            var custoLiquidacao = blocoCustos.LineOfText("Taxa de liquidação").Skip(3).InnerText();
            var custoRegistro = blocoCustos.LineOfText("Taxa de Registro").Skip(3).InnerText();
            var totalBovespa = blocoCustos.LineOfText("Total Bovespa / Soma").Skip(4).InnerText();
            var totalCustos = blocoCustos.LineOfText("Total Custos / Despesas").Skip(4).InnerText();
            var irrf = blocoCustos.LineOfText("I.R.R.F. s/ operações, base").Skip(5).InnerText(); //skip(4+1) por conta do valor liquido

            if (custoLiquidacao.Length + custoRegistro.Length + totalBovespa.Length + totalCustos.Length + irrf.Length > 0)
            {
                var custo = new Dados.DadosCusto
                {
                    NumeroNota = nrNota,
                    Folha = nrFolha,
                    DataPregao = dataPregao,
                    CustoTaxaLiquidacao = custoLiquidacao,
                    CustoTaxaRegistro = custoRegistro,
                    CustoTotalBolsa = totalBovespa,
                    CustoTotalCorretora = totalCustos,
                    IrrfSobreOperacoes = irrf
                };

                yield return new Dados { Custo = custo };
            }
        }
    }
}