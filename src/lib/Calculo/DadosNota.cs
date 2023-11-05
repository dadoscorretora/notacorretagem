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
        public string CodigoAtivo = "";
        public decimal Quantidade;
        public decimal ValorOperacao;
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
}
