#nullable disable

namespace ChimeCore.Models
{
    /// Design inspired by: https://github.com/HackerNews/API?tab=readme-ov-file#items
    public abstract class Item
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
        public int Text { get; set; }
        /// the comment's parent
        public int ParentId { get; set; }
        /// IDs of the children of the item
        public int[] Kids { get; set; }
        /// URL of attached image, gif, video, etc. if any
        public string? MediaUrl { get; set; }
    }

    public class Chime : Item { }
    public class Comment : Item { }
}
