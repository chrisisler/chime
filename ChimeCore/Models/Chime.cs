#nullable disable

namespace ChimeCore.Models
{
    /// Design inspired by: https://github.com/HackerNews/API?tab=readme-ov-file#items
    public partial class Item
    {
        public int Id { get; set; }
        public Boolean Deleted { get; set; }
        /// One of "chime", "comment"
        public string Type { get; set; }
        /// username of the item's author
        public string By { get; set; }
        /// ID of the item's author
        public int ById { get; set; }
        /// creation date of the item, in Unix Time
        public int Time { get; set; }
        /// content of the item
        public string Text { get; set; }
        /// the comment's parent, if it has one
        public int? ParentId { get; set; }
        /// IDs of the children of the item
        public int[] Kids { get; set; }
        /// URL of attached image, gif, video, etc. if any
        public string? MediaUrl { get; set; }
    }

    public class Chime : Item
    {
        public Chime(string by, int byId, string text, int[] kids, string? mediaUrl)
        {
            Deleted = false;
            Type = "chime";
            By = by;
            ById = byId;
            Text = Text;
            Time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Kids = kids;
            MediaUrl = mediaUrl;
            ParentId = null; // Chimes don't have a parent
        }
    }

    public partial class Comment : Item
    {
        public Comment(string by, int byId, string text, int? parentId, int[] kids, string? mediaUrl)
        {
            Deleted = false;
            Type = "comment";
            By = by;
            ById = byId;
            Time = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Text = text;
            Kids = kids;
            MediaUrl = mediaUrl;
            ParentId = parentId;
        }
    }
}

