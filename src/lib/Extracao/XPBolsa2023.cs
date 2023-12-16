using System.Globalization;
using DadosCorretora.NotaCorretagem.Calculo;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class XPBolsa2023
{
    private const string COL_Q = "Q";
    private const string COL_CV = "C/V";
    private const string COL_ESPECIFICACAO_TITULO = "Especificação do título";
    private const string COL_OBS = "Obs. (*)";
    private const string COL_QUANTIDADE = "Quantidade";
    private const string COL_PRECO_AJUSTE = "Preço / Ajuste";
    private const string COL_VALOR_OPERACAO_AJUSTE = "Valor Operação / Ajuste";
    private const string COL_DC = "D/C";
    private static readonly CultureInfo Br = new CultureInfo("pt-BR");
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
            linha = pagina.LineBelow(linha);
            if (linha.LineOfText("Resumo dos Negócios").Any() || linha.LineOfText("Resumo Financeiro").Any())
                break;

            var operacao = headOperacao.Intersect(linha).Single().Text;
            var especGroupCell = headEspecificacao.Intersect(linha);
            
            bool findTicker(IEnumerable<TextCell> group, TextCell cell, int pos) {
                    return cell.Text.Length > 4 &&
                    group.ElementAtOrDefault(pos-1) != null &&
                    group.ElementAtOrDefault(pos+1) != null && 
                    Math.Abs(cell.XMin - group.ElementAt(pos-1).XMax) > 8 &&
                    Math.Abs(cell.XMin - group.ElementAt(pos+1).XMax) > 8;
            }

            string TryGuessTicker(IEnumerable<TextCell> celulasTitulo) {
                var titulo = celulasTitulo.InnerText();
                if (titulo.Contains("EZTEC")) {
                    return "EZTEC3";
                } 
                else if (titulo.Contains("HELBOR"))
                {
                    return "HBOR3";
                }
                else if (titulo.Contains("TECNISA"))
                {
                    return "TCSA3";
                }
                else if (titulo.Contains("MELIUZ"))
                {
                    return "CASH3";
                }
                Console.Error.WriteLine($"AVISO: Não foi possível encontrar o ticker do pregão de título '{titulo}'.");
                return "";
            }

            string TickerOuTitulo(IEnumerable<TextCell> celulasTitulo) {
                IEnumerable<TextCell> found = celulasTitulo.Where( (cell, position) => 
                    findTicker(celulasTitulo, cell, position));
                if (found.Count() == 1) {
                    return found.Single().Text;
                } else {
                    return TryGuessTicker(celulasTitulo);
                }
            }

            var titulo = especGroupCell.InnerText();
            var codigoAtivo = TickerOuTitulo(especGroupCell);
            var observacao = headObservacao.Intersect(linha).SingleOrDefault()?.Text ?? "";
            var quantidade = headQuantidade.Intersect(linha).Single().Text;
            var preco = headPreco.Intersect(linha).Single().Text;
            var valor = headValor.Intersect(linha).Single().Text;
            var sinal = headSinal.Intersect(linha).Single().Text;

            var transacao = new Dados.DadosTransacao
            {
                NumeroNota = nrNota,
                Folha = nrFolha,
                DataPregao = dataPregao,
                Operacao = operacao,
                Titulo = titulo,
                CodigoAtivo = codigoAtivo,
                Observacao = observacao,
                Quantidade = quantidade,
                Preco = preco,
                Valor = valor,
                Sinal = sinal
            };

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

    private static decimal ConverteDecSnl(string texto, string sinal)
    {
        if (sinal.Trim() == "C")
        {
            return ConverteDec(texto);
        }
        if (sinal.Trim() == "D")
        {
            return ConverteDec(texto) * -1;
        }
        return 0;
    }

    public static DateOnly ConverteData(string data)
    {
        return DateOnly.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public static DadosNota TraduzDadosTipados(IEnumerable<Dados> dados)
    {
        var dadosNota = new DadosNota();

        dados.ToList().ForEach(dado =>
        {
            var rTrn = dado.Transacao;
            if (rTrn != null)
            {
                var quantidade = ConverteDec(rTrn.Quantidade);
                quantidade = rTrn.Operacao == "V" ? quantidade * -1 : quantidade;
                var transacao = new DadosNota.Operacao
                {
                    NumeroNota = rTrn.NumeroNota,
                    Titulo = rTrn.Titulo,
                    CodigoAtivo = rTrn.CodigoAtivo,
                    Quantidade = quantidade,
                    ValorOperacao = ConverteDecSnl(rTrn.Valor, rTrn.Sinal),
                    DataNota = ConverteData(rTrn.DataPregao)
                };
                dadosNota.AcumulaOperacao(transacao);
            }

            var rCusto = dado.Custo;
            if (rCusto != null)
            {
                //custo total
                var custoTaxaLiquidacao = ConverteDecSnl(rCusto.CustoTaxaLiquidacao);
                var custoTaxaRegistro = ConverteDecSnl(rCusto.CustoTaxaRegistro);
                var custoTotalBolsa = ConverteDecSnl(rCusto.CustoTotalBolsa);
                var custoTaxaOperacional = ConverteDecSnl(rCusto.CustoTaxaOperacional);
                var custoExecucao = ConverteDecSnl(rCusto.CustoExecucao);
                var custoTaxaCustodia = ConverteDecSnl(rCusto.CustoTaxaCustodia);
                var custoImposto = ConverteDecSnl(rCusto.CustoImposto);
                var custoOutros = ConverteDecSnl(rCusto.CustoOutros);
                var custoTotal =
                    custoTaxaLiquidacao +
                    custoTaxaRegistro +
                    custoTotalBolsa +
                    custoTaxaOperacional +
                    custoExecucao +
                    custoTaxaCustodia +
                    custoImposto +
                    custoOutros;

                //irrf
                //se não tiver d ou c o valor é zero
                var custoIrrfSobreOperacoes = ConverteDecSnl(rCusto.IrrfSobreOperacoes);

                var custo = new DadosNota.Custo
                {
                    NumeroNota = rCusto.NumeroNota,
                    DataPregao = ConverteData(rCusto.DataPregao),
                    CustoTotal = custoTotal,
                    IrrfSobreOperacoes = custoIrrfSobreOperacoes
                };

                dadosNota.AcumulaCusto(custo);
            }
        });

        return dadosNota;
    }
}