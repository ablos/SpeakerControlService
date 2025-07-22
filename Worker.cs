using NAudio.CoreAudioApi;
using NAudio;
using NAudio.CoreAudioApi.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace SpeakerControlService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HomeAssistantService _haService;
        private readonly AudioMonitoringConfig _config;
        private MMDeviceEnumerator? _deviceEnumerator;
        private bool previousState = false;

        public Worker(ILogger<Worker> logger, HomeAssistantService haService, IOptions<Config> options)
        {
            _logger = logger;
            _haService = haService;
            _config = options.Value.AudioMonitoring;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Speaker Control Service starting...");

            try
            {
                // Initialize the audio device enumerator
                _deviceEnumerator = new MMDeviceEnumerator();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred while initializing the audio device.");
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting audio monitoring loop...");

            while (!stoppingToken.IsCancellationRequested)
            {
                bool currentState = CheckIfAnyPlaying();

                if (currentState && !previousState)
                {
                    _logger.LogInformation("Audio detected, turning on speakers...");
                    await _haService.TurnOnSpeakers();
                }

                else if (previousState && !currentState)
                {
                    _logger.LogInformation("Audio stopped playing, waiting {Delay} seconds...", _config.SilenceDelaySeconds);

                    // Wait set amound of seconds, if there is still no audio, turn off the speakers
                    bool startedPlayingAgain = false;

                    for (int i = 0; i < _config.SilenceDelaySeconds; i++)
                    {
                        await Task.Delay(1000, stoppingToken);

                        currentState = CheckIfAnyPlaying();

                        if (currentState)
                        {
                            startedPlayingAgain = true;
                            _logger.LogInformation("Audio started playing again, cancelling timer...");
                            break;
                        }
                    }

                    if (!startedPlayingAgain)
                    {
                        _logger.LogInformation("No audio detected for {Delay} seconds, turning off speakers...", _config.SilenceDelaySeconds);
                        await _haService.TurnOffSpeakers();
                    }
                }

                previousState = currentState;
                await Task.Delay(_config.CheckIntervalMs, stoppingToken);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Speaker Control Service stopping...");

            // Clean up audio resources
            _deviceEnumerator?.Dispose();

            return base.StopAsync(cancellationToken);
        }

        private bool CheckIfAnyPlaying()
        {
            bool foundPlayingDevice = false;

            try
            {
                if (_deviceEnumerator != null)
                {
                    // Get all audio output devices
                    MMDeviceCollection devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

                    foreach (MMDevice device in devices)
                    {
                        if (IsPlaying(device))
                        {
                            foundPlayingDevice = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error monitoring audio sessions.");
            }

            return foundPlayingDevice;
        }

        private bool IsPlaying(MMDevice device)
        {
            try
            {
                // Get the session manager for this device
                AudioSessionManager sessionManager = device.AudioSessionManager;
                SessionCollection sessions = sessionManager.Sessions;

                // Check all active audio sessions
                for (int i = 0; i < sessions.Count; i++)
                {
                    AudioSessionControl session = sessions[i];
                    AudioSessionState sessionState = session.State;

                    // If any session is active and playing audio
                    if (sessionState == AudioSessionState.AudioSessionStateActive)
                    {
                        AudioMeterInformation audioMeter = session.AudioMeterInformation;
                        float peakValue = audioMeter.MasterPeakValue;

                        // If peak value surpasses threshold, it is playing
                        if (peakValue > _config.AudioThreshold)
                            return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error checking device {DeviceName}", device.FriendlyName);
            }

            return false;
        }
    }
}
