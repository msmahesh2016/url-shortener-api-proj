using System.ComponentModel.DataAnnotations;

namespace url_shortener_api.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }

        [Required]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ShortCode { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = false;

        public int ClickCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
