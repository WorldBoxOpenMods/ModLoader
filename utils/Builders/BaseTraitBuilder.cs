using HarmonyLib;
using NeoModLoader.General;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// The Base Builder for Trait builders
    /// </summary>
    public class BaseTraitBuilder<A, AL> : AugmentationAssetBuilder<A, AL> where A : BaseTrait<A> where AL : BaseTraitLibrary<A>
    {
        /// <inheritdoc/>
        public BaseTraitBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        protected override void Init()
        {
            base.Init();
            SetDescription1ID(null);
            SetDescription2ID(null);
            SetNameID(null);
        }
        /// <summary>
        /// Adds a opposite trait to the asset's list of opposite traits, if a object has one of these traits it cannot add this trait
        /// </summary>
        /// <param name="ID"></param>
        public void AddOpposite(string ID)
        {
            Asset.addOpposite(ID);
        }
        /// <summary>
        /// Adds a trait which this trait automatically removes when it is added to a object
        /// </summary>
        public void AddTraitToRemove(string ID)
        {
            Asset.traits_to_remove_ids.AddItem(ID);
        }

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
        /// <summary>
        /// links this trait with its opposite traits and traits to remove
        /// </summary>
        public void LinkWithTraits()
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
                    string tID2 = Asset.traits_to_remove_ids[i];
                    A tTraitToAdd = Library.get(tID2);
                    Asset.traits_to_remove[i] = tTraitToAdd;
                }
            }
        }
        void LinkWithLibrary()
        {
            if (Asset.spawn_random_trait_allowed)
            {
                Library._pot_allowed_to_be_given_randomly.AddTimes(Asset.spawn_random_rate, Asset);
            }
        }
        /// <summary>
        /// when a creature is spawned, this sets the chance they get the trait, this is not out of 100
        /// </summary>
        public void SetChanceToGetOnCreation(int Chance)
        {
            Asset.spawn_random_rate = Chance;
            Asset.spawn_random_trait_allowed = Chance > 0;
        }
        /// <summary>
        /// Builds the Trait, if autolocalize is on it will use the ID'S as the translated text, if you have opposite traits/traits to remove which you build after you build this, link it with the other traits after you build them!
        /// </summary>
        public virtual void Build(bool LinkWithOtherTraits = true, bool AutoLocalize = true)
        {
            if (LinkWithOtherTraits)
            {
                LinkWithTraits();
            }
            if (AutoLocalize)
            {
                Localize(Asset.special_locale_id, Asset.special_locale_description, Asset.special_locale_description_2);
            }
            LinkWithLibrary();
            LinkWithActors();
            base.Build();
        }
        /// <summary>
        /// Sets the ID of the Localized Description, this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public void SetDescription1ID(string Description)
        {
            Asset.special_locale_description = Description;
            if(Description == null)
            {
                Asset.has_description_1 = false;
            }
            else
            {
                Asset.has_description_1 = true;
            }
        }
        /// <summary>
        /// Sets the ID of the Localized 2nd Description, this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public void SetDescription2ID(string Description)
        {
            Asset.special_locale_description_2 = Description;
            if (Description == null)
            {
                Asset.has_description_2 = false;
            }
            else
            {
                Asset.has_description_2 = true;
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
        /// Sets the ID of the Localized Name, this does not fully localize the asset, you must either call Localize() or have a localization folder
        /// </summary>
        public void SetNameID(string Name)
        {
            Asset.special_locale_id = Name;
            if (Name == null)
            {
                Asset.has_localized_id = false;
            }
            else
            {
                Asset.has_localized_id = true;
            }
        }
        /// <summary>
        /// The Displayed Rarity of the Trait
        /// </summary>
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
        /// <summary>
        /// Used for phenotypes in subspecies, if true the icon will not use the path icon, instead it will use a phenotype color attached to the subspecies trait
        /// </summary>
        public bool UsesSpecialIconLogic { get { return Asset.special_icon_logic; } set {  Asset.special_icon_logic = value;} }
    }
}