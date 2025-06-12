using NeoModLoader.utils.SerializedAssets;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using strings;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Method to get Additional custom stats depending on the actor
    /// </summary>
    public delegate BaseStats GetAdditionalBaseStatsMethod(Actor Actor);
    /// <summary>
    /// A Builder for creating Actor Traits
    /// </summary>
    public sealed class ActorTraitBuilder : BaseTraitBuilder<ActorTrait, ActorTraitLibrary>
    {
        internal static ConcurrentDictionary<string, GetAdditionalBaseStatsMethod> AdditionalBaseStatMethods = new();
        /// <inheritdoc/>
        public ActorTraitBuilder(string ID) : base(ID)
        {
            Group = S_TraitGroup.miscellaneous;
        }
        /// <inheritdoc/>
        protected override void LoadFromPath(string FilePathToBuild)
        {
            SerializedActorTrait assetSerialized = JsonConvert.DeserializeObject<SerializedActorTrait>(File.ReadAllText(FilePathToBuild));
            Asset = SerializedActorTrait.ToAsset(assetSerialized);
        }
        /// <inheritdoc/>
        public ActorTraitBuilder(string ID, bool LoadImmediately) : base(ID, LoadImmediately) { }
        /// <inheritdoc/>
        public ActorTraitBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        void LinkWithLibrary()
        {
            if (Asset.combat)
            {
                Library.pot_traits_combat.Add(Asset);
            }
            if (Asset.is_mutation_box_allowed)
            {
                Library.pot_traits_mutation_box.Add(Asset);
            }
            if (Asset.rate_acquire_grow_up != 0)
            {
                for (int j = 0; j < Asset.rate_acquire_grow_up; j++)
                {
                    Library.pot_traits_growup.Add(Asset);
                }
            }
            if (Asset.rate_birth != 0)
            {
                for (int i = 0; i < Asset.rate_birth; i++)
                {
                    Library.pot_traits_birth.Add(Asset);
                }
            }
        }
        /// <inheritdoc/>
        public override void Build(bool SetRarityAutomatically = false, bool AutoLocalize = true, bool LinkWithOtherAssets = false)
        {
            base.Build(SetRarityAutomatically, AutoLocalize, LinkWithOtherAssets);
            LinkWithLibrary();
            Library.checkDefault(Asset);
            Asset.only_active_on_era_flag = Asset.era_active_moon || Asset.era_active_night;
        }
        /// <summary>
        /// (Optional) creates a method which gives custom stats to a actor who has this trait
        /// </summary>
        public GetAdditionalBaseStatsMethod AdditionalBaseStatsMethod { set
            {
                if (!AdditionalBaseStatMethods.TryAdd(Asset.id, value))
                {
                    AdditionalBaseStatMethods[Asset.id] = value;
                }
            }
        }
        /// <summary>
        /// if true, actors cannot have this trait if they have the strong minded trait
        /// </summary>
        public bool AffectsMind { get {  return Asset.affects_mind; } set { Asset.affects_mind = value; } }
        /// <summary>
        /// if true, actors can cure this trait and remove it
        /// </summary>
        public bool CanBeCured { get { return Asset.can_be_cured; } set { Asset.can_be_cured = value; } }
        /// <summary>
        /// when actors with the subspecies trait accelerated healing age, they can remove traits which have this set to true
        /// </summary>
        public bool RemovedByAcceleratedHealing { get { return Asset.can_be_removed_by_accelerated_healing; } set { Asset.can_be_removed_by_accelerated_healing = value; } }
        /// <summary>
        /// if true, Divine Light can remove this trait from actors
        /// </summary>
        public bool RemovedByDevineLight { get { return Asset.can_be_removed_by_accelerated_healing; } set { Asset.can_be_removed_by_divine_light = value; } }
        /// <summary>
        /// if true, this actor trait represents a combat skill, and actors could try to gain this trait
        /// </summary>
        public bool IsCombatSkill { get { return Asset.combat; } set { Asset.combat = value; } }
        /// <summary>
        /// if true, the base stats of this trait will only be applied to the actor in the age of dark
        /// </summary>
        public bool ActiveInDarkEra { get { return Asset.era_active_night; } set {  Asset.era_active_night = value;} }
        /// <summary>
        /// if true, the base stats of this trait will only be applied to the actor in the age of moon
        /// </summary>
        public bool ActiveInMoonEra { get { return Asset.era_active_moon; } set { Asset.era_active_moon = value; } }
        /// <summary>
        /// The ID of a WILD kingdom (Default, Non Civ) that this trait forces the actor to, the trait must add the forcedkingdomadd effect to its ActionOnAdd, you should also add it to ActionOnLoad
        /// </summary>
        public string ForcedKingdomID { get { return Asset.forced_kingdom; } set { Asset.forced_kingdom = value; } }
        /// <summary>
        /// if true, actors can get this trait from mutation box
        /// </summary>
        public bool UsedInMutationBox { get { return Asset.is_mutation_box_allowed; } set { Asset.is_mutation_box_allowed = value; } }
        /// <summary>
        /// when actors try to make best friends, this goes into account, with increased likeability increasing the chance they become friends
        /// </summary>
        public float ActorsLikeability { get { return Asset.likeability; } set { Asset.likeability = value; } }
        /// <summary>
        /// used for when actors try to make best friends, relations between kings, and kings with city leaders, if the other actor has a trait which is an opposite of this trait, the likeability factor is increased by this devided by 100
        /// </summary>
        public int OppositeTraitLikeability { get { return Asset.opposite_trait_mod; } set { Asset.opposite_trait_mod = value; } }
        /// <summary>
        /// When an actor turns into an adult, he can get new traits, the chance of getting this trait is determined by its rate
        /// </summary>
        public int RateAcquireWhenGrownUp { get { return Asset.rate_acquire_grow_up; } set { Asset.rate_acquire_grow_up = value; } }
        /// <summary>
        /// the chance of a child inheriting this trait from a parent, also if it is above 0 cloned actors will get this trait from their original
        /// </summary>
        public int RateBirth { get { return Asset.rate_birth; } set { Asset.rate_birth = value; } }
        /// <summary>
        /// same thing as rate birth, if this is 0 it will become rate inherit * 10
        /// </summary>
        public int RateInherit { get { return Asset.rate_inherit; } set { Asset.rate_inherit = value; } }
        /// <summary>
        /// When creating the zombie varient of a actor asset, the game removes any default traits of the actor asset which have this enabled
        /// </summary>
        public bool RemoveForZombies { get { return Asset.remove_for_zombie_actor_asset; } set { Asset.remove_for_zombie_actor_asset = value; } }
        /// <summary>
        /// basically the same as OppositeTraitLikeability but applied when both actors have this trait
        /// </summary>
        public int SameTraitLikeability { get { return Asset.same_trait_mod; } set { Asset.same_trait_mod = value; } }
        /// <summary>
        /// doesnt do anything
        /// </summary>
        public TraitType Type { get { return Asset.type; } set { Asset.type = value; } }
    }
}