using DadosCorretora.NotaCorretagem.Parser;

namespace DadosCorretora.NotaCorretagem.Extracao;

public class XPBolsa2023
{
    public class Dados
    {
    }

    public static Dados? Extrai(IEnumerable<TextCell> content) {

        var linhaRotulos = content.LineOfText("Nr. nota").ToList();
        return null;
    }
}