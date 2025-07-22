namespace SpeakerControlService
{
    public class Config
    {
        public HomeAssistantConfig HomeAssistant { get; set; } = new();
        public AudioMonitoringConfig AudioMonitoring { get; set; } = new();
    }

    public class HomeAssistantConfig
    {
        public string BaseUrl { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string SpeakerEntityId { get; set; } = "";
    }

    public class AudioMonitoringConfig
    {
        public int CheckIntervalMs { get; set; } = 1000;
        public float AudioThreshold { get; set; } = 0.001f;
        public int SilenceDelaySeconds { get; set; } = 15;
    }
}
