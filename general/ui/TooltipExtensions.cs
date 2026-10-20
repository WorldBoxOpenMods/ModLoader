namespace NeoModLoader.General.UI;

/// <summary>Plain-text tooltips backed by the game's tooltip renderer.</summary>
public static class TooltipExtensions
{
    private const string title_key = "nml_tooltip_title";
    private const string text_key = "nml_tooltip_text";

    /// <summary>Read the title and description on each hover, including after a pooled row is reused.</summary>
    public static void SetTextTooltip(this TipButton pTip, Func<string> pTitle, Func<string> pText,
        bool pAddAnimation = true)
    {
        pTip.enabled = true;
        pTip.setHoverAction(() =>
        {
            string title = pTitle();
            string text = pText();
            // Native tooltips accept locale keys. Reuse these two keys for the currently hovered text.
            LM.AddToCurrentLocale(title_key, title ?? "");
            LM.AddToCurrentLocale(text_key, text ?? "");
            Tooltip.show(pTip.gameObject, "normal", new TooltipData
            {
                tip_name = title_key,
                tip_description = string.IsNullOrEmpty(text) ? null : text_key
            });
        }, pAddAnimation);
    }
}
