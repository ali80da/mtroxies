using System.ComponentModel.DataAnnotations;

namespace Roxi.Data.Entities.Common
{
    public record ShardEntities
    {
        [Key]
        public required string Id { get; set; } = Guid.NewGuid().ToString();

        public required string PublicId { get; set; } = Guid.NewGuid().ToString();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public bool IsActive { get; set; }

    }
}
