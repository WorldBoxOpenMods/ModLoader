using System.Net;
using System.Text.RegularExpressions;
using NeoModLoader.constants;
using NeoModLoader.services;

namespace NeoModLoader.utils.installers;

internal class GBModInstaller : ACmdModInstaller
{
    private const string base_match_regex = @"^(?<scheme>ncms|nml):(?<url_to_archive>.*)$";

    private const string addition_match_regex =
        @"^(?<scheme>ncms|nml):(?<url_to_archive>.*),(?<mod_type>.*),(?<mod_id>.*)$";

    public override async Task<bool> CheckInstall(string pParam)
    {
        if (!(pParam.StartsWith("ncms:") || pParam.StartsWith("nml:"))) return false;

        Match match = null;

        var addition_match = Regex.IsMatch(pParam, addition_match_regex);
        if (!addition_match)
        {
            var base_match = Regex.IsMatch(pParam, base_match_regex);
            if (!base_match)
                return false;
            match = Regex.Match(pParam, base_match_regex);
        }
        else
        {
            match = Regex.Match(pParam, addition_match_regex);
        }

        var url_to_archive = match.Groups["url_to_archive"].Value;
        using var client = new WebClient();

        var zip_file_path = Path.Combine(Paths.ModsPath, Guid.NewGuid() + ".zip");

        await client.DownloadFileTaskAsync(new Uri(url_to_archive), zip_file_path);

        var mod_folder_path = ModInfoUtils.TryToUnzipModZip(zip_file_path);

        return ModCompileLoadService.TryCompileAndLoadModAtRuntime(ModInfoUtils.recogMod(mod_folder_path));
    }
}