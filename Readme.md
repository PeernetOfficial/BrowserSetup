# Peernet Browser Installer Project (Setup)

Peernet.Browser.Setup is [Microsoft Visual Studio Installer Project](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects).
Its purpose is to deliver self-sufficient installation process under single __.msi__ file.
Setup process is devided into __two stages__:
### Stage 1. Main installation{#stage1}
First part of the installation process is to extract all the Peernet.Browser.Setup project files into the installation location.
The full list of the files has been described [here](#insights).
Default installation location is under current user's AppData folder.

```
[LocalAppDataFolder][Manufacturer][ProductName]
```

This stage is being executed as part of
__[Install](https://docs.microsoft.com/en-us/dotnet/api/system.configuration.install.installer.install?redirectedfrom=MSDN&view=netframework-4.8#System_Configuration_Install_Installer_Install_System_Collections_IDictionary_)__ 
basic installation action.

### Stage 2. Firewall rule
The other stage is execution of __netsh__ process which creates _Peernet Browser_ firewall rule. 
The process is a __Custom Action__ which executes output of the Peernet.Browser.Setup.AddFirewallRule project. The project internally is a class library 
that runs netsh with proper arguments.
It is executed on __[Commit](https://docs.microsoft.com/en-us/dotnet/api/system.configuration.install.installer.commit?view=netframework-4.8)__ 
basic installation action. It means it executes only when [Stage 1](#stage1) is successfully completed and it doesn't impact overall installation status.
Since any firewall modification require Admin Rights the netsh will ask for administrator credentials to be able to start. The user can either provide 
the credentials or reject the request. If the user provides the valid credentials, the process will start in the background and exit once it is finished.
In case Admin credentials request has been rejected, the installer will inform the user that _The operation was canceled by the user_ and the installer 
will proceed. Note that rejecting the request will result in firewall rule not being added.

You can verify that the firewall rule has been correctly added by running __Powershell__ commands:
```
$fw=New-object -comObject HNetCfg.FwPolicy2
$fw.rules | findstr /i "Peernet"
```



### Peernet Browser Installer Insights{#insights}

#### Installation location
Installation location includes:

| File                           | Description                                                                                                                                              |
|--------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| InstallerAssets                | It is a directory with assets for the installer itself. It includes Peernet logo for installer header and Peernet logo for the executable shortcut file. |
| Backend.exe                    | The executable file for the Peernet Command Line Client compiled to lightweight version.                                                                 |
| Peernet.Browser.WPF.exe        | Peernet Browser executable file.                                                                                                                         |
| Peernet.Browser.WPF.dll.config | Peernet Browser configuration file. It contains settings like __ApiUrl__, __Backend__ executable path, __DownloadPath__.                                 |
|                                | All the remaining files are build output of the Peernet Browser solution. It consists of internal and 3rd party DLLs on which Peernet Browser depends.   |

Besides the installation location, the installer creates shortcuts to Peernet Browser executable on User's Desktop as well as Programs Menu.
After first boot in the installation location you may find also other files created by the __Backend.exe__.

#### Setup project build

Recommanded and simplest way of building the Setup project is via Visual Studio 2019.
In Visual Studio:

```
Right-Click 'Peernet.Browser.Setup' project in Solution Explorer ->  Select 'Build'
```

Peernet.Browser.Setup project Debug and Release build configurations are set to install the prerequisites. 
The prerequisite for the Peernet Browser is __.NET Runtime 5.0.8 (x64)__. In consequence, output installer will be installing the 
runtime if missing on the machine.  
The build also defines PostBuildEvents. Once the Peernet.Browser.Setup build succeeded, the following command is being executed:
```
D:\Tools\WIndowsKits\10\bin\10.0.22000.0\x86\MsiInfo.exe "D:\Sources\Peernet\BrowserSetup\Peernet.Browser.Setup\Release\Peernet.Browser.Setup.msi" -w 10
```
The event is modifying the __MSI__ file in order to allow non-admin users to successfully complete the installation.

Notes:
- The commandline above is machine specific and should be adjusted to whatever machine it is being built on.
- MsiInfo.exe is part of __[Windows SDK AddOn](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)__
- The MsiInfo.exe _-w 10_ switch sets the MSI "Word Count Summary" property of the MSI file to "compressed - elevated privileges are not required to install this package". More info [here](https://docs.microsoft.com/en-au/windows/win32/msi/word-count-summary)