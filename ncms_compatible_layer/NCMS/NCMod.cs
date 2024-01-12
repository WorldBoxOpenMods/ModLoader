#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS
{
    /// <remarks>
    ///     From [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class NCMod
    {
        public string author;

        public string description;

        public string iconPath;

        public string name;

        public string path;

        public int targetGameBuild = 444;

        public string version;
    }
}