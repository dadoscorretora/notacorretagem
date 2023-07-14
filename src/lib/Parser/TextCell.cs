namespace DadosCorretora.NotaCorretagem.Parser;

public record TextCell
{
    public string Text = "";
    public decimal XMin;
    public decimal XMax;
    public decimal YMin;
    public decimal YMax;
}

public static class TextCellExtensions
{
    public static IEnumerable<TextCell> LineBelow(this IEnumerable<TextCell> cells, TextCell cell)
    {
        throw new NotImplementedException();
    }

    public static IEnumerable<TextCell> LineOfText(this IEnumerable<TextCell> cells, string text)
    {
        var tokens = new string[] { text };
        if (text.Contains(" "))
        {
            tokens = text.Split(" ");
        }
        var found = cells.LineOfText(tokens);
        return found;
    }

    public static IEnumerable<TextCell> LineOfText(this IEnumerable<TextCell> cells, string[] sequence) 
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