namespace NeoModLoader.api.features;

/// <summary>
/// A kind of <see cref="ModFeature"/> that has the primary purpose to creating a specific object of any type.
/// </summary>
/// <typeparam name="TObject">The type of object the feature is meant to produce.</typeparam>
public abstract class ModObjectFeature<TObject> : ModFeature
{
    /// <summary>
    /// The resulting object of the feature.
    /// </summary>
    public TObject Object { get; private set; }
    /// <summary>
    /// An implementation of <see cref="ModFeature.Init"/> that initializes the object. Please override <see cref="InitObject"/> for creating the object, and only override this method for behavioural adjustments unrelated to the object.
    /// </summary>
    /// <returns>Whether the object was successfully created. This is indicated by whether the object returned by <see cref="InitObject"/> is null.</returns>
    public override bool Init()
    {
        TObject obj = InitObject();
        if (obj == null) return false;
        Object = obj;
        return true;
    }
    /// <summary>
    /// The method that creates the object. This method is called by <see cref="Init"/> and should be overridden to create the object.
    /// </summary>
    /// <returns>The object, or null if object creation failed.</returns>
    protected abstract TObject InitObject();
    /// <summary>
    /// A cast for making it easier to access the object.
    /// </summary>
    /// <param name="feature">The feature that created the wanted object.</param>
    /// <returns>The wanted object.</returns>
    public static implicit operator TObject(ModObjectFeature<TObject> feature)
    {
        return feature.Object;
    }
}