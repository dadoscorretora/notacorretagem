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
        var first = cells
            .Where(x => x.YMin > cell.YMin).First();
        return cells
            .Where(x => x.YMin == first.YMin)
            .ReadingOrder();
    }

    public static IEnumerable<TextCell> LineOfText(this IEnumerable<TextCell> cells, string text)
    {
        string[] tokens;
        if (text.Contains(' '))
        {
            tokens = text.Split(' ');
        }
        else
        {
            tokens = new string[] { text };
        }
        return cells.LineOfText(tokens);
    }

    public static IEnumerable<TextCell> LineOfText(this IEnumerable<TextCell> cells, string[] sequence)
    {
        IEnumerable<TextCell> cellsCursor = new List<TextCell>(cells);
        var sequenceCursor = 0;
        foreach (var cell in cells)
        {
            if (cell.Text == sequence[sequenceCursor])
            {
                sequenceCursor++;
            }
            else
            {
                cellsCursor = cellsCursor.Skip(sequenceCursor + 1);
                sequenceCursor = 0;
            }
            if (sequenceCursor == sequence.Length)
            {
                TextCell first = cellsCursor.First();
                return cells
                    .Where(x => x.YMin == first.YMin)
                    .ReadingOrder();
            }
        }
        return new List<TextCell>();
    }

    public static IEnumerable<TextCell> ReadingOrder(this IEnumerable<TextCell> cells)
    {
        return cells.OrderBy(t => t.YMin) // Ordena de cima para baixo
            .ThenBy(t => t.XMin); // Da esquerda para direita
    }

    public static IEnumerable<TextCell> TextEquals(this IEnumerable<TextCell> cells, string text)
    {
        return cells.Where(x => x.Text == text);
    }

    public static IEnumerable<TextCell> MergeCells(this IEnumerable<TextCell> cells, decimal mergeSpan)
    {
        throw new NotImplementedException();
    }

}