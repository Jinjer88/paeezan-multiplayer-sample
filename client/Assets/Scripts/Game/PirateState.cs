using UnityEngine;

public abstract class PirateState : ScriptableObject
{
    public abstract void OnEnter(Pirate pirate);
    public abstract void OnExit(Pirate pirate);
    public abstract void OnStay(Pirate pirate);
}
