namespace NeoModLoader.api;

/// <summary>
/// A custom container used in <see cref="IModFeature"/> to store required features.
/// </summary>
public class ModFeatureRequirementList : IEnumerable<Type>
{
    private List<Type> RequiredFeatureList { get; } = new List<Type>();
    /// <summary>
    /// A constructor for creating a <see cref="ModFeatureRequirementList"/> based on one or more required types.
    /// </summary>
    /// <param name="types">The types required for the <see cref="IModFeature"/> this list belongs to.</param>
    /// <exception cref="ArgumentNullException">Thrown when a required feature type is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a required feature type is not a valid feature type.</exception>
    public ModFeatureRequirementList(params Type[] types)
    {
        foreach (Type type in types)
        {
            if (type is null) throw new ArgumentNullException(nameof(types), "A required feature type was null.");
            if (!typeof(IModFeature).IsAssignableFrom(type)) throw new ArgumentException($"The type {type.Name} is not a valid feature type.");
        }
        RequiredFeatureList.AddRange(types);
    }
    /// <summary>
    /// An operator overload for allowing an easy combination of requirements through inheritance structure by adding more types to base.RequiredModFeatures.
    /// </summary>
    /// <param name="list">The list that the new type should be concatenated with</param>
    /// <param name="type">The new type to add.</param>
    /// <returns>A new <see cref="ModFeatureRequirementList"/> with all requirements from the base list and the new requirement.</returns>
    public static ModFeatureRequirementList operator +(ModFeatureRequirementList list, Type type)
    {
        return list.RequiredFeatureList.Append(type).ToList();
    }
    /// <summary>
    /// A cast for making it possible to specify requirements with a normal list structure.
    /// </summary>
    /// <param name="list">A list of requirements.</param>
    /// <returns>A <see cref="ModFeatureRequirementList"/> with the specified types.</returns>
    public static implicit operator ModFeatureRequirementList(List<Type> list) => new ModFeatureRequirementList(list.ToArray());
    /// <summary>
    /// A cast for making it possible to specify requirements with a normal list structure.
    /// </summary>
    /// <param name="list">A <see cref="ModFeatureRequirementList"/></param>
    /// <returns>A list with the requirements of the <see cref="ModFeatureRequirementList"/>.</returns>
    public static implicit operator List<Type>(ModFeatureRequirementList list) => list.RequiredFeatureList.ToList();
    /// <summary>
    /// A cast for making it convenient to specify requirements with a single type.
    /// </summary>
    /// <param name="type">The type that should be a requirement.</param>
    /// <returns>A <see cref="ModFeatureRequirementList"/> with the specified type as a single entry.</returns>
    public static implicit operator ModFeatureRequirementList(Type type) => new ModFeatureRequirementList(type);
    /// <summary>
    /// A cast for making it possible to specify requirements with a normal array structure.
    /// </summary>
    /// <param name="list">An array of requirements.</param>
    /// <returns></returns>
    public static implicit operator ModFeatureRequirementList(Type[] list) => new ModFeatureRequirementList(list);
    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator() => RequiredFeatureList.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}