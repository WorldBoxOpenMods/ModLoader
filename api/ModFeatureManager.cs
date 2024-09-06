using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
namespace NeoModLoader.api;

/// <summary>
/// A standard implementation of <see cref="IModFeatureManager"/> that provides all required functionality for dynamically loading mod features.
/// </summary>
public class ModFeatureManager<TMod> : IModFeatureManager where TMod : BasicMod<TMod>
{
    private readonly BasicMod<TMod> _mod;
    private readonly List<IModFeature> _foundFeatures = new List<IModFeature>();
    private FeatureLoadPathNode _featureLoadPath;
    private StackTrace _firstInstantiationStackTrace;
    private readonly List<IModFeature> _loadedFeatures = new List<IModFeature>();
    private StackTrace _firstLoadStackTrace;

    /// <summary>
    /// A constructor for the <see cref="ModFeatureManager{TMod}"/>.
    /// </summary>
    /// <param name="mod"></param>
    public ModFeatureManager(BasicMod<TMod> mod)
    {
        _mod = mod;
    }

    /// <inheritdoc cref="IModFeatureManager.IsFeatureLoaded{T}"/>
    public bool IsFeatureLoaded<T>() where T : IModFeature
    {
        return IsFeatureLoaded(typeof(T));
    }

    private bool IsFeatureLoaded(Type featureType)
    {
        return _loadedFeatures.Any(feature => feature.GetType() == featureType);
    }

    /// <summary>
    /// A method to get a feature of a specific type.
    /// </summary>
    /// <param name="askingModFeature">The feature that is asking for this information.</param>
    /// <typeparam name="T">The type that should be checked for.</typeparam>
    /// <returns>An instance of the requested mod feature.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the feature requesting the feature does not have the requested feature set as a requirement.</exception>
    public T GetFeature<T>(IModFeature askingModFeature) where T : IModFeature
    {
        if (!askingModFeature.RequiredModFeatures.Contains(typeof(T))) throw new InvalidOperationException($"Feature {typeof(T).FullName} is not set as a requirement for feature {askingModFeature.GetType().FullName}.");
        if (!IsFeatureLoaded<T>()) throw new InvalidOperationException($"Feature {typeof(T).FullName} is not loaded.");
        return (T)GetFeature(typeof(T));
    }

    private IModFeature GetFeature(Type featureType)
    {
        return _foundFeatures.FirstOrDefault(feature => feature.GetType() == featureType);
    }

    /// <summary>
    /// A method to check if a feature of a specific type is loaded and get it if it is.
    /// </summary>
    /// <param name="askingModFeature">The feature that is asking for this information.</param>
    /// <param name="feature">The variable that the feature gets stored into if it's loaded.</param>
    /// <typeparam name="T">The type that should be checked for.</typeparam>
    /// <returns>Whether a feature of the specific type is loaded.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the feature requesting the feature does not have the requested feature set as a requirement or optional feature.</exception>
    public bool TryGetFeature<T>(IModFeature askingModFeature, out T feature) where T : IModFeature
    {
        if (!askingModFeature.RequiredModFeatures.Contains(typeof(T)) && !askingModFeature.OptionalModFeatures.Contains(typeof(T)))
        {
            throw new InvalidOperationException($"Feature {typeof(T).FullName} is not set as a requirement or optional feature for feature {askingModFeature.GetType().FullName}.");
        }
        if (!IsFeatureLoaded<T>())
        {
            feature = default;
            return false;
        }
        feature = (T)GetFeature(typeof(T));
        return true;
    }

    private class FeatureTreeNode
    {
        internal IModFeature ModFeature { get; }
        internal List<FeatureTreeNode> DependentFeatures { get; } = new List<FeatureTreeNode>();
        internal FeatureTreeNode(IModFeature modFeature)
        {
            ModFeature = modFeature;
        }
        internal static FeatureTreeNode[] CreateFeatureTrees(IModFeature[] features)
        {
            var featureNodes = new Dictionary<string, FeatureTreeNode>();
            var roots = new List<FeatureTreeNode>();
            foreach (IModFeature feature in features)
            {
                FeatureTreeNode featureTreeNode = new FeatureTreeNode(feature);
                featureNodes.Add(feature.GetType().AssemblyQualifiedName ?? throw new Exception("AssemblyQualifiedName is null, apparently."), featureTreeNode);
                if (!feature.RequiredModFeatures.Concat(feature.OptionalModFeatures).Any())
                {
                    roots.Add(featureTreeNode);
                }
            }
            foreach (FeatureTreeNode node in featureNodes.Values)
            {
                foreach (Type requirement in node.ModFeature.RequiredModFeatures.Concat(node.ModFeature.OptionalModFeatures))
                {
                    if (featureNodes.TryGetValue(requirement.AssemblyQualifiedName ?? throw new Exception("AssemblyQualifiedName is null, apparently."), out FeatureTreeNode requiredNode))
                    {
                        requiredNode.DependentFeatures.Add(node);
                    }
                }
            }
            return roots.ToArray();
        }
    }

