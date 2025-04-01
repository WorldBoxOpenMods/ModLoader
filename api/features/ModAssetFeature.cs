namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModObjectFeature{TObject}"/> that has the primary purpose to creating a specific asset controlled by WorldBoxes <see cref="AssetManager"/>.
/// </summary>
/// <typeparam name="TAsset">The <see cref="Asset"/> type that the feature is meant to produce.</typeparam>
public abstract class ModAssetFeature<TAsset> : ModObjectFeature<TAsset> where TAsset : Asset
{
    /// <summary>
    /// Whether the asset should be added to its according <see cref="AssetManager"/> library automatically. Default is true.
    /// </summary>
    protected virtual bool AddToLibrary => true;

    /// <summary>
    /// Does the same as <see cref="ModObjectFeature{TObject}.Init"/>, but also adds the asset to its according <see cref="AssetManager"/> library if <see cref="AddToLibrary"/> is set to true.
    /// </summary>
    /// <returns>Whether the asset was created successfully.</returns>
    /// <exception cref="FeatureLoadException">The <see cref="TAsset"/> couldn't be added to its relevant <see cref="AssetManager"/> library, or no such library was found.</exception>
    public override bool Init()
    {
        if (!base.Init()) return false;
        if (AddToLibrary)
        {
            var library = AssetManager._instance._list.OfType<AssetLibrary<TAsset>>().FirstOrDefault();
            if (library == null) throw new FeatureLoadException($"No library found for {typeof(TAsset).Name}");
            library.add(Object);
        }
        return true;
    }
}