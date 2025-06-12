using Mlie;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace ShowModUpdates;

[StaticConstructorOnStartup]
internal class ShowModUpdatesMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static ShowModUpdatesMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public ShowModUpdatesMod(ModContentPack content) : base(content)
    {
        Instance = this;
        Settings = GetSettings<ShowModUpdatesSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal ShowModUpdatesSettings Settings { get; }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Show Mod Updates";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        if (listingStandard.ButtonText(ShowModUpdates.GetUpdatesString(), widthPct: 0.5f))
        {
            Find.WindowStack.Add(new Dialog_ModUpdates());
        }

        listingStandard.Gap();
        listingStandard.CheckboxLabeled("SMU.CheckAll".Translate(), ref Settings.CheckAll,
            "SMU.CheckAllTT".Translate());
        listingStandard.CheckboxLabeled("SMU.CheckOnline".Translate(), ref Settings.CheckOnline,
            "SMU.CheckOnlineTT".Translate());

        if (Settings.CheckOnline)
        {
            listingStandard.CheckboxLabeled("SMU.OrderByDate".Translate(), ref Settings.OrderByDate,
                "SMU.OrderByDateTT".Translate());
        }

        if (SteamManager.Initialized)
        {
            listingStandard.CheckboxLabeled("SMU.preferOverlay".Translate(), ref Settings.PreferOverlay,
                "SMU.preferOverlaytt".Translate());
        }

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("SMU.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ShowModUpdates.FinishedLoading = false;
        ShowModUpdates.ReadyToRead();
    }
}