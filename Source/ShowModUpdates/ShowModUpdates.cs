using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse;

namespace ShowModUpdates;

[StaticConstructorOnStartup]
public static class ShowModUpdates
{
    public static readonly Uri SteamBaseUri = new Uri("https://steamcommunity.com/sharedfiles/filedetails/?id=");

    public static readonly Texture2D RimworldIcon = ContentFinder<Texture2D>.Get("UI/HeroArt/RimWorldLogo");
    public static readonly Texture2D RimworldTitle = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle");
    public static readonly Texture2D RimworldBackGround = ContentFinder<Texture2D>.Get("UI/HeroArt/BGPlanet");
    public static readonly string RimworldChangenotes = "https://store.steampowered.com/news/app/294100";
    public static readonly string RimworldDiscord = "https://discord.gg/rimworld";
    public static readonly string RimworldWiki = "https://rimworldwiki.com/wiki/Version/";
    public static readonly string RimworldSteamDB = "https://steamdb.info/app/294100/patchnotes/";

    public static bool GameUpdated;

    public static readonly List<string> DiscordDomains =
    [
        "dsc.gg",
        "discord.gg",
        "discord.com"
    ];

    public static readonly Uri SteamBaseChangelogUri =
        new Uri("https://steamcommunity.com/sharedfiles/filedetails/changelog/");

    public static bool NoExistingSave;
    public static string CurrentSavePath;
    public static string CurrentSaveName;
    public static FileInfo CurrentSave;
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
        if (NoExistingSave)
        {
            return false;
        }

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
        GameUpdated = false;

        if (CurrentSavePath == null && GenFilePaths.AllSavedGameFiles.FirstOrDefault() is { } fileInfo)
        {
            Log.Message($"[ShowModUpdates]: Found latest save, {fileInfo.Name}");
            CurrentSavePath = fileInfo.FullName;
            CurrentSaveName = fileInfo.Name;
            CurrentSave = fileInfo;
        }

        if (CurrentSavePath == null)
        {
            Log.Message($"[ShowModUpdates]: {"SMU.LogSave".Translate()}");
            Scanning = false;
            FinishedLoading = true;
            NoExistingSave = true;
            CurrentSave = null;
            return;
        }

        if (!File.Exists(CurrentSavePath))
        {
            Log.Warning($"[ShowModUpdates]: Could not find save-file at {CurrentSavePath}, cannot show mod-updates.");
            Scanning = false;
            FinishedLoading = true;
            CurrentSave = null;
            return;
        }

        SelectedDate = File.GetLastWriteTime(CurrentSavePath);
        Log.Message($"[ShowModUpdates]: Last save writetime, {SelectedDate}");
        ModUpdates = [];

        var saveVersion = ScribeMetaHeaderUtility.GameVersionOf(CurrentSave);
        if (saveVersion != VersionControl.CurrentVersionString)
        {
            GameUpdated = true;
            Log.Message(
                $"[ShowModUpdates]: Rimworld has updated since the last save, is now {VersionControl.CurrentVersionString}, was {saveVersion}");
        }

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

    public static string GetUpdatesString()
    {
        if (!ModUpdates.Any())
        {
            return "SMU.GameUpdated".Translate();
        }

        var returnString = ModUpdates.Count == 1
            ? "SMU.CurrentUpdate".Translate(ModUpdates.Count)
            : "SMU.CurrentUpdates".Translate(ModUpdates.Count);

        if (GameUpdated)
        {
            returnString += $"{Environment.NewLine}{"SMU.GameUpdated".Translate()}";
        }

        return returnString;
    }
}