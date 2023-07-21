using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class BtgBolsa2023
{
    public class Dados
    {
    }

    private static Dados? Extrai(IEnumerable<TextCell> pagina)
    {
        // Nr. nota / Folha / Data pregão

        var linha = pagina.LineOfText("Nr. nota");
        var headNota = linha.HeaderOf("Nr. nota");
        var headFolh = linha.HeaderOf("Folha");
        var headData = linha.HeaderOf("Data");

        linha = linha.LineBelow(linha);
        var infoNota = headNota.Intersec(linha).Single().Text;
        var infoFolh = headFolh.Intersec(linha).Single().Text;
        var infoData = headData.Intersec(linha).Single().Text;

        if (infoNota.Length == 0 | infoFolh.Length == 0 || infoData.Length == 0)
            return null;

        // Negócios
        /*
        label = pagina.LineOfText("Especificação do título");

        var headOpera = label.Where(x => x.Text == "C/V").Single();
        var headEspec = label.Where(x => x.Text == "Especificação do título").Single();
        var headObser = label.Where(x => x.Text == "Obs. (*)").Single();
        var headQuant = label.Where(x => x.Text == "Quantidade").Single();
        var headPreco = label.Where(x => x.Text == "Preço / Ajuste").Single();
        var headValor = label.Where(x => x.Text == "Valor Operação / Ajuste").Single();
        var headSinal = label.Where(x => x.Text == "D/C").Single();

        var lineAbove = label.First();
        while (true)
        {
            var operacao = new Result();

            operacao.NumeroNota = infoNota;
            operacao.Folha = infoFolh;
            operacao.DataPregao = infoData;

            linha = pagina.LineBelow(lineAbove);
            if (linha.Where(x => x.Text == "Resumo dos Negócios" || x.Text == "Resumo Financeiro").Any())
                break;
            if (linha.Count == 0)
                break;
            else
                lineAbove = linha.First();

            var infoOpera = PdfMath.IntersectH(headOpera, linha).Single();
            var infoEspec = PdfMath.IntersectH(headEspec, linha).First();
            var infoObser = PdfMath.IntersectH(headObser, linha).SingleOrDefault();
            var infoQuant = PdfMath.IntersectH(headQuant, linha).Single();
            var infoPreco = PdfMath.IntersectH(headPreco, linha).Single();
            var infoValor = PdfMath.IntersectH(headValor, linha).Single();
            var infoSinal = PdfMath.IntersectH(headSinal, linha).Single();

            operacao.Operacao = infoOpera.Text;
            operacao.Titulo = infoEspec.Text;
            operacao.Observacao = infoObser == null ? "" : infoObser.Text;
            operacao.Quantidade = infoQuant.Text;
            operacao.Preco = infoPreco.Text;
            operacao.Valor = infoValor.Text;
            operacao.Sinal = infoSinal.Text;

            output.Add(operacao);
        }

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
        return null;
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