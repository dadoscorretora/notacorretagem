namespace DadosCorretora.NotaCorretagem.Parser;

public record TextCell
{
    public string Text = "";
    public decimal XMin;
    public decimal XMax;
    private decimal _YMin = 0;
    private decimal _YMax = 0;
    private decimal _YMean = 0;

    public decimal YMin {
        get => this._YMin;
        set {
            this._YMin = value;
            this._YMean = ((this._YMin + this._YMax) / 2);
        }
    }

    public decimal YMax {
        get => this._YMax;
        set {
            this._YMax = value;
            this._YMean = ((this._YMin + this._YMax) / 2);
        }
    }

    public decimal YMean
    {
        get {
            return this._YMean;
        }
    }

    public bool YEquals(TextCell other)
    {
        return this.YMin == other.YMin && this.YMax == other.YMax;
    }
}

public static class TextCellExtensions
{
    public static IEnumerable<TextCell> Below(this IEnumerable<TextCell> cells, TextCell cell)
    {
        return cells.Where(data => data.YMin > cell.YMin);
    }

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
            var head = list[outer];
            if (head.Text == words[0])
            {
                for (int inner = outer, found = 0; inner < limit && found < words.Length; inner++, found++)
                {
                    var tail = list[inner];

                    if (head.YEquals(tail) == false)
                        break; // outra linha
                    if (tail.Text != words[found])
                        break; // fora de sequẽncia

                    if (found + 1 == words.Length)
                    {
                        var xmin = head.XMin;
                        if (inner + 1 < limit && head.YEquals(list[inner + 1]))
                            return new TextHeader { Text = text, XMin = xmin, XMax = list[inner + 1].XMin, };
                        else
                            return new TextHeader { Text = text, XMin = xmin };

                    }
                }
            }
        }
        throw new ArgumentException(text);
    }

    public static string InnerText(this IEnumerable<TextCell> cells)
    {
        return string.Join(" ", cells.Select(x => x.Text));
    }

    public static IEnumerable<TextCell> Left(this IEnumerable<TextCell> cells, TextCell cell)
    {
        return cells.Where(data => data.XMin < cell.XMin);
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
        return cells.Where(x => x.YEquals(first)).ReadingOrder();
    }

    public static IEnumerable<TextCell> AllStraightBelow(this IEnumerable<TextCell> cells, TextCell topCell)
    {
        return cells.Where(cell => cell.YMin > topCell.YMax)
                    .Where(cell => cell.XMax >= topCell.XMin)
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
        var skipCells = 0;
        var sequenceCursor = 0;
        foreach (var cell in cells)
        {
            if (cell.Text == sequence[sequenceCursor])
            {
                sequenceCursor++;
            }
            else
            {
                skipCells += sequenceCursor + 1;
                sequenceCursor = 0;
            }
            if (sequenceCursor == sequence.Length)
            {
                IEnumerable<TextCell> line = new List<TextCell>(cells);
                TextCell first = line.Skip(skipCells).First();
                return cells
                    .Where(x => x.YMean >= first.YMin && x.YMean <= first.YMax)
                    .Where(x => x.XMin >= first.XMin)
                    .ReadingOrder();
            }
        }
        return new List<TextCell>();
    }

    public static IEnumerable<TextCell> ReadingOrder(this IEnumerable<TextCell> cells)
    {
        return cells.OrderBy(t => t.YMax) // Ordena de cima para baixo
                    .ThenBy(t => t.XMin); // Da esquerda para direita
    }
}