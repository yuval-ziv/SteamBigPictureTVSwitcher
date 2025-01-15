using DotNetEnv;
using Microsoft.Win32;
using static SteamBigPictureTVSwitcher.AudioDeviceHelper;

namespace SteamBigPictureTVSwitcher;

internal class Program
{
    private static string _televisionDeviceName = "";
    private static string _televisionAudioDeviceName = "";

    private static AudioDevice _lastAudioDevice = new();

    [STAThread]
    private static void Main()
    {
        Env.Load();

        _televisionDeviceName = Env.GetString("TELEVISION_DISPLAY_NAME");
        _televisionAudioDeviceName = Env.GetString("TELEVISION_AUDIO_DEVICE_NAME");

        var bluetoothDeviceMonitor = new DeviceMonitor();
        _lastAudioDevice = GetCurrentAudioDevice();

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
            _lastAudioDevice = GetCurrentAudioDevice();
            Console.WriteLine("Primary screen is television");
            ChangeDefaultAudioDevice(_televisionAudioDeviceName);
        }
        else
        {
            Console.WriteLine($"Primary screen is not television [name - '{primaryScreen?.DeviceName}']");
            ChangeDefaultAudioDevice(_lastAudioDevice.Name);
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