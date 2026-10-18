using System.Net;
using System.Net.Http;
using NeoModLoader.constants;
using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.utils;

/// <summary>
///     Workshop preview images as sprites, cached in memory and on disk under the NML folder.
/// </summary>
/// <remarks>
///     Use it from Unity's main thread. File reads and downloads run in the background and their awaits resume on
///     the main thread, where textures must be created.
/// </remarks>
internal static class WorkshopThumbnails
{
    private const int max_parallel_downloads = 4;
    private const int memory_limit = 512;
    private const int max_image_bytes = 2 * 1024 * 1024;

    // Steam's image CDN resizes on request: a 128x128 JPEG of a few KB instead of the full preview.
    private const string small_size_query =
        "imw=128&imh=128&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false";

    private static readonly Dictionary<string, Sprite> sprites = new();
    private static readonly Queue<string> load_order = new();
    private static readonly Dictionary<string, List<Action<Sprite>>> waiting = new();
    private static readonly Dictionary<string, HashSet<object>> owners_by_key = new();
    private static readonly Dictionary<object, string> key_by_owner = new();
    private static readonly SemaphoreSlim download_slots = new(max_parallel_downloads);
    private static HttpClient http_client;

    private static string CacheFolder => Path.Combine(Paths.NMLPath, "workshop_thumbnails");

    /// <summary>
    ///     Call <paramref name="pOnLoaded" /> with the preview at <paramref name="pUrl" />: at once when it is
    ///     cached, otherwise once it has loaded. It is not called when the image cannot be loaded.
    /// </summary>
    public static void Request(string pUrl, object pOwner, Action<Sprite> pOnLoaded)
    {
        if (pOwner != null) AssignOwner(pOwner, pUrl == null ? null : UrlKey(pUrl));
        if (string.IsNullOrEmpty(pUrl)) return;

        string key = UrlKey(pUrl);
        if (sprites.TryGetValue(key, out Sprite sprite))
        {
            pOnLoaded(sprite);
            return;
        }

        if (waiting.TryGetValue(key, out List<Action<Sprite>> callbacks))
        {
            callbacks.Add(pOnLoaded);
            return;
        }

        waiting[key] = new List<Action<Sprite>> { pOnLoaded };
        _ = LoadAsync(pUrl, key);
    }

    private static async Task LoadAsync(string pUrl, string pKey)
    {
        Sprite sprite = null;
        try
        {
            string cache_path = Path.Combine(CacheFolder, pKey + ".img");
            byte[] bytes = await Task.Run(() => File.Exists(cache_path) ? File.ReadAllBytes(cache_path) : null);
            sprite = CreateSprite(bytes);
            string separator = pUrl.Contains("?") ? "&" : "?";
            foreach (string url in new[] { pUrl + separator + small_size_query, pUrl })
            {
                if (sprite != null) break;
                byte[] downloaded = await DownloadAsync(url);
                sprite = CreateSprite(downloaded);
                if (sprite != null) await Task.Run(() => WriteCache(cache_path, downloaded));
            }
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot load the Workshop preview {pUrl}: {e.Message}");
        }

        List<Action<Sprite>> callbacks = waiting[pKey];
        waiting.Remove(pKey);
        if (sprite == null) return;

        Store(pKey, sprite);
        foreach (Action<Sprite> callback in callbacks) callback(sprite);
    }

    private static async Task<byte[]> DownloadAsync(string pUrl)
    {
        await download_slots.WaitAsync();
        try
        {
            using HttpResponseMessage response = await GetClient().GetAsync(pUrl);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > max_image_bytes) return null;

            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes.Length > max_image_bytes ? null : bytes;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Workshop preview download failed ({e.GetBaseException().Message}): {pUrl}");
            return null;
        }
        finally
        {
            download_slots.Release();
        }
    }

    private static HttpClient GetClient()
    {
        if (http_client != null) return http_client;
        // TLS 1.2, as in HttpUtils.Request.
        ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
        http_client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        return http_client;
    }

    private static Sprite CreateSprite(byte[] pBytes)
    {
        if (pBytes == null) return null;

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(pBytes))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static void WriteCache(string pPath, byte[] pBytes)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pPath)!);
            File.WriteAllBytes(pPath, pBytes);
        }
        catch (Exception e)
        {
            LogService.LogWarningConcurrent($"Cannot cache a Workshop preview at {pPath}: {e.Message}");
        }
    }

    /// <summary>
    ///     Keep at most <see cref="memory_limit" /> sprites, dropping the oldest. A row still showing a dropped
    ///     sprite requests it again the next time the list refreshes.
    /// </summary>
    private static void Store(string pKey, Sprite pSprite)
    {
        sprites[pKey] = pSprite;
        load_order.Enqueue(pKey);
        int inspected = 0;
        while (load_order.Count > memory_limit && inspected < load_order.Count)
        {
            string oldest = load_order.Dequeue();
            if (!sprites.TryGetValue(oldest, out Sprite oldest_sprite))
            {
                continue;
            }

            if (owners_by_key.TryGetValue(oldest, out HashSet<object> owners) && owners.Count > 0)
            {
                load_order.Enqueue(oldest);
                inspected++;
                continue;
            }

            UnityEngine.Object.Destroy(oldest_sprite.texture);
            UnityEngine.Object.Destroy(oldest_sprite);
            sprites.Remove(oldest);
            owners_by_key.Remove(oldest);
            inspected = 0;
        }
    }

    /// <summary>
    ///     A file name that is the same in every session (string.GetHashCode is not), so a changed preview URL
    ///     gets a new cache file.
    /// </summary>
    private static string UrlKey(string pUrl)
    {
        ulong hash = 14695981039346656037;
        foreach (char c in pUrl)
        {
            hash ^= c;
            hash *= 1099511628211;
        }

        return hash.ToString("x16");
    }
    
    private static void AssignOwner(object pOwner, string pKey)
    {
        if (key_by_owner.TryGetValue(pOwner, out string current))
        {
            if (current == pKey) return;

            if (owners_by_key.TryGetValue(current, out HashSet<object> current_owners))
            {
                current_owners.Remove(pOwner);
            }
        }

        if (pKey == null)
        {
            key_by_owner.Remove(pOwner);
            return;
        }

        key_by_owner[pOwner] = pKey;
        if (!owners_by_key.TryGetValue(pKey, out HashSet<object> owners))
        {
            owners = new HashSet<object>();
            owners_by_key[pKey] = owners;
        }

        owners.Add(pOwner);
    }
}
