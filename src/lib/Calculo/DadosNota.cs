using DadosCorretora.NotaCorretagem.Extracao;

namespace DadosCorretora.NotaCorretagem.Calculo;

public class DadosNota
{
    public DadosNota()
    {
        Operacoes = new List<Operacao>();
        Custos = new List<Custo>();
    }

    public List<Operacao> Operacoes;

    public List<Custo> Custos;

    public class Operacao
    {
        public string NumeroNota = "";
        public DateOnly DataNota;
        public string Ticker = "";
        public string Titulo = "";
        public string CodigoAtivo = "";
        public decimal Quantidade;
        public decimal ValorOperacao;
        public decimal Preco { get { return ValorOperacao / Quantidade; } }
        public decimal CustoOperacao = 0;
    }
    public class Custo
    {
        public string NumeroNota = "";
        public DateOnly DataPregao;
        public decimal CustoTotal;
        public decimal IrrfSobreOperacoes;
    }

    public void AcumulaCusto(DadosNota.Custo custo)
    {
        this.Custos.Add(custo);
    }

    public void AcumulaOperacao(DadosNota.Operacao operacao)
    {
        this.Operacoes.Add(operacao);
    }

    internal void IncluiOperacao(
        string numeroNota, DateOnly data, 
        string ticker, string titulo, 
        decimal quant, decimal finan, 
        bool daytd)
    {
        var result = this.Operacoes.Where(o =>
                o.NumeroNota == numeroNota && 
                o.DataNota == data && 
                (o.Ticker == ticker || 
                o.Titulo == titulo)
            );
        if (result.Count() == 0) {
            Operacao operacao = new()
            {
                NumeroNota = numeroNota,
                DataNota = data,
                Ticker = ticker,
                Titulo = titulo,
                Quantidade = quant,
                ValorOperacao = finan,
                CustoOperacao = 0,
                
            };
            this.Operacoes.Add(operacao);
        } else if (result.Count() == 1) {
            var operacao = result.First();
            operacao.Quantidade += quant;
            operacao.ValorOperacao += finan;
        } else {
            throw new Exception("Mais de uma operação com os mesmos parâmetros foi incluida.");
        }
    }
}
