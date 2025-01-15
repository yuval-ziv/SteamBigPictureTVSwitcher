using System.Text.RegularExpressions;

namespace SteamBigPictureTVSwitcher;

public static partial class AudioDeviceHelper
{
    private const string CaptureIdRegex = @"ID\s+:\s+({[^}]+}\.[{0-9a-fA-F\-}]+)";

    [GeneratedRegex(CaptureIdRegex)]
    private static partial Regex CaptureIdMatcher();

    private const string CaptureNameRegex = @"Name\s+:\s+(.*)";

    [GeneratedRegex(CaptureNameRegex)]
    private static partial Regex CaptureNameMatcher();

    public static string GetCurrentAudioDeviceId()
    {
        string commandOutput = "Get-AudioDevice -PlaybackCommunication".RunUsingPowershell();

        string currentAudioDeviceId = GetAudioDeviceIdFromCommandOutput(commandOutput);

        string audioDeviceName = GetAudioDeviceName(currentAudioDeviceId);
        Console.WriteLine($"Current audio device is {audioDeviceName} ({currentAudioDeviceId}).");

        return currentAudioDeviceId;
    }

    public static void ChangeDefaultAudioDevice(string audioDeviceId)
    {
        Console.WriteLine($"Changing default audio device to id {audioDeviceId}");
        $"Set-AudioDevice -ID '{audioDeviceId}'".RunUsingPowershell();
        string audioDeviceName = GetAudioDeviceName(audioDeviceId);
        Console.WriteLine($"Changed default audio device to {audioDeviceId} successfully [Name - {audioDeviceName}].");
    }

    private static string GetAudioDeviceName(string audioDeviceId)
    {
        string commandOutput = $"Get-AudioDevice -ID '{audioDeviceId}'".RunUsingPowershell();

        return GetAudioDeviceNameFromCommandOutput(commandOutput);
    }

    private static string GetAudioDeviceIdFromCommandOutput(string commandOutput)
    {
        Match match = CaptureIdMatcher().Match(commandOutput);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        throw new Exception($"Couldn't find current audio device id using this regex - {CaptureIdRegex} in powershell command output{Environment.NewLine}{commandOutput}");
    }

    private static string GetAudioDeviceNameFromCommandOutput(string commandOutput)
    {
        Match match = CaptureNameMatcher().Match(commandOutput);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        throw new Exception($"Couldn't find audio device name using this regex - {CaptureNameRegex} in powershell command output{Environment.NewLine}{commandOutput}");
    }
}