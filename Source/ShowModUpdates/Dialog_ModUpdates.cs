using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace ShowModUpdates;

[StaticConstructorOnStartup]
public class Dialog_ModUpdates : Window
{
    private const int headerHeight = 50;
    private const int rowHeight = 60;
    private static Vector2 scrollPosition;
    private static readonly Color alternateBackground = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    private static readonly Vector2 previewImage = new Vector2(120f, 100f);
    private static readonly Vector2 buttonSize = new Vector2(140f, 25f);
    private static readonly Texture2D discordIcon = ContentFinder<Texture2D>.Get("UI/Discord");
    private static readonly Texture2D steamIcon = ContentFinder<Texture2D>.Get("UI/Steam");
    private static readonly Texture2D folderIcon = ContentFinder<Texture2D>.Get("UI/Folder");
    private static List<ModWithUpdateInfo> localModUpdates;

    public Dialog_ModUpdates()
    {
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
        localModUpdates = ShowModUpdatesMod.instance.Settings.OrderByDate
            ? ShowModUpdates.ModUpdates.OrderByDescending(info => info.Updated).ToList()
            : ShowModUpdates.ModUpdates.OrderBy(info => info.ModMetaData.Name).ToList();
    }

    public override Vector2 InitialSize => new Vector2(700f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        Text.Font = GameFont.Medium;

        listingStandard.Label("SMU.ModListTitle".Translate(localModUpdates.Count,
            ShowModUpdates.NiceDate(ShowModUpdates.SelectedDate)));
        Text.Font = GameFont.Small;

        if (SteamManager.Initialized && SteamUtils.IsOverlayEnabled())
        {
            listingStandard.CheckboxLabeled("SMU.preferOverlay".Translate(),
                ref ShowModUpdatesMod.instance.Settings.PreferOverlay,
                "SMU.preferOverlaytt".Translate());
        }

        var subtitleRect = listingStandard.GetRect(50f);
        Widgets.Label(subtitleRect.BottomHalf(),
            ShowModUpdatesMod.instance.Settings.CheckAll
                ? "SMU.CheckingAll".Translate()
                : "SMU.CheckingEnabled".Translate());
        Widgets.Label(subtitleRect.TopHalf(), ShowModUpdates.CurrentSaveName);
        listingStandard.End();

        var borderRect = inRect;
        borderRect.y += subtitleRect.y + headerHeight;
        borderRect.height -= subtitleRect.y + headerHeight;
        var scrollContentRect = inRect;
        scrollContentRect.height = localModUpdates.Count * (rowHeight + 1);
        if (ShowModUpdates.GameUpdated)
        {
            scrollContentRect.height += rowHeight;
        }

        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);

        var alternate = false;

        if (ShowModUpdates.GameUpdated)
        {
            var rowRectFull = scrollListing.GetRect(rowHeight);
            alternate = true;
            Widgets.DrawBoxSolid(rowRectFull, alternateBackground);

            var rowRect = rowRectFull.ContractedBy(5f);

            var modInfoRect = rowRect.RightPartPixels(rowRect.width - previewImage.x - 5f);
            var modName = $"RimWorld {VersionControl.CurrentVersionString}";


            Widgets.Label(modInfoRect.TopHalf(), modName);


            var currentX = 0;
            var bottomPart = modInfoRect.BottomHalf();
            var steamRect = new Rect(bottomPart.position, buttonSize);
            if (Widgets.ButtonText(steamRect, "SteamDB"))
            {
                openUrl(ShowModUpdates.RimworldSteamDB);
            }

            TooltipHandler.TipRegion(steamRect, ShowModUpdates.RimworldSteamDB);

            currentX += (int)buttonSize.x + 5;

            var changelogRect = new Rect(new Vector2(bottomPart.x + currentX, bottomPart.y), buttonSize);
            if (Widgets.ButtonText(changelogRect, "SMU.Changenotes".Translate()))
            {
                openUrl(ShowModUpdates.RimworldChangenotes);
            }

            TooltipHandler.TipRegion(changelogRect, ShowModUpdates.RimworldChangenotes);
            currentX += (int)buttonSize.x + 5;


            changelogRect = new Rect(new Vector2(bottomPart.x + currentX, bottomPart.y), buttonSize);
            var wikiString = $"{ShowModUpdates.RimworldWiki}{VersionControl.CurrentVersionString}";
            if (Widgets.ButtonText(changelogRect, "Wiki"))
            {
                openUrl(wikiString);
            }

            TooltipHandler.TipRegion(changelogRect, wikiString);

            if (Widgets.ButtonImageFitted(bottomPart.RightPartPixels(25f), discordIcon))
            {
                openUrl(ShowModUpdates.RimworldDiscord);
            }

            TooltipHandler.TipRegion(bottomPart.RightPartPixels(25f), ShowModUpdates.RimworldDiscord);

            var previewRect = rowRect.LeftPartPixels(previewImage.x);

            Widgets.DrawBoxSolid(previewRect.ContractedBy(1f), Color.black);
            Widgets.DrawTextureFitted(previewRect.ContractedBy(1f), ShowModUpdates.RimworldBackGround, 1f);

            Widgets.DrawTextureFitted(previewRect.TopHalf().ContractedBy(previewRect.width * 0.2f, 0),
                ShowModUpdates.RimworldTitle, 1f);
            Widgets.DrawTextureFitted(previewRect.BottomHalf().LeftHalf().LeftHalf(), ShowModUpdates.RimworldIcon, 1f);
            TooltipHandler.TipRegion(previewRect.BottomHalf().LeftHalf().LeftHalf(), "RimWorld");
        }

