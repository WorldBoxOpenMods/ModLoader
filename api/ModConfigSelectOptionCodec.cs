namespace NeoModLoader.api;

internal static class ModConfigSelectOptionCodec
{
    public static int ClampIndex(int pIndex, int pOptionCount)
    {
        if (pOptionCount <= 0) return pIndex;
        if (pIndex < 0) return 0;
        if (pIndex >= pOptionCount) return pOptionCount - 1;
        return pIndex;
    }

    public static int CountOptions(IEnumerable<string> pOptions)
    {
        if (pOptions == null) return 0;
        return pOptions
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Count();
    }
}
