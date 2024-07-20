namespace NeoModLoader.General.UI.Tab;

public class TabBombs : ReconstructedVanillaTab
{
    protected override string[] Groups => new string[] {  };

    protected override void InitTab()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Bombs));
    }
}