        foreach (var modInfo in localModUpdates)
        {
            var rowRectFull = scrollListing.GetRect(rowHeight);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRectFull, alternateBackground);
            }

            var rowRect = rowRectFull.ContractedBy(5f);

            var modInfoRect = rowRect.RightPartPixels(rowRect.width - previewImage.x - 5f);
            var modName = modInfo.ModMetaData.Name;


            Widgets.Label(modInfoRect.TopHalf(), modName);
            if (modInfo.Updated != default)
            {
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(modInfoRect.TopHalf(), ShowModUpdates.NiceDate(modInfo.Updated));
                Text.Anchor = TextAnchor.UpperLeft;
            }


            var currentX = 0;
            var bottomPart = modInfoRect.BottomHalf();
            if (modInfo.SteamUri != null)
            {
                var steamRect = new Rect(bottomPart.position, buttonSize);
                if (Widgets.ButtonText(steamRect, "Steam"))
                {
                    openUrl(modInfo.SteamUri.AbsoluteUri);
                }

                TooltipHandler.TipRegion(steamRect, modInfo.SteamUri.AbsoluteUri);

                currentX += (int)buttonSize.x + 5;
            }

            if (modInfo.SteamChangelog != null)
            {
                var changelogRect = new Rect(new Vector2(bottomPart.x + currentX, bottomPart.y), buttonSize);
                if (Widgets.ButtonText(changelogRect, "SMU.Changenotes".Translate()))
                {
                    openUrl(modInfo.SteamChangelog.AbsoluteUri);
                }

                TooltipHandler.TipRegion(changelogRect, modInfo.SteamChangelog.AbsoluteUri);

                currentX += (int)buttonSize.x + 5;
            }

            if (modInfo.OtherUrl != null)
            {
                var domain = modInfo.OtherUrl.Host;
                var changelogRect = new Rect(new Vector2(bottomPart.x + currentX, bottomPart.y), buttonSize);
                if (Widgets.ButtonText(changelogRect, domain))
                {
                    openUrl(modInfo.OtherUrl.AbsoluteUri);
                }

                TooltipHandler.TipRegion(changelogRect, modInfo.OtherUrl.AbsoluteUri);
            }

            if (modInfo.DiscordUri != null)
            {
                if (Widgets.ButtonImageFitted(bottomPart.RightPartPixels(25f), discordIcon))
                {
                    openUrl(modInfo.DiscordUri.AbsoluteUri);
                }

                TooltipHandler.TipRegion(bottomPart.RightPartPixels(25f), modInfo.DiscordUri.AbsoluteUri);
            }

            var previewRect = rowRect.LeftPartPixels(previewImage.x);

            if (modInfo.ModMetaData.PreviewImage != null)
            {
                Widgets.DrawBoxSolid(previewRect.ContractedBy(1f), Color.black);
                Widgets.DrawTextureFitted(previewRect.ContractedBy(1f), modInfo.ModMetaData.PreviewImage, 1f);
            }

            if (modInfo.ModMetaData.OnSteamWorkshop)
            {
                Widgets.DrawTextureFitted(previewRect.BottomHalf().LeftHalf().LeftHalf(), steamIcon, 1f);
                TooltipHandler.TipRegion(previewRect.BottomHalf().LeftHalf().LeftHalf(), "SMU.SteamMod".Translate());
                continue;
            }

            Widgets.DrawTextureFitted(previewRect.BottomHalf().LeftHalf().LeftHalf(), folderIcon, 1f);
            TooltipHandler.TipRegion(previewRect.BottomHalf().LeftHalf().LeftHalf(), "SMU.LocalMod".Translate());
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }

    private static void openUrl(string url)
    {
        if (ShowModUpdatesMod.instance.Settings.PreferOverlay)
        {
            SteamUtility.OpenUrl(url);
        }
        else
        {
            Application.OpenURL(url);
        }
    }
}