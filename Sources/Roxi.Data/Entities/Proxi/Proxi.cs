using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Roxi.Data.Entities.Common;

namespace Roxi.Data.Entities.Proxi;

public record Proxi : ShardEntities
{
    

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public required int Port { get; set; }

    [RegularExpression(@"^[0-9a-f]{32}$", ErrorMessage = "Secret must be a 32-character hexadecimal string.")]
    public required string Secret { get; set; }
    public required string Send2Channel { get; set; } = string.Empty;

    [RegularExpression(@"^@[A-Za-z0-9_]{5,}$", ErrorMessage = "SponsorChannel must be a valid Telegram channel (e.g., @Channel).")]
    public required string SponsorChannel { get; set; }

    public List<string> Tags { get; set; } = new List<string>();

    [RegularExpression(@"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "FakeDomain must be a valid domain (e.g., domain.com).")]
    public string? FakeDomain { get; set; } = string.Empty;


    public string? Region { get; set; }
    public string? EncryptedSecret { get; set; } = string.Empty;
}
