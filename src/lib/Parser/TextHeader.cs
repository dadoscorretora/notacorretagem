namespace DadosCorretora.NotaCorretagem.Parser;

public record TextHeader
{
    public string Text = "";
    public decimal XMin;
    public decimal? XMax;

    public IEnumerable<TextCell> Intersect(IEnumerable<TextCell> cells)
    {
        foreach (var cell in cells)
        {
            if (this.XMax == null)
            {
                if (cell.XMin >= this.XMin)
                {
                    yield return cell; // start inside
                    continue;
                }
                if (cell.XMax >= this.XMin)
                {
                    yield return cell; // end inside
                    continue;
                }
            }
            else
            {
                if (cell.XMin >= this.XMin && cell.XMin <= this.XMax)
                {
                    yield return cell; // start inside
                    continue;
                }
                if (cell.XMax >= this.XMin && cell.XMax <= this.XMax)
                {
                    yield return cell; // end inside
                    continue;
                }
                if (cell.XMin <= this.XMin && cell.XMax >= this.XMax)
                {
                    yield return cell; // start and end outside
                    continue;
                }
            }
        }
    }
}