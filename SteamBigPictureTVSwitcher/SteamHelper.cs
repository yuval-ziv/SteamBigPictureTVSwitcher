using System.Text.RegularExpressions;
using DotNetEnv;

namespace SteamBigPictureTVSwitcher;

public partial class SteamHelper
{
    private const char Apostrophe = '\'';
    private const string DefaultPathToSteamExecutable = @"'C:\Program Files (x86)\Steam\steam.exe'";

    private const string SpaceRegex = @"\s+";

    [GeneratedRegex(SpaceRegex)]
    private static partial Regex SpaceMatcher();

    public static void StartSteamBigPicture()
    {
        string rawPathToSteamExecutable = Env.GetString("PATH_TO_STEAM_EXECUTABLE", DefaultPathToSteamExecutable);

        rawPathToSteamExecutable = AddQuotationIfNeeded(rawPathToSteamExecutable);

        $"& {rawPathToSteamExecutable} -start steam://open/bigpicture -fulldesktopres".RunUsingPowershell();
    }

    private static string AddQuotationIfNeeded(string rawPathToSteamExecutable)
    {
        if (!SpaceMatcher().IsMatch(rawPathToSteamExecutable))
        {
            return rawPathToSteamExecutable;
        }

        if (!rawPathToSteamExecutable.StartsWith(Apostrophe))
        {
            rawPathToSteamExecutable = Apostrophe + rawPathToSteamExecutable;
        }

        if (!rawPathToSteamExecutable.EndsWith(Apostrophe))
        {
            rawPathToSteamExecutable += Apostrophe;
        }

        return rawPathToSteamExecutable;
    }
}