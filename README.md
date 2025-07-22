# SpeakerControlService

This is a Windows service that will toggle a switch on Home Assistant based on audio activity on your Windows PC. I have made this service since I have some nice speakers that create a humming sound when no audio is playing.

To solve this issue I have built in a relay with an ESP8266 hooked up to it running ESPHome and configured it as a switch in Home Assistant. Using the HA Rest API and the Windows Audio API the switch is toggled on whenever audio starts playing, it toggles off if no audio has been playing for a set amount of time.

# Installation

To install this service, download the latest release and put it in a safe spot where you don't accedentally delete it, like C:\Services\SpeakerControl or something. Then open the config.json file and fill in your HomeAssistant details and maybe tweak the timing settings to your liking.

After this, you can choose to run the file manually and test it out by double clicking the .exe or you can install it as a service in Windows which makes it run automatically in the background always. To do this open the folder in either PowerShell or CMD by running the cd command.

Then depending on the terminal of choosing run the install command:

PowerShell: `./SpeakerControlService.exe --install`  
CMD: `SpeakerControlService.exe --install`

That's it! The service is now installed and running.

# Uninstalling

Uninstalling the service is just as easy as installing it. Once again navigate to the installation folder of your choosing by using the cd command and then depending on the terminal of choosing run the uninstall command:

PowerShell: `./SpeakerControlService.exe --uninstall`  
CMD: `SpeakerControlService.exe --uninstall`