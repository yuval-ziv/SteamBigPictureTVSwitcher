using DotNetEnv;
using Microsoft.Win32;
using static SteamBigPictureTVSwitcher.AudioDeviceHelper;

namespace SteamBigPictureTVSwitcher;

// Requires Powershell module - AudioDeviceCmdlets (https://www.powershellgallery.com/packages/AudioDeviceCmdlets/3.1.0.2)

internal class Program
{
    private static string _televisionDeviceName = "";
    private static string _televisionAudioDevice = "";

    private static string _lastAudioDeviceId = "";

    [STAThread]
    private static void Main()
    {
        Env.Load();

        _televisionDeviceName = Env.GetString("TELEVISION_DISPLAY_NAME");
        _televisionAudioDevice = Env.GetString("TELEVISION_AUDIO_DEVICE");

        var bluetoothDeviceMonitor = new DeviceMonitor();
        _lastAudioDeviceId = GetCurrentAudioDeviceId();

        SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        bluetoothDeviceMonitor.DeviceAdded += DeviceAdded;
        bluetoothDeviceMonitor.DeviceRemoved += DeviceRemoved;

        bluetoothDeviceMonitor.StartMonitoring();
        Console.WriteLine("Listening for display changes. Press Enter to exit...");

        Console.ReadLine();
        SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;
        bluetoothDeviceMonitor.DeviceAdded -= DeviceAdded;
        bluetoothDeviceMonitor.DeviceRemoved -= DeviceRemoved;
        bluetoothDeviceMonitor.Dispose();
    }


    private static void DisplaySettingsChanged(object? sender, EventArgs e)
    {
        var primaryScreen = Screen.PrimaryScreen;

        if (primaryScreen?.DeviceName == _televisionDeviceName)
        {
            _lastAudioDeviceId = GetCurrentAudioDeviceId();
            Console.WriteLine("Primary screen is television");
            ChangeDefaultAudioDevice(_televisionAudioDevice);
        }
        else
        {
            Console.WriteLine($"Primary screen is not television [name - '{primaryScreen?.DeviceName}']");
            ChangeDefaultAudioDevice(_lastAudioDeviceId);
        }
    }

    private static void DeviceAdded(DeviceMonitor monitor, MonitoredDevice device)
    {
        Console.WriteLine($"Device connected [Id:{device.Id},Name:{device.Name},InstanceId:{device.InstanceId}]");
        Console.WriteLine("Starting steam in Big Picture mode");
        SteamHelper.StartSteamBigPicture();
        Console.WriteLine("Started steam in Big Picture mode");
    }

    private static void DeviceRemoved(DeviceMonitor monitor, MonitoredDevice device)
    {
        Console.WriteLine($"Device disconnected [Id:{device.Id},Name:{device.Name},InstanceId:{device.InstanceId}]");
    }
}