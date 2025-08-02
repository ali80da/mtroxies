using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using Roxi.Core.Models.V01.Common;
using Roxi.Core.Models.V01.Proxies;
using Roxi.Data.Context;
using Microsoft.EntityFrameworkCore;
using Roxi.Core.Services.V01.Robot;
using System.Threading.Channels;

namespace Roxi.Core.Services.V01.Proxie
{
    /// <summary>
    /// Defines the contract for managing MTProto Proxies.
    /// </summary>
    public interface IProxiService
    {

        /// <summary>
        /// Creates a new MTProto proxy with the specified request data.
        /// </summary>
        /// <param name="request">The request containing sponsor channel, fake domain, and tags.</param>
        /// <returns>A ResultConditions containing the created proxy details or an error.</returns>
        Task<ResultConditions<Proxi>> CreateProxiAsync(CreateAndUpdateProxiRequest request);

        /// <summary>
        /// Updates an existing proxy with the specified port and request data.
        /// </summary>
        /// <param name="port">The port of the proxy to update.</param>
        /// <param name="request">The request containing new sponsor channel, fake domain, and tags.</param>
        /// <returns>A ResultConditions containing the updated proxy details or an error.</returns>
        Task<ResultConditions<Proxi>> UpdateProxiAsync(int port, CreateAndUpdateProxiRequest request);

        /// <summary>
        /// Deletes a proxy with the specified port.
        /// </summary>
        /// <param name="port">The port of the proxy to delete.</param>
        /// <returns>A ResultConditions indicating the deletion result.</returns>
        Task<ResultConditions<bool>> DeleteProxiAsync(int port);

        /// <summary>
        /// Retrieves the list of all active proxies.
        /// </summary>
        /// <returns>A ResultConditions containing the list of proxies.</returns>
        Task<ResultConditions<List<Proxi>>> GetProxiesAsync();

    }


    /// <summary>
    /// Manages MTProto proxies, including creation, update, deletion, and retrieval.
    /// </summary>
    public class ProxiService : IProxiService
    {
        private readonly RoxiDatabaseContext _dbContext;
        private readonly ITeleRobotService _telegramBotService;
        private readonly ILogger<ProxiService> _logger;
        private readonly IValidator<CreateAndUpdateProxiRequest> _createValidator;
        private readonly IValidator<Proxi> _proxiValidator;

        public ProxiService(
            RoxiDatabaseContext dbContext,
            ITeleRobotService telegramBotService,
            ILogger<ProxiService> logger,
            IValidator<CreateAndUpdateProxiRequest> createValidator,
            IValidator<Proxi> proxiValidator)
        {
            _dbContext = dbContext;
            _telegramBotService = telegramBotService;
            _logger = logger;
            _createValidator = createValidator;
            _proxiValidator = proxiValidator;
        }

        #region Proxy Management (v1)

        /// <summary>
        /// Creates a new MTProto proxy with the specified request data.
        /// </summary>
        public async Task<ResultConditions<Proxi>> CreateProxiAsync(CreateAndUpdateProxiRequest request)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Creating proxy: RequestId={RequestId}, SponsorChannel={SponsorChannel}, FakeDomain={FakeDomain}", requestId, request.SponsorChannel, request.FakeDomain);

            try
            {
                // Validate request
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ErrorDetail(
                        "VALIDATION_ERROR",
                        e.ErrorMessage,
                        "Correct the input data.",
                        e.PropertyName
                    )).ToList();
                    _logger.LogWarning("Request validation failed: RequestId={RequestId}, Errors={Errors}", requestId, string.Join("; ", errors.Select(e => e.Message)));
                    return ResultConditions<Proxi>.Error(
                        "Request validation failed.",
                        "INVALID_REQUEST",
                        "Validation errors occurred.",
                        errors.Select(e => e.Resolution).ToList(),
                        null,
                        null,
                        HttpStatusCode.BadRequest
                    );
                }