    private class FeatureLoadPathNode
    {
        private class PlaceholderRootModFeature : ModFeature
        {
            public override bool Init()
            {
                return true;
            }
        }
        internal IModFeature ModFeature { get; }
        internal FeatureLoadPathNode DependentFeature { get; private set; }
        internal FeatureLoadPathNode DependencyFeature { get; private set; }
        internal FeatureLoadPathNode(IModFeature modFeature)
        {
            ModFeature = modFeature;
        }
        [CanBeNull]
        internal static FeatureLoadPathNode CreateFeatureLoadPath(FeatureTreeNode[] featureTrees)
        {
            FeatureTreeNode rootTreeNode = new FeatureTreeNode(new PlaceholderRootModFeature());
            foreach (FeatureTreeNode featureTree in featureTrees)
            {
                rootTreeNode.DependentFeatures.Add(featureTree);
            }
            FeatureLoadPathNode rootLoadPathNode = new FeatureLoadPathNode(rootTreeNode.ModFeature);
            FeatureLoadPathNode newestLoadPathNode = rootLoadPathNode;
            var nodesToProcess = new List<FeatureTreeNode>(rootTreeNode.DependentFeatures);
            while (nodesToProcess.Count > 0)
            {
                FeatureTreeNode treeNode = nodesToProcess.Pop();
                FeatureLoadPathNode currentLoadPathNode = newestLoadPathNode;
                while (currentLoadPathNode != null)
                {
                    if (currentLoadPathNode.ModFeature == treeNode.ModFeature)
                    {
                        if (currentLoadPathNode.DependentFeature != null)
                        {
                            currentLoadPathNode.DependentFeature.DependencyFeature = currentLoadPathNode.DependencyFeature;
                        }
                        if (currentLoadPathNode.DependencyFeature != null)
                        {
                            currentLoadPathNode.DependencyFeature.DependentFeature = currentLoadPathNode.DependentFeature;
                        }
                    }
                    currentLoadPathNode = currentLoadPathNode.DependencyFeature;
                }
                FeatureLoadPathNode newLoadPathNode = new FeatureLoadPathNode(treeNode.ModFeature);
                newestLoadPathNode.DependentFeature = newLoadPathNode;
                newLoadPathNode.DependencyFeature = newestLoadPathNode;
                newestLoadPathNode = newLoadPathNode;
                nodesToProcess.AddRange(treeNode.DependentFeatures);
            }
            return rootLoadPathNode.DependentFeature;
        }
    }

