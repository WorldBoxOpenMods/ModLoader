using NeoModLoader.utils;

namespace NeoModLoader.services;

internal sealed class ModEnablePlan
{
    public HashSet<string> RequestedRoots { get; } = new();
    public HashSet<string> AutoEnabledNodes { get; } = new();
    public HashSet<string> PlannedEnabledNodes { get; } = new();
    public List<ModDependencyNode> LoadOrder { get; } = new();
    public Dictionary<string, bool> DesiredEnabledSnapshot { get; } = new();
    public string FailureReason { get; private set; } = string.Empty;

    public bool HasFailure => !string.IsNullOrWhiteSpace(FailureReason);

    public void SetFailure(string pFailureReason)
    {
        FailureReason = pFailureReason?.Trim() ?? string.Empty;
    }
}
