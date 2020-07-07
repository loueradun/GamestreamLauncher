GamestreamLauncher is a simpler launcher application to help assist with operations to be completed before and after your gamestream session.  

Release notes 1.3.0:  
Fix bug causing launcher to not restore state when session is terminated from the gamestream menu.  Issue #3
Add ability to set a an image or looping video as the wallpaper.
Add ability to set a custom desktop resolution when the stream starts. (You can not set a custom resolution not listed in the display settings.  There are other utilites that can help you create custom resolutions.)

Operations on Session start:  
Disable GPU Miner through Awesome Miner if running  
Disable non-primary monitors if present  
Set a custom resolution
Run a custom script  

Operations on Session end:  
Run a custom script  
restore previous desktop resolution
Enable non-primary monitors that were previously disabled  
Enable the GPU Miner through Awesome Miner if it was previously disabled  
Close the Gamestream session  

Installation:  
Copy the release folder to your desired location  
Run GamestreamLauncher.exe to configure the launcher  
Add a custom game in Geforce Experience that points to the launcher  

The launcher is configured with the config utility when launching it for the first time.  
You can run the config utility again by deleting the user.config file that can be found at %localappdata%\GamestreamLauncher under the proper version.