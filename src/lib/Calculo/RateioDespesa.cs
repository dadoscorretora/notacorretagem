namespace DadosCorretora.NotaCorretagem.Calculo;

public class RateioDespesa
{
    public static void Calcular(DadosNota dadosNota)
    {
        var nrNotas = dadosNota.Custos.GroupBy(c => c.NumeroNota);
        if (nrNotas.Any( g => g.Count() > 1))
        {
            throw new Exception("Mais de um custo para a mesma nota. Provável erro na extração.");
        }
        dadosNota.Custos.ToList().ForEach(custo =>
        {
            var operacoes = dadosNota.Operacoes.Where(o => o.NumeroNota == custo.NumeroNota).OrderBy(o => o.ValorOperacao);
            var totalOperacoes = operacoes.Sum(o => o.ValorOperacao);
            var custoTotal = custo!.CustoTotal;
            decimal restoCusto = custoTotal;
            restoCusto = DistribuiRateio(operacoes, totalOperacoes, custoTotal, restoCusto);

            if (restoCusto > 0)
            {
                throw new Exception("Arrendondamento do rateio maior que o custo.");
            }

            custoTotal = restoCusto;

            if (restoCusto != 0)
            {
                restoCusto = DistribuiRateio(operacoes, totalOperacoes, restoCusto, restoCusto);
            }

            var opCount = 0;
            while (restoCusto != 0) {
                var operacao = operacoes.ElementAt(opCount);
                restoCusto += 0.01m;
                operacao.CustoOperacao -= 0.01m;
                opCount++;
            }
        });

    }

    private static decimal DistribuiRateio(IEnumerable<DadosNota.Operacao> operacoes, decimal totalOperacoes, decimal custoTotal, decimal restoCusto)
    {
        operacoes.ToList().ForEach(operacao =>
        {
            decimal percentual = operacao.ValorOperacao / totalOperacoes;
            var custoRateado = Math.Round(custoTotal * percentual, 2, MidpointRounding.ToZero);
            operacao.CustoOperacao += custoRateado;
            restoCusto -= custoRateado;
        });
        return restoCusto;
    }
}