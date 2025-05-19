namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// used for when building a subspecies trait
    /// </summary>
    public enum SubSpeciesTrait
    {
        /// <summary>
        /// A Normal Subspecies trait
        /// </summary>
        Trait,
        /// <summary>
        /// this subspecies trait is a phenotype, linking it with a phenotype asset
        /// </summary>
        PhenoType,
        /// <summary>
        /// this subspecies trait is an egg, it controls the eggs produced from this subspecies (their shape, colors, hatch time)
        /// </summary>
        Egg,
        /// <summary>
        /// this trait is a skin mutation
        /// </summary>
        SkinMutation
    }
    /// <summary>
    /// a builder which creates subspecies traits
    /// </summary>
    public sealed class SubspeciesTraitBuilder : BaseTraitBuilder<SubspeciesTrait, SubspeciesTraitLibrary>
    {
        static string TraitToDerive(SubSpeciesTrait trait) => trait switch
        {
            SubSpeciesTrait.Trait => null,
            SubSpeciesTrait.Egg => SubspeciesTraitLibrary.TEMPLATE_EGG,
            SubSpeciesTrait.SkinMutation => SubspeciesTraitLibrary.TEMPLATE_SKIN_MUTATION,
            _ => null
        };
        /// <summary>
        /// creates a subspecies trait (type egg) with a afterhatchfromeggaction
        /// </summary>
        /// <remarks>
        /// the afterhatchfromeggaction is performed when actors from this subspecies hatch from an egg
        /// </remarks>
        public SubspeciesTraitBuilder(string ID, AfterHatchFromEggAction afterHatchFromEggAction) : this(ID, SubSpeciesTrait.Egg)
        {
            Asset.after_hatch_from_egg_action = afterHatchFromEggAction;
        }
        /// <summary>
        /// creates a subspecies trait (type skin mutation), the overridepath species the path to the sprites which overrides the normal sprites
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="OverridePath">an example would be actors/species/mutations/mutation_skin_light_orb</param>
        /// <param name="RenderChildHeads">any actors in this subspecies who are babys will not render their head if false</param>
        public SubspeciesTraitBuilder(string ID, string OverridePath, bool RenderChildHeads) : this(ID, SubSpeciesTrait.SkinMutation)
        {
            Asset.render_heads_for_children = RenderChildHeads;
            Asset.sprite_path = OverridePath;
        }
        /// <summary>
        /// builds an asset depending on the Type
        /// </summary>
        public SubspeciesTraitBuilder(string ID, SubSpeciesTrait Type) : base(ID, TraitToDerive(Type)) {
            if (Type == SubSpeciesTrait.PhenoType)
            {
                UsesSpecialIconLogic = true;
                PathIcon = "ui/Icons/iconPhenotype";
                Asset.id = "phenotype_skin" + "_" + ID;
                Asset.phenotype_skin = true;
                Asset.id_phenotype = ID;
                Asset.group_id = "phenotypes";
                NameID = "subspecies_trait_phenotype";
                Description1ID = ("subspecies_trait_phenotype_info");
                Asset.spawn_random_trait_allowed = false;
            }
            else if (Type == SubSpeciesTrait.Egg)
            {
                Asset.id_egg = Asset.id;
                Asset.sprite_path = "eggs/" + Asset.id_egg;
            }
        }
        void LinkWithLibrary()
        {
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.Add(Asset);
            }
            if (Asset.in_mutation_pot_add)
            {
                int tRate = Asset.rarity.GetRate();
                Library._pot_mutation_traits_add.AddTimes(tRate, Asset);
            }
            if (Asset.in_mutation_pot_remove)
            {
                int tRate2 = Asset.rarity.GetRate();
                Library._pot_mutation_traits_remove.AddTimes(tRate2, Asset);
            }
            if (Asset.phenotype_egg && Asset.after_hatch_from_egg_action != null)
            {
                Asset.has_after_hatch_from_egg_action = true;
            }
        }
        /// <inheritdoc/>
        public override void Build(bool SetRarityAutomatically = false, bool AutoLocalize = true, bool LinkWithOtherAssets = false)
        {
            base.Build(SetRarityAutomatically, AutoLocalize, LinkWithOtherAssets);
            Library.loadSpritesPaths(Asset);
            LinkWithLibrary();
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            if (Asset.id_phenotype != null)
            {
                PhenotypeAsset Phenotype = AssetManager.phenotype_library.get(Asset.id_phenotype);
                Phenotype.subspecies_trait_id = Asset.id;
                Asset.priority = Phenotype.priority;
            }
            if (Asset.is_mutation_skin)
            {
                OpposeAllOtherTraits = new[] { (SubspeciesTrait trait) => trait.is_mutation_skin };
            }
            if (Asset.phenotype_skin)
            {
                OpposeAllOtherTraits = new[] { (SubspeciesTrait trait) => trait.phenotype_skin };
            }
            if (Asset.phenotype_egg)
            {
                OpposeAllOtherTraits = new[] { (SubspeciesTrait trait) => trait.phenotype_egg };
            }
            base.LinkAssets();
            
        }
        /// <inheritdoc/>
        public SubspeciesTraitBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public SubspeciesTraitBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// Used for phenotypes if true the icon will not use the path icon, instead it will use the phenotype color attached to the subspecies trait
        /// </summary>
        public bool UsesSpecialIconLogic { get { return Asset.special_icon_logic; } set { Asset.special_icon_logic = value; } }
        /// <summary>
        /// the file names used for actors in this subspecies's swim animations, along with its speed (default 10)
        /// </summary>
        /// <remarks>
        /// use ActorAnimationSequences as it already has lists of these for you
        /// </remarks>
        public ValueTuple<string[], float> SwimAnimation { get { return new(Asset.animation_swim, Asset.animation_swim_speed); } set { Asset.animation_swim = value.Item1; Asset.animation_swim_speed = value.Item2; } }
        /// <summary>
        /// the file names used for actors in this subspecies's walk animations, along with its speed (default 10)
        /// </summary>
        /// <remarks>
        /// use ActorAnimationSequences as it already has lists of these for you
        /// </remarks>
        public ValueTuple<string[], float> WalkAnimation { get { return new(Asset.animation_walk, Asset.animation_walk_speed); } set { Asset.animation_walk = value.Item1; Asset.animation_walk_speed = value.Item2; } }
        /// <summary>
        /// the file names used for actors in this subspecies's idle animations, along with its speed (default 10)
        /// </summary>
        /// <remarks>
        /// use ActorAnimationSequences as it already has lists of these for you
        /// </remarks>
        public ValueTuple<string[], float> IdleAnimation { get { return new(Asset.animation_idle, Asset.animation_idle_speed); } set { Asset.animation_idle = value.Item1; Asset.animation_idle_speed = value.Item2; } }
        /// <summary>
        /// if true, when subspecies's mutate, they can get this triat, the less its rarity the higher the chance
        /// </summary>
        public bool CanBeAddedFromMutations { get { return Asset.in_mutation_pot_add; } set { Asset.in_mutation_pot_add = value; } }
        /// <summary>
        /// if true, when subspecies's mutate, they can get this triat removed, the less its rarity the higher the chance
        /// </summary>
        public bool CanbeRemovedFromMutations { get { return Asset.in_mutation_pot_remove; } set { Asset.in_mutation_pot_remove = value; } }
        /// <summary>
        /// used if the trait is a skin mutation, this chooses the female skins used
        /// </summary>
        /// <remarks>names used are like "female_1" for example</remarks>
        public List<string> FemaleSkins { get { return Asset.skin_citizen_female; } set { Asset.skin_citizen_female = value; } }
        /// <summary>
        /// used if the trait is a skin mutation, this chooses the male skins used
        /// </summary>
        /// <remarks>names used are like "male_1" for example</remarks>
        public List<string> MaleSkins { get { return Asset.skin_citizen_male; } set { Asset.skin_citizen_male = value; } }
        /// <summary>
        /// used if the trait is a skin mutation, this chooses the warrior skins used
        /// </summary>
        /// <remarks>names used are like "warrior_1" for example</remarks>
        public List<string> WarriorSkins { get { return Asset.skin_warrior; } set { Asset.skin_warrior = value; } }
        /// <summary>
        /// if true, any resources whose diet list has a tag apart of this assets meta tags, any subspecies's with this trait will consume that resource
        /// </summary>
        public bool DietRelated { get { return Asset.is_diet_related; } set { Asset.is_diet_related = value; } }
        /// <summary>
        /// when a zombie subspecies is created they will remove remove any traits with this set to true
        /// </summary>
        public bool RemoveIfZombieSubSpecies { get { return Asset.remove_for_zombies; } set { Asset.remove_for_zombies = value; } }
        /// <summary>
        /// if true, any actors in this subspecies will not rotate when unconscious
        /// </summary>
        public bool DontRotateWhenUnconscious { get { return Asset.prevent_unconscious_rotation; } set { Asset.prevent_unconscious_rotation = value; } }
    }
}