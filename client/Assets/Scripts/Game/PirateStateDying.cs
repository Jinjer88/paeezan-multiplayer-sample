using UnityEngine;

[CreateAssetMenu(menuName = "Pirate States/Pirate State Dying")]
public class PirateStateDying : PirateState
{
    private const string dieAnimParamName = "Dead";

    public override void OnEnter(Pirate pirate)
    {
        pirate.Animator.SetBool(dieAnimParamName, true);
    }

    public override void OnExit(Pirate pirate)
    {
        pirate.Animator.SetBool(dieAnimParamName, false);
    }

    public override void OnStay(Pirate pirate)
    {
    }
}
