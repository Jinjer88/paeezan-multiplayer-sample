using UnityEngine;

[CreateAssetMenu(menuName = "Pirate States/Pirate State Attacking")]
public class PirateStateAttacking : PirateState
{
    private const string attackingAnimParamName = "Attacking";
    private const string attackAnimParamName = "Attack";
    private const string pistolAnimParamName = "Pistol";

    public override void OnEnter(Pirate pirate)
    {
        pirate.Animator.SetBool(attackingAnimParamName, true);

        if (pirate.PirateType == "melee")
        {
            pirate.Animator.SetTrigger(attackAnimParamName);
        }
        else
        {
            pirate.Animator.SetTrigger(pistolAnimParamName);
        }

        pirate.AttackTimer = 2f;
    }

    public override void OnExit(Pirate pirate)
    {
        pirate.Animator.SetBool(attackingAnimParamName, false);
    }

    public override void OnStay(Pirate pirate)
    {
        pirate.AttackTimer -= Time.deltaTime;
        if (pirate.AttackTimer < 0)
        {
            pirate.AttackTimer = 2f;
            if (pirate.PirateType == "melee")
            {
                pirate.Animator.SetTrigger(attackAnimParamName);
            }
            else
            {
                pirate.Animator.SetTrigger(pistolAnimParamName);
            }
        }
    }
}
