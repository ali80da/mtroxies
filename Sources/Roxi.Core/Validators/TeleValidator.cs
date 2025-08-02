using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Roxi.Core.Services.V01.Robot;

namespace Roxi.Core.Validators
{
    public class TeleValidator : AbstractValidator<TeleRobotService.RegisterProxyRequest>
    {

        public TeleValidator()
        {
            RuleFor(x => x.Port)
                .InclusiveBetween(10000, 65535).WithMessage("Port must be between 10000 and 65535.");

            RuleFor(x => x.Secret)
                .NotEmpty().WithMessage("Secret is required.")
                .Matches(@"^[0-9a-f]{32}$").WithMessage("Secret must be a 32-character hexadecimal string.");

            RuleFor(x => x.SponsorChannel)
                .NotEmpty().WithMessage("Sponsor channel is required.")
                .Matches(@"^@[A-Za-z0-9_]{5,}$").WithMessage("SponsorChannel must be a valid Telegram channel (e.g., @Channel).");
        }

    }
}
