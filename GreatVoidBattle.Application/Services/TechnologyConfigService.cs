using System.Text.Json;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreatVoidBattle.Application.Services;

public class TechnologyConfigService
{
    private readonly List<Technology> _technologies;
    private readonly ILogger<TechnologyConfigService>? _logger;

    public TechnologyConfigService(IConfiguration configuration, ILogger<TechnologyConfigService>? logger = null)
    {
        _logger = logger;
        
        try
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "technologies.json");
            _logger?.LogInformation($"Loading technologies from: {jsonPath}");
            
            if (!File.Exists(jsonPath))
            {
                _logger?.LogError($"Technologies file not found: {jsonPath}");
                _technologies = new List<Technology>();
                return;
            }
            
            var jsonContent = File.ReadAllText(jsonPath);
            _logger?.LogInformation($"Loaded JSON content length: {jsonContent.Length}");
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var config = JsonSerializer.Deserialize<TechnologyConfig>(jsonContent, options);
            _technologies = config?.Technologies ?? new List<Technology>();
            
            _logger?.LogInformation($"Loaded {_technologies.Count} technologies");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading technologies");
            _technologies = new List<Technology>();
        }
    }

    public List<Technology> GetAllTechnologies()
    {
        return _technologies;
    }

    public List<Technology> GetTechnologiesByTier(int tier)
    {
        return _technologies.Where(t => t.Tier == tier).ToList();
    }

    public Technology? GetTechnologyById(string id)
    {
        return _technologies.FirstOrDefault(t => t.Id == id);
    }

    public int GetMaxTier()
    {
        return _technologies.Max(t => t.Tier);
    }

    private class TechnologyConfig
    {
        public List<Technology> Technologies { get; set; } = new();
    }
}
