using UnityEngine;

[CreateAssetMenu(menuName = "Pirate States/Pirate State Dying")]
public class PirateStateDying : PirateState
{
    private const string dieAnimParamName = "Dead";
    private const string speedAnimParamName = "Speed";

    public override void OnEnter(Pirate pirate)
    {
        pirate.Animator.SetFloat(speedAnimParamName, 0);
        pirate.HideHealthBar();
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
