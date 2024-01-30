namespace NeoModLoader.General.Event.Handlers;

public abstract class ActorTryToAttackHandler : AbstractHandler<ActorTryToAttackHandler>
{
    public abstract void Handle(Actor      pAttacker, BaseSimObject pTarget, CombatActionAsset pCombatActionAsset,
                                AttackData pAttackData);
}