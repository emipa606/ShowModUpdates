using System.Linq;
using UnityEngine;
using Verse;

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

    public Dialog_ModUpdates()
    {
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new Vector2(700f, 700f);

    public override void DoWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        Text.Font = GameFont.Medium;

        listingStandard.Label("SMU.ModListTitle".Translate(ShowModUpdates.ModUpdates.Count,
            ShowModUpdates.NiceDate(ShowModUpdates.SelectedDate)));
        Text.Font = GameFont.Small;
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
        scrollContentRect.height = ShowModUpdates.ModUpdates.Count * (rowHeight + 1);
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);

        var alternate = false;
        foreach (var modInfo in ShowModUpdates.ModUpdates.OrderBy(info => info.ModMetaData.Name))
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
                    Application.OpenURL(modInfo.SteamUri.AbsoluteUri);
                }

                TooltipHandler.TipRegion(steamRect, modInfo.SteamUri.AbsoluteUri);

                currentX += (int)buttonSize.x + 5;
            }

            if (modInfo.SteamChangelog != null)
            {
                var changelogRect = new Rect(new Vector2(bottomPart.x + currentX, bottomPart.y), buttonSize);
                if (Widgets.ButtonText(changelogRect, "SMU.Changenotes".Translate()))
                {
                    Application.OpenURL(modInfo.SteamChangelog.AbsoluteUri);
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
                    Application.OpenURL(modInfo.OtherUrl.AbsoluteUri);
                }

                TooltipHandler.TipRegion(changelogRect, modInfo.OtherUrl.AbsoluteUri);
            }

            if (modInfo.DiscordUri != null)
            {
                if (Widgets.ButtonImageFitted(bottomPart.RightPartPixels(25f), discordIcon))
                {
                    Application.OpenURL(modInfo.DiscordUri.AbsoluteUri);
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
}