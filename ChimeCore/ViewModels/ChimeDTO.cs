#nullable disable

using ChimeCore.Models;

public class ChimeDTO
{
    public string By { get; set; }
    public int ById { get; set; }
    public string Text { get; set; }
    public int[] Kids { get; set; } = Array.Empty<int>();

    public ChimeDTO() { }

    public ChimeDTO(Chime chime) =>
        (By, ById, Text, Kids) = (chime.By, chime.ById, chime.Text, chime.Kids);

    public ChimeDTO(string by, int byId, string text, int[] kids) =>
        (this.By, this.ById, this.Text, this.Kids) = (by, byId, text, kids);
}
