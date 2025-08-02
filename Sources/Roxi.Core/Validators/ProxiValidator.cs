using FluentValidation;
using Roxi.Core.Models.V01.Proxies;

namespace Roxi.Core.Validators
{
    public class ProxiValidator : AbstractValidator<Proxi>
    {
        public ProxiValidator()
        {
            RuleFor(x => x.Port)
                .InclusiveBetween(10000, 65535).WithMessage("Port must be between 10000 and 65535.");

            RuleFor(x => x.Secret)
                .NotEmpty().WithMessage("Secret is required.")
                .Matches(@"^[0-9a-f]{32}$").WithMessage("Secret must be a 32-character hexadecimal string.");

            RuleFor(x => x.SponsorChannel)
                .NotEmpty().WithMessage("Sponsor channel is required.")
                .Matches(@"^@[A-Za-z0-9_]{5,}$").WithMessage("SponsorChannel must be a valid Telegram channel (e.g., @Channel).");

            RuleFor(x => x.FakeDomain)
                .Matches(@"^([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$")
                .When(x => !string.IsNullOrEmpty(x.FakeDomain))
                .WithMessage("FakeDomain must be a valid domain (e.g., domain.com).");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.");
        }
    }
}
