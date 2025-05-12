using Newtonsoft.Json;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// A Builder for building augmentation assets
    /// </summary>
    public class AugmentationAssetBuilder<A, AL> : UnlockableAssetBuilder<A, AL> where A : BaseAugmentationAsset, new() where AL : BaseLibraryWithUnlockables<A>
    {
        /// <inheritdoc/>
        public AugmentationAssetBuilder(string FilePath, bool LoadImmediately) : base(FilePath, LoadImmediately) { }
        /// <inheritdoc/>
        public AugmentationAssetBuilder(string ID) : base(ID) { }
        /// <inheritdoc/>
        public AugmentationAssetBuilder(string ID, string CopyFrom) : base(ID, CopyFrom) { }
        /// <summary>
        /// Adds a combat action to this Asset, any actors with this Asset will have this actiom, and if this Asset is for a clan/subspecies/etc, any actors in that group will also have it
        /// </summary>
        public void AddCombatAction(string ID)
        {
            Asset.addCombatAction(ID);
        }
        void LinkDecisions()
        {
            if (Asset.decision_ids != null)
            {
                Asset.decisions_assets = new DecisionAsset[Asset.decision_ids.Count];
                for (int i = 0; i < Asset.decision_ids.Count; i++)
                {
                    string tDecisionID = Asset.decision_ids[i];
                    DecisionAsset tDecisionAsset = AssetManager.decisions_library.get(tDecisionID);
                    Asset.decisions_assets[i] = tDecisionAsset;
                }
            }
        }
        /// <inheritdoc/>
        public override void LinkAssets()
        {
            LinkDecisions();
            Asset.linkCombatActions();
            Asset.linkSpells();
            base.LinkAssets();
        }
        /// <summary>
        /// Adds a Decision (Neuron) to the Object with this Asset, if the object is a group like clan or subspecies, all actors in the group get this decision
        /// </summary>
        public void AddDecision(string ID) { Asset.addDecision(ID);}

        /// <summary>
        /// Adds a spell to the Asset, which an actor can use
        /// </summary>
        public void AddSpell(string ID) { Asset.addSpell(ID);}
        /// <summary>
        /// Adds a attack action for a actor with this asset, or a actor apart of a group with this trait
        /// </summary>
        /// <param name="Action"></param>
        public void AddAttackAction(AttackAction Action)
        {
            Asset.action_attack_target += Action;
        }
        /// <summary>
        /// The Action Performed on something, when this asset is added to it!
        /// </summary>
        public WorldActionTrait ActionWhenAdded { get { return Asset.action_on_add; } set { Asset.action_on_add = value; } }
        /// <summary>
        /// The Action Performed on something, when this asset is removed from it!
        /// </summary>
        public WorldActionTrait ActionWhenRemoved { get { return Asset.action_on_remove; } set { Asset.action_on_remove = value; } }
        /// <summary>
        /// The Action Performed on something with this asset, when that something is loaded from a save file
        /// </summary>
        public WorldActionTrait ActionOnLoad { get { return Asset.action_on_load; } set { Asset.action_on_load = value; } }
        /// <summary>
        /// The Action Performed on something, this keeps happening until that something is destroyed or this is removed
        /// </summary>
        public WorldAction ActonSpecialEffect { get { return Asset.action_special_effect; } set { Asset.action_special_effect = value; } }
        /// <summary>
        /// the cooldown for the ActionSpecialEffect
        /// </summary>
        public float SpecialEffectCoolDown { get { return Asset.special_effect_interval; } set { Asset.special_effect_interval = value; } }
        /// <summary>
        /// if false, it cant be removed from something
        /// </summary>
        public bool CanBeRemoved { get { return Asset.can_be_removed; } set { Asset.can_be_removed = value; } }
        /// <summary>
        /// if false, this asset cant be given to something, AND it cant be removed
        /// </summary>
        public bool CanBeGiven { get { return Asset.can_be_given; } set { Asset.can_be_given = value; } }
        /// <summary>
        /// The ID of the group this asset is in, you can find them in S_TraitGroup, S_EquipmentGroup, etc
        /// </summary>
        public string Group { get { return Asset.group_id; } set { Asset.group_id = value; } }
        /// <summary>
        /// The Priority used for when this asset is being displayed next to other assets in its Group
        /// </summary>
        public int Priority { get { return  Asset.priority; } set { Asset.priority = value; } }
        /// <summary>
        /// If true, Meta Editors like the Plots Editor and Equipment Editor will show this asset
        /// </summary>
        public bool ShowInMetaEditor { get { return Asset.show_in_meta_editor; } set { Asset.show_in_meta_editor = value; } }
    }
}
