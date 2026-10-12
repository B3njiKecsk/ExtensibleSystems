using UnityEngine;

public class HypnotizedEffect : StatusEffect
{
    Team setTeam, resetTeam;
    bool originalDir;
    public HypnotizedEffect(Team teamToSwitchInto)
    {
        setTeam = teamToSwitchInto;
    }
    protected override void EffectOnApplied()
    {
        resetTeam = target.team;
        target.SetTeam(setTeam);
        target.actor.SetHypnotizedTint(true);

        if (target is Zombie)
        {
            originalDir = ((Zombie)target).MovesRight;
            ((Zombie)target).SetDirection(true);
        }
            
    }

    protected override void OnEffectEnd()
    {
        target.SetTeam(resetTeam);
        target.actor.SetHypnotizedTint(false);

        if (target is Zombie)
            ((Zombie)target).SetDirection(originalDir);
    }
}