                // Check for duplicate proxy
                if (await _dbContext.Proxies.AnyAsync(p => p.SponsorChannel == request.SponsorChannel && p.FakeDomain == request.FakeDomain && p.IsActive))
                {
                    _logger.LogWarning("Duplicate proxy detected: RequestId={RequestId}, SponsorChannel={SponsorChannel}, FakeDomain={FakeDomain}", requestId, request.SponsorChannel, request.FakeDomain);
                    return ResultConditions<Proxi>.Error(
                        "A proxy with the same sponsor channel and fake domain already exists.",
                        "DUPLICATE_PROXY",
                        "Use a different sponsor channel or fake domain.",
                        new List<string> { "Provide unique sponsor channel or fake domain." },
                        null,
                        "sponsorChannel, fakeDomain",
                        HttpStatusCode.Conflict
                    );
                }

                // Create entity
                var port = await GetAvailablePortAsync();
                var secret = GenerateRandomSecret();
                var entity = new Roxi.Data.Entities.Proxi.Proxi
                {
                    Id = Guid.NewGuid().ToString(),
                    PublicId = Guid.NewGuid().ToString(),
                    Port = port,
                    Secret = secret,
                    EncryptedSecret = secret, // Simplified: no encryption
                    Send2Channel = request.Send2Channel,
                    SponsorChannel = request.SponsorChannel,
                    FakeDomain = request.FakeDomain,
                    Tags = request.Tags.Any() ? request.Tags : new List<string> { "mtroto" },
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Validate entity
                var entityValidationResult = await _proxiValidator.ValidateAsync(MapToDto(entity));
                if (!entityValidationResult.IsValid)
                {
                    var errors = entityValidationResult.Errors.Select(e => new ErrorDetail(
                        "VALIDATION_ERROR",
                        e.ErrorMessage,
                        "Correct the input data.",
                        e.PropertyName
                    )).ToList();
                    _logger.LogWarning("Entity validation failed: RequestId={RequestId}, Errors={Errors}", requestId, string.Join("; ", errors.Select(e => e.Message)));
                    return ResultConditions<Proxi>.Error(
                        "Proxy entity validation failed.",
                        "INVALID_ENTITY",
                        "Validation errors occurred.",
                        errors.Select(e => e.Resolution).ToList(),
                        null,
                        null,
                        HttpStatusCode.BadRequest
                    );
                }

                // Save to database
                _dbContext.Proxies.Add(entity);
                await _dbContext.SaveChangesAsync();

                
                // Update MTProto and NGINX configurations
                //await UpdateMtProtoConfigAsync();
                //await AppendNginxConfigAsync(port);

                // Register with Telegram
                var registrationResult = await _telegramBotService.RegisterProxyAsync(entity.Send2Channel, port, secret, request.SponsorChannel);
                if (registrationResult.Status != ResultStatus.Success)
                {
                    _logger.LogWarning("Proxy registration failed: RequestId={RequestId}, Message={Message}", requestId, registrationResult.Message);
                    var proxiDto = MapToDto(entity);
                    return ResultConditions<Proxi>.Warning(
                        proxiDto,
                        $"Proxy created but failed to register with Telegram: {registrationResult.Message}",
                        "PROXY_CREATED_BUT_REGISTRATION_FAILED",
                        "Proxy created successfully but Telegram registration failed.",
                        new List<string> { "Check Telegram bot configuration or try again later." },
                        registrationResult.Errors.FirstOrDefault()?.Resolution,
                        null,
                        HttpStatusCode.OK
                    );
                }

                // Reload services
                //await ReloadServicesAsync();
                

                var resultDto = MapToDto(entity);
                _logger.LogInformation("Proxy created: RequestId={RequestId}, Port={Port}, SponsorChannel={SponsorChannel}", requestId, resultDto.Port, resultDto.SponsorChannel);
                return ResultConditions<Proxi>.Success(
                    resultDto,
                    "Proxy created successfully.",
                    "PROXY_CREATED",
                    "Proxy created and saved to database.",
                    new List<string> { "Awaiting Telegram registration and service configuration." },
                    new ResultMetadata { Custom = new Dictionary<string, object> { { "proxyCount", await _dbContext.Proxies.CountAsync(p => p.IsActive) } } }
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "No available ports: RequestId={RequestId}", requestId);
                return ResultConditions<Proxi>.Error(
                    "No available ports found.",
                    "NO_AVAILABLE_PORTS",
                    "No ports available in the range 10000-65535.",
                    new List<string> { "Ensure there are available ports or increase the range." },
                    null,
                    null,
                    HttpStatusCode.ServiceUnavailable
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create proxy: RequestId={RequestId}", requestId);
                return ResultConditions<Proxi>.Error(
                    $"Failed to create proxy: {ex.Message}",
                    "PROXY_CREATION_FAILED",
                    "An unexpected error occurred.",
                    new List<string> { "Check server logs or contact support." },
                    null,
                    null,
                    HttpStatusCode.InternalServerError
                );
            }
        }

