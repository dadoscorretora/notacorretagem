using System.Net.Quic;
using System.Text;
using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public partial class BtgBolsa2023
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
            //FIXME Não parece ter tratamento de titulo para ticker
            transacao.Ticker = infoEspec;
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

            dt.Base = ExtraiCustoDayTrade("Day Trade: Base", quadro);
            dt.Irrf = ExtraiCustoDayTrade("IRRF Projeção", quadro);
            dt.Bruto = ExtraiCustoDayTrade("Valor Bruto:", quadro);
            dt.Corretagem = ExtraiCustoDayTrade("Corretagem:", quadro);
            dt.Emolumento = ExtraiCustoDayTrade("Emolumentos:", quadro);
            dt.TaxaLiquidacao = ExtraiCustoDayTrade("Taxa de Liquidação:", quadro);
            dt.TaxaRegistro = ExtraiCustoDayTrade("Taxa de Registro:", quadro);
            dt.TaxaAna = ExtraiCustoDayTrade("Taxa Ana:", quadro);

            yield return new Dados { Daytrade = dt };
        }
    }

    public static void Extrai(IEnumerable<TextCell> pagina, Calculo.DadosNota calculo)
    {
        foreach (var extracao in Extrai(pagina))
        {
            // Transação
            {
                var input = extracao.Transacao;
                if (input != null)
                {
                    var ticker = input.Titulo;
                    if (ticker.EndsWith("F"))
                        ticker = ticker.Substring(0, ticker.Length - 1);
                    var data = ConvertDat(input.DataPregao);
                    var quant = ConverteDec(input.Quantidade);
                    var finan = ConverteDec(input.Valor);
                    var daytd = false;

                    switch (input.Sinal)
                    {
                        case "C": break;
                        case "D": finan *= -1; break;
                        default: throw new FormatException(nameof(input.Sinal) + ": " + input.Sinal);
                    }

                    switch (input.Observacao)
                    {
                        case "": break;
                        case "D": daytd = true; break;
                        default: throw new FormatException(nameof(input.Observacao) + ": " + input.Observacao);
                    }

                    switch (input.Operacao)
                    {
                        case "C": break;
                        case "V": quant *= -1; break;
                        default: throw new FormatException(nameof(input.Operacao) + ": " + input.Operacao);
                    }

                    calculo.IncluiOperacao(input.NumeroNota, data, input.Ticker, input.Titulo, quant, finan, daytd);
                    continue;
                }
            }

            // Custo
            {
                var input = extracao.Custo;
                if (input != null)
                {
                    var data = ConvertDat(input.DataPregao);

                    var custo = Decimal.Zero;
                    custo += ConverteDecSnl(input.CustoTaxaLiquidacao);
                    custo += ConverteDecSnl(input.CustoTaxaRegistro);
                    custo += ConverteDecSnl(input.CustoTotalBolsa);
                    custo += ConverteDecSnl(input.CustoTotalCorretora);
                    var irrf = ConverteDecSnl(input.IrrfSobreOperacoes);

                    //if (custo != 0 || irrf != 0)
                    //  calculo.IncluiCusto(input.NumeroNota, data, custo, irrf , false );
                    continue;
                }
            }

            // Custo daytrade
            {
                var input = extracao.Daytrade;
                if (input != null)
                {
                    var custo = Decimal.Zero;
                    custo += ConverteDec(input.Corretagem);
                    custo += ConverteDec(input.Emolumento);
                    custo += ConverteDec(input.TaxaLiquidacao);
                    custo += ConverteDec(input.TaxaRegistro);
                    custo += ConverteDec(input.TaxaAna);
                    var irrf = ConverteDecSnl(input.Irrf);

                    //if (custo != 0 || irrf != 0)
                    //  calculo.IncluiCusto(input.NumeroNota, data, custo, irrf , true );
                    continue;
                }
            }
        }
    }

    private static string ExtraiCustoDayTrade(string prefixo, IEnumerable<TextCell> cells)
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

    private static readonly System.Globalization.CultureInfo Br = new System.Globalization.CultureInfo("pt-BR");

    private static DateOnly ConvertDat(string texto)
    {
        return DateOnly.ParseExact(texto, "dd/MM/yyyy", Br);
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
            internal string Ticker =  "";
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