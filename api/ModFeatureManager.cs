using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
namespace NeoModLoader.api;

/// <summary>
/// A standard implementation of <see cref="IModFeatureManager"/> that provides all required functionality for dynamically loading mod features.
/// </summary>
public class ModFeatureManager : IModFeatureManager
{
    private readonly List<IModFeature> _foundFeatures = new List<IModFeature>();
    private readonly List<IModFeature> _loadedFeatures = new List<IModFeature>();
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
    /// A method to initialize the <see cref="ModFeatureManager"/> and load all found features. This needs to be manually called in the mods init method.
    /// </summary>
    public void Init()
    {
        Init(Assembly.GetCallingAssembly());
    }

    private void Init(Assembly searchAssembly)
    {
        var features = new List<IModFeature>();
        foreach ((Type featureType, ConstructorInfo instanceConstructor) in searchAssembly.Modules.SelectMany(m => m.GetTypes()).Where(t => t.IsSubclassOf(typeof(IModFeature))).Where(ft => !ft.IsAbstract).Where(ft => !ft.IsNestedPrivate).Select(featureType => (featureType, featureType.GetConstructors().FirstOrDefault(constructor => constructor.GetParameters().Length < 1))))
        {
            Debug.Log($"Creating instance of Feature {featureType.FullName}...");
            if (instanceConstructor is null)
            {
                Debug.LogError($"No suitable constructor found for Feature {featureType.FullName}.");
                continue;
            }
            IModFeature instance;
            try
            {
                instance = instanceConstructor.Invoke(new object[] { }) as IModFeature;
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occurred while trying to create an instance of Feature {featureType.FullName}:\n{e}");
                continue;
            }
            if (instance is null)
            {
                Debug.LogError($"Failed to create instance of Feature {featureType.FullName} for unknown reasons.");
                continue;
            }
            instance.ModFeatureManager = this;
            if (instance.RequiredModFeatures.Any(requiredFeature => !requiredFeature.IsSubclassOf(typeof(IModFeature))))
            {
                throw new InvalidOperationException($"Feature {featureType.FullName} has a required feature that is not a subclass of IModFeature.");
            }
            if (instance.OptionalModFeatures.Any(optionalFeature => !optionalFeature.IsSubclassOf(typeof(IModFeature))))
            {
                throw new InvalidOperationException($"Feature {featureType.FullName} has an optional feature that is not a subclass of IModFeature.");
            }
            features.Add(instance);
            Debug.Log($"Successfully created instance of Feature {featureType.FullName}.");
        }
        _foundFeatures.AddRange(features);
        var featureTrees = FeatureTreeNode.CreateFeatureTrees(features.ToArray());
        FeatureLoadPathNode featureLoadPath = FeatureLoadPathNode.CreateFeatureLoadPath(featureTrees);
        FeatureLoadPathNode currentLoadPathNode = featureLoadPath;
        while (currentLoadPathNode != null)
        {
            InitFeature(currentLoadPathNode.ModFeature);
            currentLoadPathNode = currentLoadPathNode.DependentFeature;
        }
    }

    private void InitFeature(IModFeature modFeature)
    {
        Debug.Log($"Loading feature {modFeature.GetType().FullName}...");
        try
        {
            var missingRequirement = modFeature.RequiredModFeatures.Where(requiredFeature => !IsFeatureLoaded(requiredFeature)).ToList();
            if (missingRequirement.Count > 0)
            {
                Debug.LogError($"Loading feature {modFeature.GetType().FullName} failed due missing requirement features:\n{string.Join("\n", missingRequirement.Select(type => type.FullName))}");
                return;
            }
            bool successfulInit = modFeature.Init();
            if (!successfulInit)
            {
                Debug.LogError($"Loading feature {modFeature.GetType().FullName} failed due to a failing condition.");
                return;
            }
            Debug.Log($"Successfully loaded feature {modFeature.GetType().FullName}.");
            _loadedFeatures.Add(modFeature);
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while trying to load feature {modFeature.GetType().FullName}:\n{e}");
        }
    }
}