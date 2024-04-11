using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ShowModUpdates;

[StaticConstructorOnStartup]
public static class ShowModUpdates
{
    public static readonly Uri SteamBaseUri = new Uri("https://steamcommunity.com/sharedfiles/filedetails/?id=");

    public static readonly List<string> DiscordDomains =
    [
        "dsc.gg",
        "discord.gg",
        "discord.com"
    ];

    public static readonly Uri SteamBaseChangelogUri =
        new Uri("https://steamcommunity.com/sharedfiles/filedetails/changelog/");

    public static string CurrentSavePath;
    public static string CurrentSaveName;
    public static DateTime SelectedDate;
    public static bool FinishedLoading;
    public static List<ModWithUpdateInfo> ModUpdates;

    static ShowModUpdates()
    {
        new Harmony("Mlie.ShowModUpdates").PatchAll(Assembly.GetExecutingAssembly());
        LongEventHandler.QueueLongEvent(CheckModUpdates, "ShowModUpdates.CheckModUpdates.Main", true, null);
    }

    public static void CheckModUpdates()
    {
        if (CurrentSavePath == null && GenFilePaths.AllSavedGameFiles.FirstOrDefault() is { } fileInfo)
        {
            CurrentSavePath = fileInfo.FullName;
            CurrentSaveName = fileInfo.Name;
        }

        if (CurrentSavePath == null)
        {
            Log.Message($"[ShowModUpdates]: {"SMU.LogSave".Translate()}");
            return;
        }

        if (!File.Exists(CurrentSavePath))
        {
            Log.Warning($"[ShowModUpdates]: Could not find save-file at {CurrentSavePath}, cannot show mod-updates.");
            return;
        }

        SelectedDate = File.GetLastWriteTime(CurrentSavePath);
        ModUpdates = [];

        foreach (var modContentPack in LoadedModManager.RunningMods)
        {
            var aboutFilePath = Path.Combine(modContentPack.RootDir, "About", "About.xml");
            if (!File.Exists(aboutFilePath))
            {
                Log.WarningOnce($"[ShowModUpdates]: Could not find About-file for '{modContentPack.Name}'.",
                    modContentPack.GetHashCode());
                continue;
            }

            if (File.GetLastWriteTime(aboutFilePath) > SelectedDate)
            {
                ModUpdates.Add(new ModWithUpdateInfo(modContentPack.ModMetaData));
            }
        }

        if (!ModUpdates.Any())
        {
            Log.Message(
                $"[ShowModUpdates]: {"SMU.LogMessageNone".Translate(SelectedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern))}");
            FinishedLoading = true;
            return;
        }


        Log.Message(
            $"[ShowModUpdates]: {"SMU.LogMessage".Translate(ModUpdates.Count, SelectedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern))}\n{string.Join("\n", ModUpdates.Select(info => info.ModMetaData.Name))}");
        ModUpdates.ForEach(modInfo => modInfo.PopulateLinks());

        FinishedLoading = true;
    }
}