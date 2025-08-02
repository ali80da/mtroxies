using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Roxi.Core.Models.V01.Proxies;

public record Proxi
{

    [JsonIgnore]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [JsonIgnore]
    public string PublicId { get; init; } = Guid.NewGuid().ToString();

    [Required]
    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; init; }

    [RegularExpression(@"^[0-9a-f]{32}$", ErrorMessage = "Secret must be a 32-character hexadecimal string.")]
    public string Secret { get; init; } = string.Empty;

    public required string Send2Channel { get; set; } = string.Empty;

    [RegularExpression(@"^@[A-Za-z0-9_]{5,}$", ErrorMessage = "SponsorChannel must be a valid Telegram channel (e.g., @Channel).")]
    public string SponsorChannel { get; init; } = string.Empty;

    [RegularExpression(@"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$", ErrorMessage = "FakeDomain must be a valid domain (e.g., domain.com).")]
    public string? FakeDomain { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; init; }


    public string? Region { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;

    [JsonIgnore]
    public string? EncryptedSecret { get; init; } = string.Empty;
    public List<string> Tags { get; init; } = new List<string>();

}