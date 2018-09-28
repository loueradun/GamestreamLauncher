GamestreamLauncher is a simpler launcher application to help assist with operations to be completed before and after your gamestream session.

Operations on Session start:
Disable GPU Miner through Awesome Miner if running
Disable non-primary monitors if present
Run a custom script

Operations on Session end:
Run a custom script
Enable non-primary monitors that were previously disabled
Enable the GPU Miner through Awesome Miner if it was previously disabled
Close the Gamestream session

The launcher is configured in the GamestreamLauncher.exe.config xml file.  Do not remove any keys from the file, if a key is unused just leave it empty.


config.file parameters:

AppName - The name of the application or game to launch, will show in the launcher splashscreen as "<appName> Launcher"
AppPath - The path of the application or game to launch.  This will be a full qualified path like "C:\Games\Game.exe"

AdminAccount - The user name of an account with admin access (admin access account is used for terminating the Gamestream session and running elevated scripts)
AdminPassword - The password of the admin account

MultiMonSwitchEnabled - Allow the launcher to enable/disable monitors

AwesomeMinerSwitchEnabled - Allow the launcher to enable/disable miners through Awesome Miner
AwesomeMinerUrl - The url of the web api for Awesome Miner (usually 127.0.0.1).
AwesomeMinerPort - The port of the web api for Awesome Miner
AwesomeMinerKey - The api key for Awesome Miner

ScriptsEnabled - Enable running of the startup/shutdown scripts
StartupScriptPath - full qualified path of the startup script to executed
StartupScriptParameters - any additional parameters to run on the script
StartupScriptElevated - whether we should run the script as an admin
ShutdownScriptPath - full qualified path of the shutdown script
ShutdownScriptParameters - any additional parameters to run on the script
ShutdownScriptElevated - whether we should run the script as an admin