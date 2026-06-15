using Content.Client.Stylesheets.Palette;

namespace Content.Client.Stylesheets.Stylesheets;

public partial class NanotrasenStylesheet
{
    public override ColorPalette PrimaryPalette => Palettes.Neutral;
    public override ColorPalette SecondaryPalette => Palettes.Dark;
    public override ColorPalette PositivePalette => Palettes.Blue;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => Palettes.Gold;
}
