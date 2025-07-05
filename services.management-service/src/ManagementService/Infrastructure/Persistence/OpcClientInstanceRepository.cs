using System.Text.Json;
using System.Text.Json.Serialization;
using ManagementService.Application.Contracts.Persistence;
using ManagementService.Domain.Aggregates;
using ManagementService.Domain.ValueObjects;

namespace ManagementService.Infrastructure.Persistence;

/// <summary>
/// Implements the persistence contract for OpcClientInstance by communicating with a central Data Service over HTTP.
/// This class acts as an Anti-Corruption Layer, translating between the domain model and the external service's DTOs.
/// </summary>
public class OpcClientInstanceRepository : IOpcClientInstanceRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpcClientInstanceRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public OpcClientInstanceRepository(IHttpClientFactory httpClientFactory, ILogger<OpcClientInstanceRepository> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<OpcClientInstance?> GetByIdAsync(Guid id)
    {
        var httpClient = _httpClientFactory.CreateClient("DataServiceClient");
        try
        {
            var response = await httpClient.GetAsync($"api/opcclientdata/{id}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var dataServiceDto = await JsonSerializer.DeserializeAsync<OpcClientInstanceDataDto>(stream, _jsonOptions);
            return MapToDomain(dataServiceDto);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Data Service failed while getting client by ID {ClientId}", id);
            throw; // Re-throw to be handled by application layer
        }
    }

    public async Task<IReadOnlyList<OpcClientInstance>> GetAllAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("DataServiceClient");
        try
        {
            var dtos = await httpClient.GetFromJsonAsync<List<OpcClientInstanceDataDto>>("api/opcclientdata", _jsonOptions);
            return dtos?.Select(MapToDomain).ToList() ?? new List<OpcClientInstance>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Data Service failed while getting all clients");
            throw;
        }
    }

    public async Task AddAsync(OpcClientInstance clientInstance)
    {
        var httpClient = _httpClientFactory.CreateClient("DataServiceClient");
        var dto = MapToDataDto(clientInstance);
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/opcclientdata", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Data Service failed while adding client {ClientId}", clientInstance.Id);
            throw;
        }
    }

    public async Task UpdateAsync(OpcClientInstance clientInstance)
    {
        var httpClient = _httpClientFactory.CreateClient("DataServiceClient");
        var dto = MapToDataDto(clientInstance);
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/opcclientdata/{clientInstance.Id}", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to Data Service failed while updating client {ClientId}", clientInstance.Id);
            throw;
        }
    }

    #region Anti-Corruption Layer (Mapping)

    // Maps the external Data Service DTO to our Domain model.
    private static OpcClientInstance MapToDomain(OpcClientInstanceDataDto dto)
    {
        if (dto is null) return null;

        var instance = (OpcClientInstance)Activator.CreateInstance(typeof(OpcClientInstance), true)!;

        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.Id))!.SetValue(instance, dto.Id);
        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.Name))!.SetValue(instance, dto.Name);
        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.Site))!.SetValue(instance, dto.Site);
        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.LastSeen))!.SetValue(instance, dto.LastSeen);
        
        var config = new ClientConfiguration(dto.Configuration.PollingIntervalSeconds,
            dto.Configuration.TagConfigurations.Select(t => new TagConfig(t.TagName, t.ScanRate)).ToList());
        var health = new HealthStatus(dto.HealthStatus.IsConnected, dto.HealthStatus.DataThroughput, dto.HealthStatus.CpuUsagePercent);

        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.Configuration))!.SetValue(instance, config);
        typeof(OpcClientInstance).GetProperty(nameof(OpcClientInstance.HealthStatus))!.SetValue(instance, health);

        return instance;
    }

    // Maps our Domain model to the external Data Service DTO.
    private static OpcClientInstanceDataDto MapToDataDto(OpcClientInstance instance)
    {
        return new OpcClientInstanceDataDto
        {
            Id = instance.Id,
            Name = instance.Name,
            Site = instance.Site,
            LastSeen = instance.LastSeen,
            Configuration = new ClientConfigurationDataDto
            {
                PollingIntervalSeconds = instance.Configuration.PollingIntervalSeconds,
                TagConfigurations = instance.Configuration.TagConfigurations.Select(t => new TagConfigDataDto { TagName = t.TagName, ScanRate = t.ScanRate }).ToList()
            },
            HealthStatus = new HealthStatusDataDto
            {
                IsConnected = instance.HealthStatus.IsConnected,
                DataThroughput = instance.HealthStatus.DataThroughput,
                CpuUsagePercent = instance.HealthStatus.CpuUsagePercent
            }
        };
    }


    /// <summary>
    /// DTO for communicating with the external Data Service.
    /// This is an implementation detail of the repository.
    /// </summary>
    private class OpcClientInstanceDataDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public DateTimeOffset LastSeen { get; set; }
        public HealthStatusDataDto HealthStatus { get; set; } = new();
        public ClientConfigurationDataDto Configuration { get; set; } = new();
    }

    private class HealthStatusDataDto
    {
        public bool IsConnected { get; set; }
        public double DataThroughput { get; set; }
        public double CpuUsagePercent { get; set; }
    }

    private class ClientConfigurationDataDto
    {
        public int PollingIntervalSeconds { get; set; }
        public List<TagConfigDataDto> TagConfigurations { get; set; } = [];
    }
    
    private class TagConfigDataDto
    {
        public string TagName { get; set; } = string.Empty;
        public int ScanRate { get; set; }
    }

    #endregion
}