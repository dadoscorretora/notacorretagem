namespace DadosCorretora.NotaCorretagem.Parser;

public record TextHeader
{
    public string Text = "";
    public decimal XMin;
    public decimal? XMax;

    public IEnumerable<TextCell> Intersec(IEnumerable<TextCell> cells)
    {
        foreach (var cell in cells)
        {
            if (this.XMax == null)
            {
                if (this.XMin <= cell.XMin)
                    yield return cell;
                if (this.XMin <= cell.XMax)
                    yield return cell;
            }
            else
            {
                if (this.XMin <= cell.XMin && cell.XMin <= this.XMax)
                    yield return cell;
                if (this.XMin <= cell.XMax && cell.XMax <= this.XMax)
                    yield return cell;
            }
        }
    }
}