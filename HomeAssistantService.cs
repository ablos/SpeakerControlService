using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace SpeakerControlService
{
    public class HomeAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HomeAssistantService> _logger;
        private readonly HomeAssistantConfig _config;

        public HomeAssistantService(HttpClient httpClient, ILogger<HomeAssistantService> logger, IOptions<Config> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = options.Value.HomeAssistant;

            // Set up HTTP client headers
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.AccessToken}");
        }

        public async Task TurnOnSpeakers()
        {
            await CallService("switch", "turn_on", _config.SpeakerEntityId);
        }

        public async Task TurnOffSpeakers()
        {
            await CallService("switch", "turn_off", _config.SpeakerEntityId);
        }

        private async Task CallService(string domain, string service, string entityId)
        {
            try
            {
                string url = $"{_config.BaseUrl}/api/services/{domain}/{service}";
                var payload = new { entity_id = entityId };
                string json = JsonSerializer.Serialize(payload);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                    _logger.LogInformation("Successfully called {Domain}.{Service} for {EntityId}", domain, service, entityId);

                else
                    _logger.LogError("Failed ot call HA service. Status: {Status}, Response: {Response}", response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error calling HA service {Domain}.{Service}", domain, service);
            }
        }
    }
}
