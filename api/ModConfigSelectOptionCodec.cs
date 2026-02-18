using Newtonsoft.Json;

namespace NeoModLoader.api;

internal static class ModConfigSelectOptionCodec
{
    public static string[] Parse(string pRaw)
    {
        if (string.IsNullOrWhiteSpace(pRaw)) return Array.Empty<string>();
        string raw = pRaw.Trim();
        if (raw.StartsWith("["))
            try
            {
                var parsed_options = JsonConvert.DeserializeObject<List<string>>(raw);
                if (parsed_options != null)
                    return parsed_options
                        .Where(option => !string.IsNullOrWhiteSpace(option))
                        .Select(option => option.Trim())
                        .ToArray();
            }
            catch
            {
            }

        return raw
            .Split('|')
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .ToArray();
    }

    public static int ClampIndex(int pIndex, string[] pOptions)
    {
        if (pOptions == null || pOptions.Length == 0) return pIndex;
        if (pIndex < 0) return 0;
        if (pIndex >= pOptions.Length) return pOptions.Length - 1;
        return pIndex;
    }

    public static string Serialize(IEnumerable<string> pOptions)
    {
        if (pOptions == null) return "";
        var options = pOptions
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Select(option => option.Trim())
            .ToArray();
        if (options.Length == 0) return "";
        return JsonConvert.SerializeObject(options);
    }
}
