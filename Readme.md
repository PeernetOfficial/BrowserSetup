# Peernet Browser Installer Project (Setup)

Peernet.Browser.Setup is [Microsoft Visual Studio Installer Project](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects).
Its purpose is to deliver self-sufficient installation process under single __.msi__ file.
Setup process is devided into __two stages__:
### Stage 1. Main installation
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



### Peernet Browser Installer Insights

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
After first boot in the installation location you may find also other files created by the __Backend.exe__ under the __data__ folder.

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

> Notes:
> - The command line above is machine specific and should be adjusted to whatever machine it is being built on.
> - MsiInfo.exe is part of __[Windows SDK AddOn](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)__
> - The MsiInfo.exe _-w 10_ switch sets the MSI "Word Count Summary" property of the MSI file to "compressed - elevated privileges are not required to install this package". More info [here](https://docs.microsoft.com/en-au/windows/win32/msi/word-count-summary)

##### Project can be also built from command line.
If you already have Visual Studio installed you can use **devenv.exe** from its files location to build the project with command:

```
D:\Microsoft Visual Studio\2019\Professional\Common7\IDE>devenv D:\Sources\Peernet\BrowserSetup\Peernet.Browser.Setup\Peernet.Browser.Setup.vdproj /build "Release|Any CPU"
```

However there is no guarantee the build will succeed.  
If your build fails with error:
> ERROR: An error occurred while validating.  HRESULT = '8000000A'

It is most likely related to your _EnableOutOfProcBuild_ registry entry. To fix the error you should disable it.
Depending on Visual Studio version:
- [VS2013] Set to 0
 >HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\12.0_Config\MSBuild\EnableOutOfProcBuild

- [VS2015] Set to 0
 >HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\MSBuild\EnableOutOfProcBuild

- [VS2019] Run:
 >D:\Microsoft Visual Studio\2019\Professional\Common7\IDE\CommonExtensions\Microsoft\VSI\DisableOutOfProcBuild\DisableOutOfProcBuild.exe

### Logo

The installer logo is included in this repository as file `Installer.png` and `Peernet.ico`. Since the logo is not expected to change, it does not require to be replaced.

## Required Files

### Browser Files

This is the list of required files in the `Peernet.Browser.WPF` folder. They must be copied from the directory `\Peernet.Browser.WPF\bin\Release\net5.0-windows\win-x64\publish` from the [Peernet Browser repository](https://github.com/PeernetOfficial/Browser). The file `Peernet Browser.dll.config` is always included in this repository and contains recommended default values. Make sure it does not get overwritten accidentally by the Browser repository version when copying the files.

```
Application.dll
Infrastructure.dll
Microsoft.Extensions.DependencyInjection.Abstractions.dll
Microsoft.Extensions.Logging.Abstractions.dll
Microsoft.Extensions.Logging.dll
Microsoft.Extensions.Options.dll
Microsoft.Extensions.Primitives.dll
Models.dll
MvvmCross.dll
MvvmCross.Platforms.Wpf.dll
MvvmCross.Plugin.Control.dll
MvvmCross.Plugin.Control.Platforms.Wpf.dll
Newtonsoft.Json.dll
Peernet Browser.deps.json
Peernet Browser.dll
Peernet Browser.dll.config
Peernet Browser.exe
Peernet Browser.runtimeconfig.json
Serilog.dll
Serilog.Extensions.Logging.dll
Serilog.Sinks.Trace.dll
System.Drawing.Common.dll
```

### Backend

The backend executable must be copied as file `Backend.exe` in this root folder. It can be compiled from the [Cmd repository](https://github.com/PeernetOfficial/Cmd).
