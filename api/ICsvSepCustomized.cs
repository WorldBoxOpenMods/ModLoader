namespace NeoModLoader.api;

/// <summary>
///     Customize the separator of the automatically loaded CSV file(for localization)
/// </summary>
public interface ICsvSepCustomized
{
    /// <summary>
    ///     Get the separator of the automatically loaded CSV file(for localization)
    /// </summary>
    /// <returns></returns>
    public char GetCsvSeparator();
}