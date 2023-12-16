namespace DadosCorretora.NotaCorretagem.Extracao;

public class Dados
{
    public DadosTransacao? Transacao;
    public DadosCusto? Custo;

    public class DadosTransacao
    {
        public string NumeroNota = "";
        public string Folha = "";
        public string DataPregao = "";
        public string Operacao = "";
        public string Titulo = "";
        public string CodigoAtivo = "";
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
        public string CustoTaxaOperacional = "";
        public string CustoExecucao = "";
        public string CustoTaxaCustodia = "";
        public string CustoImposto = "";
        public string IrrfSobreOperacoes = "";
        public string CustoOutros = "";
    }
}