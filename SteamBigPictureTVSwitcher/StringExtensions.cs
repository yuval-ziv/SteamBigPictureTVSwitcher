using System.Diagnostics;
using System.Text.RegularExpressions;
using DotNetEnv;

namespace SteamBigPictureTVSwitcher;

public static partial class StringExtensions
{
    [GeneratedRegex(@"\x1B\[[0-?9;]*[mK]")]
    private static partial Regex AnsiEscapeCharactersRegex();

    public static string RunUsingPowershell(this string command)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = "pwsh.exe",
                Arguments = $"-Command \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            },
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string normalizedOutput = NormalizeOutput(output);
        PrintRawCommandOutputIfNeeded(output);
        PrintNormalizedCommandOutputIfNeeded(normalizedOutput);

        process.WaitForExit();

        return normalizedOutput;
    }

    private static string NormalizeOutput(string output)
    {
        return AnsiEscapeCharactersRegex().Replace(output, string.Empty);
    }

    private static void PrintRawCommandOutputIfNeeded(string output)
    {
        bool printRawCommandOutput = Env.GetBool("PRINT_RAW_COMMAND_OUTPUT");

        if (printRawCommandOutput)
        {
            Console.WriteLine(output);
        }
    }

    private static void PrintNormalizedCommandOutputIfNeeded(string output)
    {
        bool printNormalizedCommandOutput = Env.GetBool("PRINT_NORMALIZED_COMMAND_OUTPUT");

        if (printNormalizedCommandOutput)
        {
            Console.WriteLine(output);
        }
    }
}