using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Pirate : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public SkinnedMeshRenderer meshRenderer;
    [HideInInspector] public PirateState currentState;

    [Space]
    [Header("States:")]
    public PirateState attackingState;
    public PirateState movingState;
    public PirateState idleState;
    public PirateState dyingState;

    private const string speedAnimParamName = "Speed";
    private const string danceAnimParamName = "Dance";
    private const string danceRandomizerParamName = "DanceRandomizer";
    private const string dieAnimParamName = "Dead";
    private const string attackAnimParamName = "Attack";
    private const string attackingAnimParamName = "Attacking";
    private const string pistolAnimParamName = "Pistol";

    public int CurrentHealth { get; set; }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        meshRenderer = animator.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        SwitchState(idleState);
    }

    private void Update()
    {
        if (currentState != null)
            currentState.OnStay(this);
    }

    public void SwitchState(PirateState newState)
    {
        if (currentState != null)
            currentState.OnExit(this);

        currentState = newState;
        currentState.OnEnter(this);
    }

    public void Dance()
    {
        int random = Random.Range(0, 4);
        animator.speed = Random.Range(0.9f, 1.25f);
        animator.SetFloat(danceRandomizerParamName, random);
        animator.SetBool(danceAnimParamName, true);
    }
}
