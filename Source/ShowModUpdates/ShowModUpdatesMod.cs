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
    public static ShowModUpdatesMod instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public ShowModUpdatesMod(ModContentPack content) : base(content)
    {
        instance = this;
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
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        if (listing_Standard.ButtonText(ShowModUpdates.GetUpdatesString(), widthPct: 0.5f))
        {
            Find.WindowStack.Add(new Dialog_ModUpdates());
        }

        listing_Standard.Gap();
        listing_Standard.CheckboxLabeled("SMU.CheckAll".Translate(), ref Settings.CheckAll,
            "SMU.CheckAllTT".Translate());
        listing_Standard.CheckboxLabeled("SMU.CheckOnline".Translate(), ref Settings.CheckOnline,
            "SMU.CheckOnlineTT".Translate());

        if (Settings.CheckOnline)
        {
            listing_Standard.CheckboxLabeled("SMU.OrderByDate".Translate(), ref Settings.OrderByDate,
                "SMU.OrderByDateTT".Translate());
        }

        if (SteamManager.Initialized)
        {
            listing_Standard.CheckboxLabeled("SMU.preferOverlay".Translate(), ref Settings.PreferOverlay,
                "SMU.preferOverlaytt".Translate());
        }

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("SMU.CurrentModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ShowModUpdates.FinishedLoading = false;
        ShowModUpdates.ReadyToRead();
    }
}