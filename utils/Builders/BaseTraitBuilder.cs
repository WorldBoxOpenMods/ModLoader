using FMOD;
using HarmonyLib;
using NeoModLoader.General;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Builder for Trait builders
    /// </summary>
    public class BaseTraitBuilder<A, AL> : AugmentationAssetBuilder<A, AL> where A : BaseTrait<A>, new() where AL : BaseTraitLibrary<A>
    {
        /// <inheritdoc/>
        public BaseTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public BaseTraitBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public BaseTraitBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }

        /// <inheritdoc/>
        protected override void Init()
        {
            Description1ID = null;
            Description2ID = null;
            NameID = (null);
        }
        /// <summary>
        /// the traits in the asset's list of opposite traits, if a object has one of these traits it cannot add this trait
        /// </summary>
        public IEnumerable<string> OppositeTraits
        {
            set
            {
                foreach (string trait in value)
                {
                    Asset.addOpposite(trait);
                }
            }
            get
            {
                return Asset.opposite_list;
            }
        }
        /// <summary>
        /// the tags in assets base stats meta
        /// </summary>
        public IEnumerable<string> MetaTags
        {
            get
            {
                return Asset.base_stats_meta._tags;
            }
            set
            {
                foreach (string tag in value)
                {
                    Asset.base_stats_meta.addTag(tag);
                }
            }
        }
        /// <summary>
        /// sets traits which this trait automatically removes when it is added to a object
        /// </summary>
        public IEnumerable<string> TraitsToRemove
        {
            set
            {
                Asset.traits_to_remove_ids = TraitsToRemove.ToArray();
            }
            get
            {
                return Asset.traits_to_remove_ids;
            }
        }
        /// <inheritdoc/>
        public override void Build(bool LinkWithOtherAssets)
        {
            Build(false, true, LinkWithOtherAssets);
        }
        /// <summary>
        /// all other traits in the library will be added to the opposites list if the ShouldOppose function returns true
        /// </summary>
        /// <param name="value">a function which has a trait as input and outputs true if added to opposites list</param>
        public IEnumerable<Func<A, bool>> OpposeAllOtherTraits { set {
                foreach(Func<A, bool> func in value)
                {
                    foreach (A asset in Library.list)
                    {
                        if (asset.id != Asset.id && func(asset))
                        {
                            Asset.addOpposite(asset.id);
                        }
                    }
                }
            } }
        void LinkWithActors()
        {
            foreach (ActorAsset tActorAsset in AssetManager.actor_library.list)
            {
                List<string> traits = Library.getDefaultTraitsForMeta(tActorAsset);
                if (traits != null && traits.Contains(Asset.id))
                {
                    Asset.default_for_actor_assets ??= new List<ActorAsset>();
                    Asset.default_for_actor_assets.Add(tActorAsset);
                }
            }
        }
        void LinkWithTraits()
        {
            if (Asset.opposite_list != null && Asset.opposite_list.Count > 0)
            {
                Asset.opposite_traits = new HashSet<A>(Asset.opposite_list.Count);
                foreach (string tID in Asset.opposite_list)
                {
                    A tOppositeTrait = Library.get(tID);
                    Asset.opposite_traits.Add(tOppositeTrait);
                }
            }
            if (Asset.traits_to_remove_ids != null)
            {
                int tCount = Asset.traits_to_remove_ids.Length;
                Asset.traits_to_remove = new A[tCount];
                for (int i = 0; i < tCount; i++)
                {
                    string ID = Asset.traits_to_remove_ids[i];
                    A tTraitToAdd = Library.get(ID);
                    Asset.traits_to_remove[i] = tTraitToAdd;
                }
            }
        }
        void CheckIcon()
        {
            if (string.IsNullOrEmpty(Asset.path_icon))
            {
                Asset.path_icon = Library.icon_path + Asset.getLocaleID();
            }
        }
        void LinkWithBaseLibrary()
        {
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.AddTimes(Asset.spawn_random_rate, Asset);
            }
        }
        void SetRarityAutomatically()
        {
            if (Asset.unlocked_with_achievement)
            {
                Asset.rarity = Rarity.R3_Legendary;
            }
            else
            {
                bool tHasDecisions = Asset.decision_ids != null;
                bool tHasSpells = Asset.spells_ids != null;
                bool tHasCombatActions = Asset.combat_actions_ids != null;
                bool tHasTag = Asset.base_stats.hasTags();
                bool tHasPlot = !string.IsNullOrEmpty(Asset.plot_id);
                int tCount = 0;
                if (Asset.action_death != null || Asset.action_special_effect != null || Asset.action_get_hit != null || Asset.action_birth != null || Asset.action_attack_target != null || Asset.action_on_add != null || Asset.action_on_remove != null || Asset.action_on_load != null)
                {
                    tCount++;
                }
                if (tHasDecisions)
                {
                    tCount++;
                }
                if (tHasSpells)
                {
                    tCount++;
                }
                if (tHasCombatActions)
                {
                    tCount++;
                }
                if (tHasTag)
                {
                    tCount++;
                }
                if (tHasPlot)
                {
                    tCount++;
                }
                if (tCount > 0)
                {
                    if (tCount == 1)
                    {
                        Asset.rarity = Rarity.R1_Rare;
                    }
                    else
                    {
                        Asset.rarity = Rarity.R2_Epic;
                    }
                }
            }
        }
        /// <summary>
        /// when a creature/group is spawned, this is the chance they get the trait, this is not out of 100
        /// </summary>
        public int ChanceToGetOnCreation { set {
                Asset.spawn_random_rate = value;
                Asset.spawn_random_trait_allowed = value > 0;
            }
            get
            {
                return Asset.spawn_random_rate;
            }
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            LinkWithTraits();
            LinkWithActors();
            base.LinkAssets();
        }
        /// <summary>
        /// Builds the Trait, if autolocalize is on it will use the ID'S as the translated text
        /// </summary>
        /// <remarks>
        /// if you have opposite traits/traits to remove which you build after you build this, link it with the other traits after you build them!
        /// </remarks>
        public virtual void Build(bool SetRarityAutomatically = false, bool AutoLocalize = true, bool LinkWithOtherAssets = false)
        {
            base.Build(LinkWithOtherAssets);
            if (AutoLocalize)
            {
                Localize(Asset.special_locale_id, Asset.special_locale_description, Asset.special_locale_description_2);
            }
            if (SetRarityAutomatically)
            {
                this.SetRarityAutomatically();
            }
            CheckIcon();
            LinkWithBaseLibrary();
        }
        /// <summary>
        /// the ID of the Localized Description, setting this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public string Description1ID { set
            {
                Asset.special_locale_description = value;
                if (value == null)
                {
                    Asset.has_description_1 = false;
                }
                else
                {
                    Asset.has_description_1 = true;
                }
            }
            get
            {
                return Asset.special_locale_description;
            }
        }
        /// <summary>
        /// the ID of the Localized 2nd Description, setting this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public string Description2ID
        {
            set
            {
                Asset.special_locale_description_2 = value;
                if (value == null)
                {
                    Asset.has_description_2 = false;
                }
                else
                {
                    Asset.has_description_2 = true;
                }
            }
            get
            {
                return Asset.special_locale_description_2;
            }
        }
        /// <summary>
        /// Localizes the Asset, you must set the ID's of the descriptions and name first
        /// </summary>
        public void Localize(string Name = null, string Description = null, string Description2 = null)
        {
            if (Name != null) {
                LM.AddToCurrentLocale(Asset.special_locale_id, Name);
            }
            if (Description != null)
            {
                LM.AddToCurrentLocale(Asset.special_locale_description, Description);
            }
            if (Description2 != null)
            {
                LM.AddToCurrentLocale(Asset.special_locale_description_2, Description2);
            }
        }
        /// <summary>
        /// the ID of the Localized Name, setting this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public string NameID
        {
            set
            {
                Asset.special_locale_id = value;
                if (value == null)
                {
                    Asset.has_localized_id = false;
                }
                else
                {
                    Asset.has_localized_id = true;
                }
            }
            get
            {
                return Asset.special_locale_id;
            }
        }
        /// <summary>
        /// The Displayed Rarity of the Trait
        /// </summary>
        /// <remarks>
        /// also, sometimes this controls how common it is like in subspecies traits
        /// </remarks>
        public Rarity Rarity { get { return Asset.rarity; } set { Asset.rarity = value; } }
        /// <summary>
        /// just like base stats, but mainly used to add Tags, not stats
        /// </summary>
        public BaseStats BaseStatsMeta { get { return Asset.base_stats_meta; } set { Asset.base_stats_meta = value; } }
        /// <summary>
        /// Not used for actor traits, but for kingdoms, subspecies, clans, etc. any actor born in that group will perform this action
        /// </summary>
        public WorldAction ActionOnBirth { get { return Asset.action_birth; } set { Asset.action_birth = value; } }
        /// <summary>
        /// used for actor traits and groups (kingdoms, clans, etc), any actor with the trait or in the group will perform this action on death
        /// </summary>
        public WorldAction ActionOnDeath { get { return Asset.action_death; } set { Asset.action_death = value; } }
        /// <summary>
        /// Used for groups (subspecies, clan, etc) whenever a actor in this group reaches their birthday (age goes up by one) they perform this action
        /// </summary>
        public WorldAction ActionOnGrowth {  get { return Asset.action_growth; } set { Asset.action_growth = value;} }
        /// <summary>
        /// used for actor traits and group traits (traits for kingdoms, clans, etc) any actor with the trait or in the group with this trait perform this when they are hit
        /// </summary>
        public GetHitAction ActionGetHit { get { return Asset.action_get_hit; } set { Asset.action_get_hit = value; } }
        /// <summary>
        /// when an actor writes a book, any trait from his language, culture, his traits, and religion can be in the book and can be transfered to those who read it, a trait must have this set to true to be able to be written
        /// </summary>
        public bool CanBeInBook { get { return Asset.can_be_in_book; } set { Asset.can_be_in_book = value; } }
        /// <summary>
        /// Used for languages and cultures, for languages it is the chance (out of 100) that the book written in this language does its action, and for cultures it controls experience for actors, spread of culture, etc.
        /// </summary>
        public float CustomValue { get { return Asset.value; } set { Asset.value = value;} }
        /// <summary>
        /// this stores an ID of a plot asset, used for religions, if a religion trait has a plotID, the plot can be done by the religion with the trait
        /// </summary>
        public string PlotID { get { return Asset.plot_id; } set { Asset.plot_id = value; } }
    }
}