    /// <summary>
    /// A method to initialize the <see cref="ModFeatureManager{TMod}"/> and load all found features.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the features have already been initialized.</exception>
    public void InstantiateFeatures()
    {
        if (_featureLoadPath != null)
        {
            throw new InvalidOperationException($"Features have already been instantiated for this ModFeatureManager. Stack trace of first instantiation:\n{_firstInstantiationStackTrace}");
        }
        var features = FindAndInstantiateModFeatures();
        _featureLoadPath = ParseModFeaturesIntoLoadPath(features);
        if (_foundFeatures.Count > 0)
        {
            _firstInstantiationStackTrace = new StackTrace();
        }
    }
    /// <inheritdoc cref="IStagedLoad.Init"/>
    /// <exception cref="InvalidOperationException">Thrown if the features have already been initialized.</exception>
    public void Init()
    {
        if (_loadedFeatures.Count > 0)
        {
            throw new InvalidOperationException($"Features have already been loaded for this ModFeatureManager. Stack trace of first load:\n{_firstLoadStackTrace}");
        }
        FeatureLoadPathNode currentLoadPathNode = _featureLoadPath;
        while (currentLoadPathNode != null)
        {
            InitFeature(currentLoadPathNode.ModFeature);
            currentLoadPathNode = currentLoadPathNode.DependentFeature;
        }
        if (_loadedFeatures.Count > 0)
        {
            _firstLoadStackTrace = new StackTrace();
        }
    }
    /// <inheritdoc cref="IStagedLoad.PostInit"/>
    public void PostInit()
    {
        FeatureLoadPathNode currentLoadPathNode = _featureLoadPath;
        while (currentLoadPathNode != null)
        {
            SafePerformActionOnFeature(currentLoadPathNode.ModFeature, "Post-Loading", feature => feature.PostInit());
            currentLoadPathNode = currentLoadPathNode.DependentFeature;
        }
    }
    private static FeatureLoadPathNode ParseModFeaturesIntoLoadPath(List<IModFeature> features)
    {
        var featureTrees = FeatureTreeNode.CreateFeatureTrees(features.ToArray());
        FeatureLoadPathNode featureLoadPath = FeatureLoadPathNode.CreateFeatureLoadPath(featureTrees);
        return featureLoadPath;
    }
    private List<IModFeature> FindAndInstantiateModFeatures()
    {
        var features = new List<IModFeature>();
        foreach ((Type featureType, ConstructorInfo instanceConstructor) in _mod.GetType().Assembly.Modules.SelectMany(m => m.GetTypes()).Where(t => typeof(IModFeature).IsAssignableFrom(t)).Where(ft => !ft.IsAbstract).Where(ft => !ft.IsNestedPrivate).Select(featureType => (featureType, featureType.GetConstructors().FirstOrDefault(constructor => constructor.GetParameters().Length < 1))))
        {
            InstantiateModFeature(featureType, instanceConstructor, features);
        }
        _foundFeatures.AddRange(features);
        return features;
    }
    private void InstantiateModFeature(Type featureType, ConstructorInfo instanceConstructor, List<IModFeature> features)
    {
        BasicMod<TMod>.LogInfo($"Creating instance of Feature {featureType.FullName}...");
        if (instanceConstructor is null)
        {
            BasicMod<TMod>.LogError($"No suitable constructor found for Feature {featureType.FullName}.");
            return;
        }
        IModFeature instance;
        try
        {
            instance = instanceConstructor.Invoke(new object[] { }) as IModFeature;
        }
        catch (Exception e)
        {
            BasicMod<TMod>.LogError($"An error occurred while trying to create an instance of Feature {featureType.FullName}:\n{e}");
            return;
        }
        if (instance is null)
        {
            BasicMod<TMod>.LogError($"Failed to create instance of Feature {featureType.FullName} for unknown reasons.");
            return;
        }
        instance.ModFeatureManager = this;
        var invalidRequiredFeatures = instance.RequiredModFeatures.Where(requiredFeature => !typeof(IModFeature).IsAssignableFrom(requiredFeature)).ToList();
        if (invalidRequiredFeatures.Any())
        {
            throw new InvalidOperationException($"Feature {featureType.FullName} has required features that are not a subclass of IModFeature:\n{string.Join("\n", invalidRequiredFeatures.Select(type => type.FullName))}");
        }
        var invalidOptionalFeatures = instance.OptionalModFeatures.Where(optionalFeature => !typeof(IModFeature).IsAssignableFrom(optionalFeature)).ToList();
        if (invalidOptionalFeatures.Any())
        {
            throw new InvalidOperationException($"Feature {featureType.FullName} has optional features that are not a subclass of IModFeature:\n{string.Join("\n", invalidOptionalFeatures.Select(type => type.FullName))}");
        }
        features.Add(instance);
        BasicMod<TMod>.LogInfo($"Successfully created instance of Feature {featureType.FullName}.");
    }

    private void InitFeature(IModFeature modFeature)
    {
        SafePerformActionOnFeature(modFeature, "Loading", feature =>
        {
            bool successfulLoad = feature.Init();
            if (successfulLoad) _loadedFeatures.Add(modFeature);
            return successfulLoad;
        });
    }
    
    private void SafePerformActionOnFeature(IModFeature modFeature, string actionVerb, Func<IModFeature, bool> performAction, bool log = true)
    {
        if (log) BasicMod<TMod>.LogInfo($"{actionVerb} feature {modFeature.GetType().FullName}...");
        try
        {
            var missingRequirement = modFeature.RequiredModFeatures.Where(requiredFeature => !IsFeatureLoaded(requiredFeature)).ToList();
            if (missingRequirement.Count > 0)
            {
                if (log) BasicMod<TMod>.LogError($"{actionVerb} feature {modFeature.GetType().FullName} failed due missing requirement features:\n{string.Join("\n", missingRequirement.Select(type => type.FullName))}");
                return;
            }
            bool successfulPerformance = performAction(modFeature);
            if (!successfulPerformance)
            {
                if (log) BasicMod<TMod>.LogError($"{actionVerb} feature {modFeature.GetType().FullName} failed due to a failing condition.");
                return;
            }
            if (log) BasicMod<TMod>.LogInfo($"{actionVerb} feature {modFeature.GetType().FullName} succeeded.");
        }
        catch (Exception e)
        {
            if (log) BasicMod<TMod>.LogError($"{actionVerb} feature {modFeature.GetType().FullName} caused an error:\n{e}");
        }
    }
}