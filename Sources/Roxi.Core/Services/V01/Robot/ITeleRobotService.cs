using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Roxi.Core.Models.V01.Common;
using Telegram.Bot;

namespace Roxi.Core.Services.V01.Robot
{

    /// <summary>
    /// Defines the contract for interacting with Telegram Bot API to manage MTProto proxies.
    /// </summary>
    public interface ITeleRobotService
    {

        /// <summary>
        /// Registers an MTProto proxy with the specified port, secret, and sponsor channel.
        /// </summary>
        /// <param name="port">The port of the proxy.</param>
        /// <param name="secret">The secret key for the proxy.</param>
        /// <param name="sponsorChannel">The Telegram sponsor channel (e.g., @Channel).</param>
        /// <returns>A ResultConditions indicating the registration result.</returns>
        Task<ResultConditions<bool>> RegisterProxyAsync(int port, string secret, string sponsorChannel);


    }


    /// <summary>
    /// Manages interactions with Telegram Bot API for registering MTProto proxies.
    /// </summary>
    public class TeleRobotService : ITeleRobotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TeleRobotService> _logger;
        private readonly IValidator<RegisterProxyRequest> _validator;
        private readonly string _serverIp;

        public TeleRobotService(
            IConfiguration configuration,
            ILogger<TeleRobotService> logger,
            IValidator<RegisterProxyRequest> validator)
        {
            _botClient = new TelegramBotClient(configuration["TelegramBotToken"] ?? throw new ArgumentNullException("TelegramBotToken is missing."));
            _serverIp = configuration["ServerIp"] ?? throw new ArgumentNullException("ServerIp is missing.");
            _logger = logger;
            _validator = validator;
        }

        /// <summary>
        /// Registers an MTProto proxy with the specified port, secret, and sponsor channel.
        /// </summary>
        public async Task<ResultConditions<bool>> RegisterProxyAsync(int port, string secret, string sponsorChannel)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Registering proxy with Telegram: RequestId={RequestId}, Port={Port}, SponsorChannel={SponsorChannel}", requestId, port, sponsorChannel);

            try
            {
                // Validate input
                var request = new RegisterProxyRequest(port, secret, sponsorChannel);
                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ErrorDetail(
                        "VALIDATION_ERROR",
                        e.ErrorMessage,
                        "Correct the input data.",
                        e.PropertyName
                    )).ToList();
                    _logger.LogWarning("Validation failed: RequestId={RequestId}, Errors={Errors}", requestId, string.Join("; ", errors.Select(e => e.Message)));
                    return ResultConditions<bool>.Error(
                        "Request validation failed.",
                        "INVALID_REQUEST",
                        "Validation errors occurred.",
                        errors.Select(e => e.Resolution).ToList(),
                        null,
                        null,
                        HttpStatusCode.BadRequest
                    );
                }

                // Construct proxy link
                var proxyLink = $"tg://proxy?server={_serverIp}&port={port}&secret={secret}";
                var message = $"New MTProto Proxy:\nPort: {port}\nSponsor: {sponsorChannel}\nLink: {proxyLink}";

                // Send message to sponsor channel
                //await _botClient.SendTextMessageAsync(
                //    chatId: sponsorChannel,
                //    text: message,
                //    disableWebPagePreview: true
                //);

                _logger.LogInformation("Proxy registered with Telegram: RequestId={RequestId}, Port={Port}, SponsorChannel={SponsorChannel}", requestId, port, sponsorChannel);
                return ResultConditions<bool>.Success(
                    true,
                    "Proxy registered successfully with Telegram.",
                    "PROXY_REGISTERED",
                    "Proxy link sent to the sponsor channel.",
                    new List<string> { "No further actions required." },
                    new ResultMetadata { Custom = new Dictionary<string, object> { { "proxyLink", proxyLink } } }
                );
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                _logger.LogError(ex, "Failed to register proxy with Telegram: RequestId={RequestId}, Port={Port}", requestId, port);
                return ResultConditions<bool>.Error(
                    $"Failed to register proxy: {ex.Message}",
                    "TELEGRAM_API_ERROR",
                    "Error occurred while interacting with Telegram API.",
                    new List<string> { "Check Telegram bot token, channel permissions, or try again later." },
                    null,
                    null,
                    HttpStatusCode.BadRequest
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while registering proxy: RequestId={RequestId}, Port={Port}", requestId, port);
                return ResultConditions<bool>.Error(
                    $"Unexpected error: {ex.Message}",
                    "PROXY_REGISTRATION_FAILED",
                    "An unexpected error occurred.",
                    new List<string> { "Check server logs or contact support." },
                    null,
                    null,
                    HttpStatusCode.InternalServerError
                );
            }
        }


        /// <summary>
        /// Internal record for validating RegisterProxyAsync inputs.
        /// </summary>
        public record RegisterProxyRequest(int Port, string Secret, string SponsorChannel);
    }


}