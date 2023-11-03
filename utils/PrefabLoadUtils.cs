using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using life.taxi;
using NeoModLoader.api.attributes;
using NeoModLoader.services;
using Tayx.Graphy.Audio;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils
{
    [Obsolete("Use AssetBundle instead")]
    public static class PrefabLoadUtils
    {
        private static readonly IDeserializer _deserializer =
            new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

        private static readonly Dictionary<int, Type> _types = new()
        {
            { 0, typeof(Object) },
            { 1, typeof(GameObject) },
            { 2, typeof(Component) },
            { 4, typeof(Transform) },
            { 8, typeof(Behaviour) },
            { 9, typeof(GameManager) },
            { 20, typeof(Camera) },
            { 21, typeof(Material) },
            { 23, typeof(MeshRenderer) },
            { 25, typeof(Renderer) },
            { 27, typeof(Texture) },
            { 28, typeof(Texture2D) },
            { 30, typeof(GraphicsSettings) },
            { 33, typeof(MeshFilter) },
            { 41, typeof(OcclusionPortal) },
            { 43, typeof(Mesh) },
            { 45, typeof(Skybox) },
            { 47, typeof(QualitySettings) },
            { 48, typeof(Shader) },
            { 49, typeof(TextAsset) },
            { 72, typeof(ComputeShader) },
            { 84, typeof(RenderTexture) },
            { 86, typeof(CustomRenderTexture) },
            { 89, typeof(Cubemap) },
            { 96, typeof(TrailRenderer) },
            { 102, typeof(TextMesh) },
            { 104, typeof(RenderSettings) },
            { 108, typeof(Light) },
            { 114, typeof(MonoBehaviour) },
            { 117, typeof(Texture3D) },
            { 119, typeof(Projector) },
            { 120, typeof(LineRenderer) },
            { 121, typeof(Flare) },
            { 123, typeof(LensFlare) },
            { 124, typeof(FlareLayer) },
            { 128, typeof(Font) },
            { 137, typeof(SkinnedMeshRenderer) },
            { 142, typeof(AssetBundle) },
            { 147, typeof(ResourceManager) },
            { 157, typeof(LightmapSettings) },
            { 171, typeof(SparseTexture) },
            { 183, typeof(Cloth) },
            { 187, typeof(Texture2DArray) },
            { 188, typeof(CubemapArray) },
            { 192, typeof(OcclusionArea) },
            { 200, typeof(ShaderVariantCollection) },
            { 205, typeof(LODGroup) },
            { 210, typeof(SortingGroup) },
            { 212, typeof(SpriteRenderer) },
            { 213, typeof(Sprite) },
            { 215, typeof(ReflectionProbe) },
            { 220, typeof(LightProbeGroup) },
            { 222, typeof(CanvasRenderer) },
            { 223, typeof(Canvas) },
            { 224, typeof(RectTransform) },
            { 225, typeof(CanvasGroup) },
            { 226, typeof(BillboardAsset) },
            { 227, typeof(BillboardRenderer) },
            { 258, typeof(LightProbes) },
            { 259, typeof(LightProbeProxyVolume) },
            { 290, typeof(AssetBundleManifest) },
            { 1006, typeof(TextureImporter) },
            { 100000, typeof(int) },
            { 100001, typeof(bool) },
            { 100002, typeof(float) },
            { 100011, typeof(void) },
            { 687078895, typeof(SpriteAtlas) },
            { 825902497, typeof(RayTracingShader) },
            { 850595691, typeof(LightingSettings) },
            { 2083778819, typeof(LocalizationAsset) }
        };

        private static readonly Regex _object_regex = new(@"--- !u!(\d+) &(\d+)");

        [Experimental]
        public static GameObject LoadPrefab(string path)
        {
            GameObject prefab;
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string fileText = File.ReadAllText(path);

            if (!fileText.StartsWith("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"))
            {
                LogService.LogError($"File {path} is not a valid Unity YAML file.");
                return null;
            }

            int curr_idx = fileText.IndexOf("---");
            int next_idx = fileText.IndexOf("---", curr_idx + 1);
            try
            {
                Dictionary<string, Object> file_objects = new();
                while (curr_idx != fileText.Length)
                {
                    string object_string = fileText.Substring(curr_idx, next_idx - curr_idx);
                    string header = object_string.Substring(0, object_string.IndexOf('\n'));
                    string content = object_string.Substring(object_string.IndexOf('\n') + 1);

                    var match = _object_regex.Match(header);
                    int type_id = int.Parse(match.Groups[1].Value);
                    string file_id = match.Groups[2].Value;

                    Type type;
                    if (_types.ContainsKey(type_id))
                    {
                        type = _types[type_id];
                    }
                    else
                    { 
                        LogService.LogWarning($"Unknown type id {type_id} in file {path} for fileID {file_id}.");
                        LogService.LogInfo("Try to solve it...");

                        string type_name = content.Substring(0, content.IndexOf('\n') - 1);
                        type = Type.GetType(type_name);
                        if (type == null)
                        {
                            LogService.LogError($"Cannot find type {type_name} in file {path} for fileID {file_id}.");
                            return null;
                        }
                    }
                    Object result = (Object)_deserializer.Deserialize(content, type);
                    file_objects.Add(file_id, result);

                    curr_idx = next_idx;
                    next_idx = fileText.IndexOf("---", curr_idx + 1);
                    if (next_idx == -1)
                    {
                        next_idx = fileText.Length;
                    }
                }
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
                return null;
            }
            return null;
        }
    }
}
