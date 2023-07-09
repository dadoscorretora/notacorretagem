namespace DadosCorretora.NotaCorretagem.Parser;

public record Text
{
    public string Content = "";
    public decimal XMin { get; }
    public decimal XMax { get; }
    public decimal YMin { get; }
    public decimal YMax { get; }
}