using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class XPBolsa2023
{
    public class Dados
    {
        public string numero = "";
        public string folha = "";
        public string data = "";
    }

    public class Operacao {
        
    }

    public static Dados? Extrai(List<TextCell> pagina) {

        var rotulosNota = pagina.LineOfText("Nr. nota").ToList();
        var infoNota = pagina.LineBelow(rotulosNota.First()).ToList();

        if (rotulosNota.Count != 5)
            return null;

        if(rotulosNota[0].Text != "Nr." ||
            rotulosNota[1].Text != "nota" ||
            rotulosNota[2].Text != "Folha" ||
            rotulosNota[3].Text != "Data" ||
            rotulosNota[4].Text != "pregão")
            return null;

        var dados = new Dados
        {
            numero = infoNota[0].Text,
            folha = infoNota[1].Text,
            data = infoNota[2].Text
        };

        if (dados.numero.Length == 0 
            || dados.folha.Length == 0 
            || dados.data.Length == 0)
            return null;
            
        //var COL_Q = "Q";//0
        //var COL_NEGOCIACAO = "Negociação";//1
        //var COL_CV = "C/V";//2
        //var COL_TIPO_MERCADO = "Tipo mercado";//3,4***
        //var COL_PRAZO = "Prazo";//5
        var COL_ESPECIFICACAO_TITULO = "Especificação do título";//6...8****
        //var COL_OBS = "Obs.(*)";//9
        //var COL_QUANTIDADE = "Quantidade";//10
        //var COL_PRECO_AJUSTE = "Preço / Ajuste";//11...13****
        //var COL_VALOR_OPERACAO_AJUSTE = "Valor Operação / Ajuste";//14...17****
        //var COL_DC = "D/C";//18
        var merge = new [,] { {3, 4}, {6, 8}, {11, 13}, {14, 17} };
        var cab_negocios = pagina.LineOfText(COL_ESPECIFICACAO_TITULO).ToList();

        var ini = cab_negocios.First();
        var colTipoMercado = new TextCell
        {
            Text = "",
            XMin = cab_negocios[3].XMin,
            XMax = cab_negocios[4].XMax,
            YMin = cab_negocios[3].YMin,
            YMax = cab_negocios[3].YMax
        };
        colTipoMercado.Text += $"{cab_negocios[3].Text}";
        colTipoMercado.Text += $" {cab_negocios[4].Text}";

        var colEspecificacaoTitulo = new TextCell
        {
            Text = "",
            XMin = cab_negocios[6].XMin,
            XMax = cab_negocios[8].XMax,
            YMin = cab_negocios[6].YMin,
            YMax = cab_negocios[6].YMax
        };
        colEspecificacaoTitulo.Text += $"{cab_negocios[6].Text}";
        colEspecificacaoTitulo.Text += $" {cab_negocios[7].Text}";
        colEspecificacaoTitulo.Text += $" {cab_negocios[8].Text}";

        var colPrecoAjuste = new TextCell
        {
            Text = "",
            XMin = cab_negocios[11].XMin,
            XMax = cab_negocios[13].XMax,
            YMin = cab_negocios[11].YMin,
            YMax = cab_negocios[11].YMax
        };
        colPrecoAjuste.Text += $"{cab_negocios[11].Text}";
        colPrecoAjuste.Text += $" {cab_negocios[12].Text}";
        colPrecoAjuste.Text += $" {cab_negocios[13].Text}";

        var colValorOperacaoAjuste = new TextCell
        {
            Text = "",
            XMin = cab_negocios[14].XMin,
            XMax = cab_negocios[17].XMax,
            YMin = cab_negocios[14].YMin,
            YMax = cab_negocios[14].YMax
        };
        colValorOperacaoAjuste.Text += $"{cab_negocios[14].Text}";
        colValorOperacaoAjuste.Text += $" {cab_negocios[15].Text}";
        colValorOperacaoAjuste.Text += $" {cab_negocios[16].Text}";
        colValorOperacaoAjuste.Text += $" {cab_negocios[17].Text}";


        var fim = pagina.Where(x => x.Text == "Resumo").First();

        var vals = pagina
            .Where(x => x.YMin > ini.YMin && x.YMin < fim.YMin);
        

        vals.Aggregate(new List<Operacao>(), (acc, x) => {
            throw new NotImplementedException();
        });     

        return dados;
    }
}