using System.Globalization;
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
    private static readonly Vector2 buttonSize = new Vector2(150f, 25f);

    public Dialog_ModUpdates()
    {
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new Vector2(650f, 600f);

    public override void DoWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        Text.Font = GameFont.Medium;

        listingStandard.Label("SMU.ModListTitle".Translate(ShowModUpdates.ModUpdates.Count,
            ShowModUpdates.SelectedDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern)));
        Text.Font = GameFont.Tiny;
        var headerLabel = listingStandard.Label(ShowModUpdates.CurrentSavePath);
        Text.Font = GameFont.Small;
        listingStandard.End();

        var borderRect = inRect;
        borderRect.y += headerLabel.y + headerHeight;
        borderRect.height -= headerLabel.y + headerHeight;
        var scrollContentRect = inRect;
        scrollContentRect.height = ShowModUpdates.ModUpdates.Count * (rowHeight + 1);
        scrollContentRect.width -= 20;
        scrollContentRect.x = 0;
        scrollContentRect.y = 0;

        var scrollListing = new Listing_Standard();
        Widgets.BeginScrollView(borderRect, ref scrollPosition, scrollContentRect);
        scrollListing.Begin(scrollContentRect);

        var alternate = false;
        foreach (var modInfo in ShowModUpdates.ModUpdates)
        {
            var rowRectFull = scrollListing.GetRect(rowHeight);
            alternate = !alternate;
            if (alternate)
            {
                Widgets.DrawBoxSolid(rowRectFull, alternateBackground);
            }

            var rowRect = rowRectFull.ContractedBy(5f);

            var modInfoRect = rowRect.RightPartPixels(rowRect.width - previewImage.x - 5f);
            Widgets.Label(modInfoRect.TopHalf(), modInfo.ModMetaData.Name);

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

            var previewRect = rowRect.LeftPartPixels(previewImage.x);

            if (modInfo.ModMetaData.PreviewImage != null)
            {
                Widgets.DrawTextureFitted(previewRect.ContractedBy(1f), modInfo.ModMetaData.PreviewImage, 1f);
            }

            TooltipHandler.TipRegion(previewRect.ContractedBy(1f), modInfo.ModMetaData.Description);
        }

        scrollListing.End();
        Widgets.EndScrollView();
    }
}