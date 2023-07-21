namespace DadosCorretora.NotaCorretagem.Parser;

public record TextCell
{
    public string Text = "";
    public decimal XMin;
    public decimal XMax;
    public decimal YMin;
    public decimal YMax;

    public bool YEquals(TextCell other)
    {
        return this.YMin == other.YMin && this.YMax == other.YMax;
    }
}

public static class TextCellExtensions
{
    public static TextHeader HeaderOf(this IEnumerable<TextCell> cells, string text)
    {
        string[] words;
        if (text.IndexOf(' ') > 0)
            words = text.Split(' ');
        else
            words = new string[] { text };

        var list = cells.ReadingOrder().ToList();
        var limit = list.Count;

        for (var outer = 0; outer < limit; outer++)
        {
            var first = list[outer];
            if (first.Text == words[0])
            {
                for (int inner = outer, found = 0; inner < limit && found < words.Length; inner++, found++)
                {
                    var last = list[inner];

                    if (first.YEquals(last) == false)
                        break; // palavra em outra linha
                    if (list[inner].Text != words[found])
                        break; // palavra fora de sequẽncia

                    if (found + 1 == words.Length)
                    {
                        var xmin = first.XMin;
                        var xmax = (decimal?)null;
                        if (inner + 1 < limit && first.YEquals(list[inner + 1]))
                            xmax = list[inner + 1].XMin;
                        return new TextHeader { Text = text, XMax = xmax, XMin = xmin };
                    }
                }
            }
        }
        throw new ArgumentException(text);
    }

    public static IEnumerable<TextCell> LineBelow(this IEnumerable<TextCell> cells, TextCell cell)
    {
        var first = cells
            .Where(x => x.YMin > cell.YMin).First();
        return cells
            .Where(x => x.YMin == first.YMin)
            .ReadingOrder();
    }

    public static IEnumerable<TextCell> LineBelow(this IEnumerable<TextCell> cells, IEnumerable<TextCell> otherCells)
    {
        var above = otherCells.Last();
        var first = cells.Where(x => x.YMin > above.YMax).First();
        return cells.Where(x => x.YEquals(first));
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
}