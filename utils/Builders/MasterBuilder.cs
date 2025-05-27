namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A manager for your builders, meant so you can link all of your assets together
    /// </summary>
    public sealed class MasterBuilder
    {
        readonly List<Builder> Builders = new();
        /// <summary>
        /// Adds a builder
        /// </summary>
        public B AddBuilder<B>(B Builder) where B : Builder
        {
            Builders.Add(Builder);
            return Builder;
        }
        /// <summary>
        /// Adds's builders
        /// </summary>
        public void AddBuilders(IEnumerable<Builder> Builders)
        {
            if(Builders == null)
            {
                return;
            }
            this.Builders.AddRange(Builders);
        }
        /// <summary>
        /// Builds all of the builders and links their assets together
        /// </summary>
        public void BuildAll()
        {
            foreach (var builder in Builders)
            {
                builder.Build(false);
            }
            foreach (var builder in Builders)
            {
                builder.LinkAssets();
            }
        }
    }
}