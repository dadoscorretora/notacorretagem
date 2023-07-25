using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class BtgBolsa2023
{
    public class Dados
    {
        public DadosTransacao? Transacao;

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
    }

    public static IEnumerable<Dados> Extrai(IEnumerable<TextCell> pagina)
    {
        // Nr. nota / Folha / Data pregão

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
            var infoEspec = headEspec.Intersect(linha).Single().Text;
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

        /*
        // Custos
        {
            var headCustos = pagina.ByTextEquals("Resumo Financeiro");
            var sectCustos = pagina.Texts
                                    .Where(x => x != PdfText.Empty)
                                    .Where(x => x.H1 > headCustos.H0)
                                    .Where(x => x.V1 > headCustos.V0);

            var pageCustos = new PdfPage(sectCustos);

            var cstLiq = pageCustos.LineOf("Taxa de liquidação");
            var cstReg = pageCustos.LineOf("Taxa de Registro");
            var cstBolsa = pageCustos.LineOf("Total Bovespa / Soma");
            var cstCorre = pageCustos.LineOf("Total corretagem / Despesas");
            var cstIrrf = pageCustos.LineOf("I.R.R.F. s/ operações, base R$");

            var custo = new Result();

            custo.NumeroNota = infoNota;
            custo.Folha = infoFolh;
            custo.DataPregao = infoData;

            custo.CustoTaxaLiquidacao = CustoTsv(cstLiq);
            custo.CustoTaxaRegistro = CustoTsv(cstReg);
            custo.CustoTotalBolsa = CustoTsv(cstBolsa);
            custo.CustoTotalCorretora = CustoTsv(cstCorre);
            custo.IrrfSobreOperacoes = CustoTsv(cstIrrf); ;

            if (custo.CustoTotalCorretora.Contains("CONTINUA"))
                custo.CustoTotalCorretora = "";

            output.Add(custo);
        }

        // Detalhamento day trade

        if (pagina.Texts.Where(x => x.Text.StartsWith("Detalhamento do Day Trade:")).Any())
        {
            var limiteAcima = pagina.ByTextEquals("Resumo dos Negócios");
            var limiteDireita = pagina.ByTextEquals("Resumo Financeiro");

            var filtro = pagina.Texts
                                .Where(x => x != PdfText.Empty)
                                .Where(x => x.Below(limiteAcima))
                                .Where(x => x.Left(limiteDireita));

            var page = new PdfPage(filtro);

            var custo = new Result();

            custo.NumeroNota = infoNota;
            custo.Folha = infoFolh;
            custo.DataPregao = infoData;

            const string labelBase = "Day Trade: Base";
            const string labelIrrf = " IRRF Projeção";

            var text = page.ByTextStartsWith(labelBase);
            var pos = text.Text.IndexOf(labelIrrf);

            var textBase = text.Text.Substring(0, pos);
            var textIrrf = text.Text.Substring(pos);

            custo.Dt_Base = CustoDt(textBase, labelBase);
            custo.Dt_Irrf = CustoDt(textIrrf, labelIrrf);
            custo.Dt_Bruto = CustoDt(page, "Valor Bruto:");
            custo.Dt_Corretagem = CustoDt(page, "Corretagem:");
            custo.Dt_Emolumento = CustoDt(page, "Emolumentos:");
            custo.Dt_TaxaLiquidacao = CustoDt(page, "Taxa de Liquidação:");
            custo.Dt_TaxaRegistro = CustoDt(page, "Taxa de Registro:");
            custo.Dt_TaxaAna = CustoDt(page, "Taxa Ana:");

            output.Add(custo);
        }

        return true;
        */
    }

    /*
    private static string CustoDt(string payload, string prefix)
    {
        var pos = payload.IndexOf(prefix);
        var rest = payload.Substring(pos + prefix.Length);
        rest = rest.Replace("R$ ", "");
        rest = rest.Trim();
        return rest;
    }

    private static string CustoDt(PdfPage page, string prefix)
    {
        var text = page.ByTextStartsWith(prefix);
        var ret = CustoDt(text.Text, prefix);
        return ret;
    }

    private static string CustoTsv(List<PdfText> list)
    {
        if (list.Count <= 1)
            return "";
        if (list.Count == 2)
            return list[1].Text;

        return string.Join("\t", list.Skip(1).Select(x => x.Text));
    }
    */
}