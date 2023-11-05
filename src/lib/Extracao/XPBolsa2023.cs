using System.Globalization;
using DadosCorretora.NotaCorretagem.Calculo;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class XPBolsa2023
{
    private static readonly System.Globalization.CultureInfo Br = new System.Globalization.CultureInfo("pt-BR");
    public static IEnumerable<Dados> ExtraiDeHTML_BBox(IEnumerable<TextCell> pagina)
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
            var taxaOperacional = blocoCustos.LineOfText("Taxa Operacional").Skip(2).InnerText();
            var custoExecucao = blocoCustos.LineOfText("Execução").Skip(1).InnerText();
            var custoCustodia = blocoCustos.LineOfText("Taxa de Custódia").Skip(3).InnerText();
            var custoImposto = blocoCustos.LineOfText("Impostos").Skip(1).InnerText();
            var irrf = blocoCustos.LineOfText("I.R.R.F. s/ operações, base").Skip(5).InnerText(); //skip(4+1) por conta do valor liquido
            var custoOutros = blocoCustos.LineOfText("Outros").Skip(1).InnerText();

            if (totalBovespa != "CONTINUA..." && (custoLiquidacao.Length + custoRegistro.Length + totalBovespa.Length + irrf.Length > 0))
            {
                var custo = new Dados.DadosCusto
                {
                    NumeroNota = nrNota,
                    Folha = nrFolha,
                    DataPregao = dataPregao,
                    CustoTaxaLiquidacao = custoLiquidacao,
                    CustoTaxaRegistro = custoRegistro,
                    CustoTotalBolsa = totalBovespa,
                    CustoTaxaOperacional = taxaOperacional,
                    CustoExecucao = custoExecucao,
                    CustoTaxaCustodia = custoCustodia,
                    CustoImposto = custoImposto,
                    IrrfSobreOperacoes = irrf,
                    CustoOutros = custoOutros
                };

                yield return new Dados { Custo = custo };
            }
        }
    }

    private static decimal ConverteDec(string texto)
    {
        if (texto == null || texto.Length == 0)
            return 0;
        return decimal.Parse(texto, Br);
    }
    
    private static decimal ConverteDecSnl(string texto)
    {
        if (texto.EndsWith(" C"))
        {
            texto = texto.Substring(0, texto.Length - 2).Trim();
            return ConverteDec(texto);
        }
        if (texto.EndsWith(" D"))
        {
            texto = texto.Substring(0, texto.Length - 2).Trim();
            return ConverteDec(texto) * -1;
        }
        return 0;
    }

    public static DadosNota TraduzDadosTipados(IEnumerable<Dados> dados)
    {
        var dadosNota = new DadosNota();

        dados.ToList().ForEach(dado =>
        {
            var t = dado.Transacao;
            if (t != null)
            {
                var transacao = new DadosNota.Operacao();
                transacao.NumeroNota = dado.Transacao!.NumeroNota;
                transacao.DataNota = DateOnly.ParseExact(dado.Transacao.DataPregao, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                transacao.CodigoAtivo = dado.Transacao.Titulo;
                transacao.Quantidade = ConverteDec(dado.Transacao.Quantidade);
                transacao.ValorOperacao = ConverteDecSnl($"{dado.Transacao.Valor} {dado.Transacao.Sinal}");
                dadosNota.AcumulaOperacao(transacao);
            }
            var c = dado.Custo;
            if (c != null)
            {
                var custo = new DadosNota.Custo();
                custo.NumeroNota = dado.Custo!.NumeroNota;
                custo.DataPregao = DateOnly.ParseExact(dado.Custo.DataPregao, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //custo total
                var custoTaxaLiquidacao = ConverteDecSnl(dado.Custo.CustoTaxaLiquidacao);
                var custoTaxaRegistro = ConverteDecSnl(dado.Custo.CustoTaxaRegistro);
                var custoTotalBolsa = ConverteDecSnl(dado.Custo.CustoTotalBolsa);
                var custoTaxaOperacional = ConverteDecSnl(dado.Custo.CustoTaxaOperacional);
                var custoExecucao = ConverteDecSnl(dado.Custo.CustoExecucao);
                var custoTaxaCustodia = ConverteDecSnl(dado.Custo.CustoTaxaCustodia);
                var custoImposto = ConverteDecSnl(dado.Custo.CustoImposto);
                var custoOutros = ConverteDecSnl(dado.Custo.CustoOutros);
                custo.CustoTotal = 
                    custoTaxaLiquidacao + 
                    custoTaxaRegistro + 
                    custoTotalBolsa + 
                    custoTaxaOperacional + 
                    custoExecucao + 
                    custoTaxaCustodia + 
                    custoImposto + 
                    custoOutros;
                //irrf
                custo.IrrfSobreOperacoes = ConverteDecSnl(dado.Custo.IrrfSobreOperacoes);//se não tiver d ou c o valor é zero
                dadosNota.AcumulaCusto(custo);
            }
        });

        return dadosNota;
    }
}