        /// <summary>
        /// Updates an existing proxy with the specified port and request data.
        /// </summary>
        public async Task<ResultConditions<Proxi>> UpdateProxiAsync(int port, CreateAndUpdateProxiRequest request)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Updating proxy: RequestId={RequestId}, Port={Port}, SponsorChannel={SponsorChannel}, FakeDomain={FakeDomain}", requestId, port, request.SponsorChannel, request.FakeDomain);

            try
            {
                // Validate request
                var validationResult = await _createValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ErrorDetail(
                        "VALIDATION_ERROR",
                        e.ErrorMessage,
                        "Correct the input data.",
                        e.PropertyName
                    )).ToList();
                    _logger.LogWarning("Request validation failed: RequestId={RequestId}, Errors={Errors}", requestId, string.Join("; ", errors.Select(e => e.Message)));
                    return ResultConditions<Proxi>.Error(
                        "Request validation failed.",
                        "INVALID_REQUEST",
                        "Validation errors occurred.",
                        errors.Select(e => e.Resolution).ToList(),
                        null,
                        null,
                        HttpStatusCode.BadRequest
                    );
                }

                // Find proxy
                var entity = await _dbContext.Proxies.FirstOrDefaultAsync(p => p.Port == port && p.IsActive);
                if (entity == null)
                {
                    _logger.LogWarning("Proxy not found: RequestId={RequestId}, Port={Port}", requestId, port);
                    return ResultConditions<Proxi>.Error(
                        $"Proxy with port {port} not found or is inactive.",
                        "PROXY_NOT_FOUND",
                        "Proxy does not exist or is inactive.",
                        new List<string> { "Ensure the proxy port exists and is active." },
                        null,
                        "port",
                        HttpStatusCode.NotFound
                    );
                }

                // Check for duplicate proxy
                if (await _dbContext.Proxies.AnyAsync(p => p.SponsorChannel == request.SponsorChannel && p.FakeDomain == request.FakeDomain && p.IsActive && p.Id != entity.Id))
                {
                    _logger.LogWarning("Duplicate proxy detected: RequestId={RequestId}, SponsorChannel={SponsorChannel}, FakeDomain={FakeDomain}", requestId, request.SponsorChannel, request.FakeDomain);
                    return ResultConditions<Proxi>.Error(
                        "A proxy with the same sponsor channel and fake domain already exists.",
                        "DUPLICATE_PROXY",
                        "Use a different sponsor channel or fake domain.",
                        new List<string> { "Provide unique sponsor channel or fake domain." },
                        null,
                        "sponsorChannel, fakeDomain",
                        HttpStatusCode.Conflict
                    );
                }

                // Update entity
                entity.SponsorChannel = request.SponsorChannel;
                entity.FakeDomain = request.FakeDomain;
                entity.Tags = request.Tags.Any() ? request.Tags : entity.Tags.Concat(new[] { "updated" }).Distinct().ToList();
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Send2Channel = request.Send2Channel;

                // Validate entity
                var entityValidationResult = await _proxiValidator.ValidateAsync(MapToDto(entity));
                if (!entityValidationResult.IsValid)
                {
                    var errors = entityValidationResult.Errors.Select(e => new ErrorDetail(
                        "VALIDATION_ERROR",
                        e.ErrorMessage,
                        "Correct the input data.",
                        e.PropertyName
                    )).ToList();
                    _logger.LogWarning("Entity validation failed: RequestId={RequestId}, Errors={Errors}", requestId, string.Join("; ", errors.Select(e => e.Message)));
                    return ResultConditions<Proxi>.Error(
                        "Proxy entity validation failed.",
                        "INVALID_ENTITY",
                        "Validation errors occurred.",
                        errors.Select(e => e.Resolution).ToList(),
                        null,
                        null,
                        HttpStatusCode.BadRequest
                    );
                }

                // Save changes
                await _dbContext.SaveChangesAsync();

                // Update MTProto and NGINX configurations
                //await UpdateMtProtoConfigAsync();

                // Register with Telegram
                var registrationResult = await _telegramBotService.RegisterProxyAsync(entity.Send2Channel, port, entity.Secret, request.SponsorChannel);
                if (registrationResult.Status != ResultStatus.Success)
                {
                    _logger.LogWarning("Proxy registration failed: RequestId={RequestId}, Message={Message}", requestId, registrationResult.Message);
                    var proxiDto = MapToDto(entity);
                    return ResultConditions<Proxi>.Warning(
                        proxiDto,
                        $"Proxy updated but failed to register with Telegram: {registrationResult.Message}",
                        "PROXY_UPDATED_BUT_REGISTRATION_FAILED",
                        "Proxy updated successfully but Telegram registration failed.",
                        new List<string> { "Check Telegram bot configuration or try again later." },
                        registrationResult.Errors.FirstOrDefault()?.Resolution,
                        null,
                        HttpStatusCode.OK
                    );
                }

                // Reload services
                //await ReloadServicesAsync();
                

                var resultDto = MapToDto(entity);
                _logger.LogInformation("Proxy updated: RequestId={RequestId}, Port={Port}, SponsorChannel={SponsorChannel}", requestId, resultDto.Port, resultDto.SponsorChannel);
                return ResultConditions<Proxi>.Success(
                    resultDto,
                    "Proxy updated successfully.",
                    "PROXY_UPDATED",
                    "Proxy updated and saved to database.",
                    new List<string> { "Awaiting Telegram registration and service configuration." },
                    new ResultMetadata { Custom = new Dictionary<string, object> { { "proxyCount", await _dbContext.Proxies.CountAsync(p => p.IsActive) } } }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update proxy: RequestId={RequestId}, Port={Port}", requestId, port);
                return ResultConditions<Proxi>.Error(
                    $"Failed to update proxy: {ex.Message}",
                    "PROXY_UPDATE_FAILED",
                    "An unexpected error occurred.",
                    new List<string> { "Check server logs or contact support." },
                    null,
                    null,
                    HttpStatusCode.InternalServerError
                );
            }
        }

        /// <summary>
        /// Deletes a proxy with the specified port.
        /// </summary>
        public async Task<ResultConditions<bool>> DeleteProxiAsync(int port)
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Deleting proxy: RequestId={RequestId}, Port={Port}", requestId, port);

            try
            {
                var entity = await _dbContext.Proxies.FirstOrDefaultAsync(p => p.Port == port && p.IsActive);
                if (entity == null)
                {
                    _logger.LogWarning("Proxy not found: RequestId={RequestId}, Port={Port}", requestId, port);
                    return ResultConditions<bool>.Error(
                        $"Proxy with port {port} not found or is inactive.",
                        "PROXY_NOT_FOUND",
                        "Proxy does not exist or is inactive.",
                        new List<string> { "Ensure the proxy port exists and is active." },
                        null,
                        "port",
                        HttpStatusCode.NotFound
                    );
                }

                // Soft delete
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                /*
                // Update MTProto and NGINX configurations
                await UpdateMtProtoConfigAsync();
                await RemoveNginxConfigAsync(port);

                // Reload services
                await ReloadServicesAsync();
                */

                _logger.LogInformation("Proxy deleted: RequestId={RequestId}, Port={Port}", requestId, port);
                return ResultConditions<bool>.Success(
                    true,
                    "Proxy deleted successfully.",
                    "PROXY_DELETED",
                    "Proxy was deactivated successfully.",
                    new List<string> { "Awaiting service configuration updates." },
                    new ResultMetadata { Custom = new Dictionary<string, object> { { "proxyCount", await _dbContext.Proxies.CountAsync(p => p.IsActive) } } }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete proxy: RequestId={RequestId}, Port={Port}", requestId, port);
                return ResultConditions<bool>.Error(
                    $"Failed to delete proxy: {ex.Message}",
                    "PROXY_DELETION_FAILED",
                    "An unexpected error occurred.",
                    new List<string> { "Check server logs or contact support." },
                    null,
                    null,
                    HttpStatusCode.InternalServerError
                );
            }
        }

        /// <summary>
        /// Retrieves the list of all active proxies.
        /// </summary>
        public async Task<ResultConditions<List<Proxi>>> GetProxiesAsync()
        {
            var requestId = Guid.NewGuid().ToString();
            _logger.LogInformation("Retrieving proxies: RequestId={RequestId}", requestId);

            try
            {
                var entities = await _dbContext.Proxies
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                var proxies = entities.Select(MapToDto).ToList();

                var metadata = new ResultMetadata
                {
                    Pagination = new Dictionary<string, object>
                    {
                        { "totalItems", proxies.Count },
                        { "pageNumber", 1 },
                        { "pageSize", proxies.Count }
                    },
                    Custom = new Dictionary<string, object>
                    {
                        { "activeProxies", proxies.Count }
                    }
                };

                _logger.LogInformation("Proxies retrieved: RequestId={RequestId}, Count={Count}", requestId, proxies.Count);
                return ResultConditions<List<Proxi>>.Success(
                    proxies,
                    "Proxies retrieved successfully.",
                    "PROXIES_RETRIEVED",
                    "Active proxies retrieved from the database.",
                    new List<string> { "No further actions required." },
                    metadata
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve proxies: RequestId={RequestId}", requestId);
                return ResultConditions<List<Proxi>>.Error(
                    $"Failed to retrieve proxies: {ex.Message}",
                    "PROXIES_RETRIEVAL_FAILED",
                    "An unexpected error occurred.",
                    new List<string> { "Check server logs or contact support." },
                    null,
                    null,
                    HttpStatusCode.InternalServerError
                );
            }
        }

        #endregion

        #region Private Helper Methods (v1)

        /// <summary>
        /// Maps a Proxi entity to a Proxi DTO.
        /// </summary>
        private Proxi MapToDto(Roxi.Data.Entities.Proxi.Proxi entity)
        {
            return new Proxi
            {
                Id = entity.Id,
                Port = entity.Port,
                Secret = entity.Secret,
                Send2Channel = entity.Send2Channel,
                SponsorChannel = entity.SponsorChannel,
                FakeDomain = entity.FakeDomain,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Region = entity.Region,
                IsActive = entity.IsActive,
                EncryptedSecret = entity.EncryptedSecret,
                Tags = entity.Tags
            };
        }

        /// <summary>
        /// Finds an available port for a new proxy.
        /// </summary>
        private async Task<int> GetAvailablePortAsync(int startPort = 10000, int endPort = 65535)
        {
            var usedPorts = await _dbContext.Proxies.Select(p => p.Port).ToListAsync();
            for (int port = startPort; port <= endPort; port++)
            {
                if (!usedPorts.Contains(port))
                {
                    return port;
                }
            }
            throw new InvalidOperationException("No available ports found.");
        }

        /// <summary>
        /// Generates a random secret for the proxy.
        /// </summary>
        private string GenerateRandomSecret()
        {
            var bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        /*
        /// <summary>
        /// Updates the MTProto proxy configuration file.
        /// </summary>
        private async Task UpdateMtProtoConfigAsync()
        {
            // To be implemented in IDockerService or INginxService
        }

        /// <summary>
        /// Appends NGINX configuration for a new proxy port.
        /// </summary>
        private async Task AppendNginxConfigAsync(int port)
        {
            // To be implemented in INginxService
        }

        /// <summary>
        /// Removes NGINX configuration for a specified proxy port.
        /// </summary>
        private async Task RemoveNginxConfigAsync(int port)
        {
            // To be implemented in INginxService
        }

        /// <summary>
        /// Reloads NGINX and MTProto proxy services.
        /// </summary>
        private async Task ReloadServicesAsync()
        {
            // To be implemented in IDockerService
        }
        */

        #endregion
    }




}