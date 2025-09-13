using UnityEngine;

[CreateAssetMenu(menuName = "Pirate States/Pirate State Moving")]
public class PirateStateMoving : PirateState
{
    public override void OnEnter(Pirate pirate)
    {
        pirate.PlayRunAnimation();
    }

    public override void OnExit(Pirate pirate)
    {
    }

    public override void OnStay(Pirate pirate)
    {
    }
}
