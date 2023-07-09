using System.Linq;

namespace DadosCorretora.NotaCorretagem.Parser;

public class Page
{
    public int Number { get; private set; }

    private Text[] Texts;
    private bool Sorted = false;

    public Page(int number, IEnumerable<Text> texts)
    {
        this.Number = number;
        this.Texts = texts.ToArray();
        this.EnsureSorted();
    }

    private void EnsureSorted()
    {
        if (this.Sorted)
            return;
        this.Texts = this.Texts
            .OrderBy(t => t.YMin) // Ordena de cima para baixo
            .ThenBy(t => t.XMin)  // Da direita para esqueda
            .ToArray();
        this.Sorted = true;
    }
}