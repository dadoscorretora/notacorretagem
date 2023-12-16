namespace DadosCorretora.NotaCorretagem.Calculo;

public class RateioDespesa
{
    public static void Calcular(DadosNota dadosNota)
    {
        var nrNotas = dadosNota.Custos.GroupBy(c => c.NumeroNota);
        if (nrNotas.Any(g => g.Count() > 1))
        {
            throw new Exception("Mais de um custo para a mesma nota. Provável erro na extração");
        }
        dadosNota.Custos.ToList().ForEach(custo =>
        {
            var operacoes = dadosNota.Operacoes
                .Where(o => o.NumeroNota == custo.NumeroNota)
                .OrderByDescending(o => Math.Abs(o.ValorOperacao));

            var restoCusto = DistribuiRateio(operacoes, custo!.CustoTotal, MidpointRounding.ToZero);

            if (restoCusto < 0)
            {
                restoCusto = DistribuiRateio(operacoes, restoCusto, MidpointRounding.AwayFromZero);
            }

            for (var opIndex = 0; restoCusto < 0 && opIndex < operacoes.Count(); opIndex++)
            {
                var operacao = operacoes.ElementAt(opIndex);
                operacao.CustoOperacao -= 0.01m;
                restoCusto += 0.01m;
            }

            if (restoCusto != 0)
            {
                throw new Exception("Falha na rateio de custos. Residual: " + restoCusto);
            }
        });
    }

    private static decimal DistribuiRateio(IEnumerable<DadosNota.Operacao> operacoes, decimal custoTotal, MidpointRounding tratamentoFracoes)
    {
        var restoCusto = custoTotal;
        var totalOperacoes = operacoes.Sum(o => Math.Abs(o.ValorOperacao));

        operacoes.ToList().ForEach(operacao =>
        {
            if (restoCusto != 0)
            {
                decimal percentual = Math.Abs(operacao.ValorOperacao) / totalOperacoes;
                var custoRateado = Math.Round(custoTotal * percentual, 2, tratamentoFracoes);
                operacao.CustoOperacao += custoRateado;
                restoCusto -= custoRateado;
            }
        });

        return restoCusto;
    }
}