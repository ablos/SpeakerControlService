# SpeakerControlService

This is a Windows service that will toggle a switch in Home Assistant based on audio activity on your Windows PC. I have made this service since I have some nice speakers that create a humming sound when no audio is playing.

To solve this issue I have built in a relay with an ESP8266 hooked up to it running ESPHome and configured it as a switch in Home Assistant. Using the HA Rest API and the Windows Audio API the switch is toggled on whenever audio starts playing, it toggles off if no audio has been playing for a set amount of time.

# Installation

To install this service, download the latest release and put it in a safe spot where you don't accedentally delete it, like C:\Services\SpeakerControl or something. Then open the config.json file and fill in your HomeAssistant details and maybe tweak the timing settings to your liking (details below).

After this, you can choose to run the file manually and test it out by double clicking the .exe or you can install it as a service in Windows which makes it run automatically in the background always. To do this open the folder in either PowerShell or CMD by running the cd command.

Then depending on the terminal of choosing run the install command:

PowerShell: `./SpeakerControlService.exe --install`  
CMD: `SpeakerControlService.exe --install`

That's it! The service is now installed and running.

# Uninstalling

Uninstalling the service is just as easy as installing it. Once again navigate to the installation folder of your choosing by using the cd command and then depending on the terminal of choosing run the uninstall command:

PowerShell: `./SpeakerControlService.exe --uninstall`  
CMD: `SpeakerControlService.exe --uninstall`

# Configuration

You are able to configure your Home Assistant Rest API settings, which are mandatory for this service to work, and tweak the audio monitoring settings. After updating the config.json file it is necessary to uninstall and reinstall the service again.

## HomeAssistant Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| BaseUrl | `http://your_local_ha_ip:8123` | The base URL for your Home Assistant instance. Replace `your_local_ha_ip` with your actual Home Assistant server's IP address. Port 8123 is the default Home Assistant web interface port. |
| AccessToken | `your_long_lived_access_token` | A long-lived access token for authenticating with the Home Assistant API. This should be generated from your Home Assistant user profile settings. |
| SpeakerEntityId | `switch.speaker_relay` | The entity ID of the speaker relay switch in Home Assistant. This controls a physical relay that manages speaker power or audio routing. |

## Audio Monitoring Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| CheckIntervalMs | `1000` | How often to check audio levels, in milliseconds. A value of 1000ms means the system checks for audio every second. |
| AudioThreshold | `0.001` | The minimum audio level threshold to detect sound. Values below this threshold are considered silence. This is a very sensitive setting (0.1% of maximum volume). |
| SilenceDelaySeconds | `15` | How long to wait (in seconds) after detecting silence before turning off the speakers. This prevents false triggers from brief pauses in audio. |