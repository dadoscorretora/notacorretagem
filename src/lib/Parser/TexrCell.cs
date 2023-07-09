namespace DadosCorretora.NotaCorretagem.Parser;

public record TextCell
{
    public string Text = "";
    public decimal XMin { get; }
    public decimal XMax { get; }
    public decimal YMin { get; }
    public decimal YMax { get; }
}

public static class TextCellExtensions
{
    public static IEnumerable<TextCell> LineBelow(this IEnumerable<TextCell> cells, TextCell cell)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<TextCell> LineOfText(this IEnumerable<TextCell> cells, string text)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<TextCell> MergeCells(this IEnumerable<TextCell> cells, decimal factor)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<TextCell> TextEquals(this IEnumerable<TextCell> cells, string text)
    {
        return cells.Where(x => x.Text == text);
    }
}