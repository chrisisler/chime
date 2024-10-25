#nullable disable

using ChimeCore.Models;

public class ChimeDTO
{
    public string By { get; set; }
    public int ById { get; set; }
    public string Text { get; set; }
    public int[] Kids { get; set; }
    public string? MediaUrl { get; set; }

    public ChimeDTO() { }
    public ChimeDTO(Chime chime) =>
        (By, ById, Text, Kids, MediaUrl) = (chime.By, chime.ById, chime.Text, chime.Kids, chime.MediaUrl);
}
