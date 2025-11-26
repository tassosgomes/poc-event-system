using System;
using System.ComponentModel.DataAnnotations;

namespace VideoEnricher.Domain
{
    public class VideoMetadata
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SongId { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public long Views { get; set; }
        public DateTime ScrapedAt { get; set; }
    }
}
