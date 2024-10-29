#nullable disable

using ChimeCore.Models;

public class ItemDTO
{
    public string By { get; set; }
    public int ById { get; set; }
    public string Text { get; set; }
    public int[] Kids { get; set; } = Array.Empty<int>();
    public string Type { get; set; }
    public int? ParentId { get; set; }

    public ItemDTO() { }

    public ItemDTO(Chime c) =>
        (By, ById, Text, Kids, Type) = (c.By, c.ById, c.Text, c.Kids, "chime");

    public ItemDTO(Item i) =>
        (By, ById, Text, Kids, Type, ParentId) = (i.By, i.ById, i.Text, i.Kids, i.Type, i.ParentId);

    public ItemDTO(string by, int byId, string text, int[] kids, string type, int? parentId) =>
        (this.By, this.ById, this.Text, this.Kids, this.Type, this.ParentId) = (by, byId, text, kids, type, parentId);
}
