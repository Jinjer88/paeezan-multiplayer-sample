using UnityEngine;

[CreateAssetMenu(menuName = "Pirate States/Pirate State Moving")]
public class PirateStateMoving : PirateState
{
    private const string speedAnimParamName = "Speed";

    public override void OnEnter(Pirate pirate)
    {
        pirate.Animator.SetFloat(speedAnimParamName, 1f);
    }

    public override void OnExit(Pirate pirate)
    {
        pirate.Animator.SetFloat(speedAnimParamName, 0);
    }

    public override void OnStay(Pirate pirate)
    {
        if (pirate.CurrentHealth <= 0)
            pirate.SwitchState(pirate.dyingState);
    }
}
