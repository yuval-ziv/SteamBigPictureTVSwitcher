# SteamBigPictureTVSwitcher
Start Steam BP and switch audio output to your TV seamlessly

## Requirements
- Windows. I don't know which versions are supported (targeted whatever I could). Tested it on Windows 11 version 10.0.22631.0. Get your version with `[Environment]::OSVersion.Version`.
- [Steam](https://store.steampowered.com/), obviously.
- [AudioDeviceCmdlets](https://www.powershellgallery.com/packages/AudioDeviceCmdlets/3.1.0.2), a Powershell module that allows you to interact with audio devices

## Usage
Just put a shortcut to the exe file in your startup folder

`C:\Users\{myusername}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup`

Or use any other way you can think of to launch this on startup, I don't care. You can also launch it manually, but what's the point?
> [!NOTE]
> The executables in the release are built for Windows version 10.0.22621.0.


## Environment Variables
> [!NOTE]
> Rename `example.env` to `.env` after you changed the config.

### Required:

- **TELEVISION_DISPLAY_NAME** - The display name Steam switches to when starting BP. Can be found using `Get-CimInstance -Namespace root\wmi -Class WmiMonitorBasicDisplayParams`.
- **TELEVISION_AUDIO_DEVICE** - The ID of the desired audio device when BP is on. Can be found using Get-AudioDevice -List
- **DEVICE_USAGE_PAGE** - See more in the [Hardware IDs section](#hardware-ids).
- **DEVICE_USAGE_ID** - See more in the [Hardware IDs section](#hardware-ids).

### Optional:

- **PRINT_RAW_COMMAND_OUTPUT** - prints all commands output before normalization. (`FALSE`)
- **PRINT_NORMALIZED_COMMAND_OUTPUT** - prints all commands output after normalization. (`FALSE`)
- **PATH_TO_STEAM_EXECUTABLE** - if you installed Steam somewhere else for some reason. (`C:\Program Files (x86)\Steam\steam.exe`)
- **DEVICE_VENDOR_ID** - See more in the [Hardware IDs section](#hardware-ids).
- **DEVICE_PRODUCT_ID** - See more in the [Hardware IDs section](#hardware-ids).



## Hardware IDs
I'm not going to try and add anything to a microsoft documentation, they are much better in documenting than me.

So, [see more here](https://learn.microsoft.com/en-us/uwp/api/windows.devices.humaninterfacedevice.hiddevice?view=winrt-22000).

## Building from Source

### Requirements

- [DotNet 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0#:~:text=SDK%209.0.102)

### Command
`dotnet publish -r win-x86 -p:PublishSingleFile=true -f net9.0-windows10.0.22621.0`

Replace the desired Windows version with another one. There are a few in the [csproj file](SteamBigPictureTVSwitcher/SteamBigPictureTVSwitcher.csproj).


## Contributing
Publish a PR and if it's okay I'll approve it. I'm not planning to touching this project too much.