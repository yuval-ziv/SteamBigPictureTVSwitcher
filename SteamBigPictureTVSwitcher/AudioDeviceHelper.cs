using System.Text.RegularExpressions;

namespace SteamBigPictureTVSwitcher;

public class AudioDevice
{
    public int Index { get; set; }
    public bool Default { get; set; }
    public bool DefaultCommunication { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string ID { get; set; }
    public string Device { get; set; }
}

public static partial class AudioDeviceHelper
{
    [GeneratedRegex(@"(?<key>.+?)\s*:\s*(?<value>.+)")]
    private static partial Regex KeyValuePairMatcher();

    public static AudioDevice GetCurrentAudioDevice()
    {
        string commandOutput = "Get-AudioDevice -PlaybackCommunication".RunUsingPowershell();

        AudioDevice audioDevice = ParseCommandOutput(commandOutput);

        Console.WriteLine($"Current audio device is {audioDevice.Name} ({audioDevice.ID}).");

        return audioDevice;
    }

    public static void ChangeDefaultAudioDevice(string audioDeviceName)
    {
        Console.WriteLine($"Changing default audio device to name {audioDeviceName}");

        string listAudioDevicesCommandResponse = "Get-AudioDevice -List".RunUsingPowershell();
        Console.WriteLine(listAudioDevicesCommandResponse);
        List<AudioDevice> audioDevices = listAudioDevicesCommandResponse.Split([Environment.NewLine + Environment.NewLine], StringSplitOptions.RemoveEmptyEntries).Select(ParseCommandOutput).ToList();
        Console.WriteLine($"Audio devices {audioDevices.Count}, [{string.Join(',', audioDevices.Select(d => d.Name))}]");
        AudioDevice? desiredAudioDevice = audioDevices.FirstOrDefault(device => device.Name == audioDeviceName);

        if (desiredAudioDevice is null)
        {
            throw new Exception($"Couldn't find audio device named {audioDeviceName}");
        }

        $"Set-AudioDevice -ID '{desiredAudioDevice.ID}'".RunUsingPowershell();
        Console.WriteLine($"Changed default audio device to {desiredAudioDevice.ID} successfully [Name - {desiredAudioDevice.Name}].");
    }

    private static AudioDevice ParseCommandOutput(string input)
    {
        var audioDevice = new AudioDevice();

        foreach (Match match in KeyValuePairMatcher().Matches(input))
        {
            string key = match.Groups["key"].Value.Trim();
            string value = match.Groups["value"].Value.Trim();

            switch (key)
            {
                case "Index":
                    audioDevice.Index = int.Parse(value);
                    break;
                case "Default":
                    audioDevice.Default = bool.Parse(value);
                    break;
                case "DefaultCommunication":
                    audioDevice.DefaultCommunication = bool.Parse(value);
                    break;
                case "Type":
                    audioDevice.Type = value;
                    break;
                case "Name":
                    audioDevice.Name = value;
                    break;
                case "ID":
                    audioDevice.ID = value;
                    break;
                case "Device":
                    audioDevice.Device = value;
                    break;
            }
        }

        return audioDevice;
    }
}