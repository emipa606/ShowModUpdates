using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using Steamworks;
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
    public static readonly List<ModWithUpdateInfo> AllSeenMods = [];
    public static bool Scanning;
    private static CallResult<SteamUGCQueryCompleted_t> OnSteamUGCQueryCompletedCallResult;
    private static SteamUGCQueryCompleted_t collectionQueryResult;

    static ShowModUpdates()
    {
        new Harmony("Mlie.ShowModUpdates").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static string NiceDate(DateTime date)
    {
        //return date.ToString("yy-MM-dd HH:mm:ss");
        return date.ToString(Prefs.TwelveHourClockMode ? "g" : "yy-MM-dd HH:mm:ss");
    }

    public static bool ReadyToRead()
    {
        if (Scanning)
        {
            return false;
        }

        if (FinishedLoading)
        {
            return true;
        }

        Scanning = true;
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            CheckModUpdates();
        }).Start();
        return false;
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
            Scanning = false;
            FinishedLoading = true;
            return;
        }

        if (!File.Exists(CurrentSavePath))
        {
            Log.Warning($"[ShowModUpdates]: Could not find save-file at {CurrentSavePath}, cannot show mod-updates.");
            Scanning = false;
            FinishedLoading = true;
            return;
        }

        SelectedDate = File.GetLastWriteTime(CurrentSavePath);
        ModUpdates = [];


        if (ShowModUpdatesMod.instance.Settings.CheckAll)
        {
            foreach (var installedMod in ModLister.AllInstalledMods)
            {
                var aboutFilePath = Path.Combine(installedMod.RootDir.FullName, "About", "About.xml");
                if (!File.Exists(aboutFilePath))
                {
                    Log.WarningOnce($"[ShowModUpdates]: Could not find About-file for '{installedMod.Name}'.",
                        installedMod.GetHashCode());
                    continue;
                }

                if (File.GetLastWriteTime(aboutFilePath) <= SelectedDate)
                {
                    continue;
                }

                var existingMod =
                    AllSeenMods.FirstOrDefault(mod => installedMod.SamePackageId(mod.ModMetaData.PackageId));
                if (existingMod != null)
                {
                    ModUpdates.Add(existingMod);
                }
                else
                {
                    ModUpdates.Add(new ModWithUpdateInfo(installedMod));
                }
            }
        }
        else
        {
            foreach (var modContentPack in LoadedModManager.RunningMods)
            {
                var aboutFilePath = Path.Combine(modContentPack.RootDir, "About", "About.xml");
                if (!File.Exists(aboutFilePath))
                {
                    Log.WarningOnce($"[ShowModUpdates]: Could not find About-file for '{modContentPack.Name}'.",
                        modContentPack.GetHashCode());
                    continue;
                }

                if (File.GetLastWriteTime(aboutFilePath) <= SelectedDate)
                {
                    continue;
                }

                var existingMod =
                    AllSeenMods.FirstOrDefault(mod =>
                        modContentPack.ModMetaData.SamePackageId(mod.ModMetaData.PackageId));
                if (existingMod != null)
                {
                    ModUpdates.Add(existingMod);
                }
                else
                {
                    ModUpdates.Add(new ModWithUpdateInfo(modContentPack.ModMetaData));
                }
            }
        }

        if (!ModUpdates.Any())
        {
            Log.Message($"[ShowModUpdates]: {"SMU.LogMessageNone".Translate(NiceDate(SelectedDate))}");
            Scanning = false;
            FinishedLoading = true;
            return;
        }

        Log.Message(
            $"[ShowModUpdates]: {"SMU.LogMessage".Translate(ModUpdates.Count, NiceDate(SelectedDate))}\n{string.Join("\n", ModUpdates.Select(info => info.ModMetaData.Name))}");

        if (ShowModUpdatesMod.instance.Settings.CheckOnline)
        {
            PopulateDescriptions(ModUpdates);
        }

        ModUpdates.ForEach(modInfo => modInfo.PopulateLinks());
        Scanning = false;
        FinishedLoading = true;
    }

    public static void PopulateDescriptions(List<ModWithUpdateInfo> mods)
    {
        var modsToCheck = mods.Where(mod => !mod.Synced && mod.PublishedFileId != PublishedFileId_t.Invalid).ToList();

        if (!modsToCheck.Any())
        {
            return;
        }

        var idsToCheck = modsToCheck.Select(mod => mod.PublishedFileId).ToArray();

        if (!idsToCheck.Any())
        {
            return;
        }

        OnSteamUGCQueryCompletedCallResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
        collectionQueryResult = new SteamUGCQueryCompleted_t();
        var UGCDetailsRequest = SteamUGC.CreateQueryUGCDetailsRequest(idsToCheck, (uint)idsToCheck.Length);
        SteamUGC.SetReturnLongDescription(UGCDetailsRequest, true);
        SteamUGC.SetReturnChildren(UGCDetailsRequest, true);
        var createQueryUGCDetailsRequest = SteamUGC.SendQueryUGCRequest(UGCDetailsRequest);
        OnSteamUGCQueryCompletedCallResult.Set(createQueryUGCDetailsRequest);
        while (collectionQueryResult.m_eResult == EResult.k_EResultNone)
        {
            Thread.Sleep(50);
            SteamAPI.RunCallbacks();
        }

        if (collectionQueryResult.m_eResult != EResult.k_EResultOK)
        {
            return;
        }

        for (uint i = 0; i < idsToCheck.Length; i++)
        {
            if (SteamUGC.GetQueryUGCResult(UGCDetailsRequest, i, out var details))
            {
                modsToCheck[(int)i].Description = details.m_rgchDescription;
                modsToCheck[(int)i].Updated = UnixTimeToDateTime(details.m_rtimeUpdated);
            }

            modsToCheck[(int)i].Synced = true;
        }
    }

    private static DateTime UnixTimeToDateTime(double unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    private static void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t pCallback, bool bIOFailure)
    {
        collectionQueryResult = pCallback;
    }
}