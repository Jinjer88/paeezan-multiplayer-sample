using UnityEngine;
using UnityEngine.UI;

public class Pirate : MonoBehaviour
{
    [Space]
    [Header("States:")]
    public PirateState attackingState;
    public PirateState movingState;
    public PirateState idleState;
    public PirateState dyingState;

    private PirateState currentState;
    private Animator animator;
    private SkinnedMeshRenderer meshRenderer;
    private Canvas healthBarCanvas;
    private Slider healthSlider;
    private string pirateType;

    private const string danceAnimParamName = "Dance";
    private const string danceRandomizerParamName = "DanceRandomizer";
    private const string dieAnimParamName = "Dead";

    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public bool IsMine { get; set; }
    public float AttackTimer { get; set; }

    public Animator Animator => animator;
    public string PirateType => pirateType;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        meshRenderer = animator.GetComponentInChildren<SkinnedMeshRenderer>();
        healthBarCanvas = GetComponentInChildren<Canvas>();
        healthSlider = healthBarCanvas.GetComponentInChildren<Slider>();
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

    public void InitUnit(bool isMine, int health, Material mat, string type)
    {
        pirateType = type;
        IsMine = isMine;
        meshRenderer.material = mat;
        MaxHealth = health;
        UpdateHealth(health);
        if (isMine == false)
            healthBarCanvas.transform.localRotation = Quaternion.Euler(45f, 180f, 0);
    }

    public void SwitchState(PirateState newState)
    {
        if (currentState != null && currentState.name == newState.name)
            return;

        if (currentState != null)
            currentState.OnExit(this);

        currentState = newState;
        currentState.OnEnter(this);
    }

    public void UpdateHealth(int health)
    {
        CurrentHealth = health;
        healthSlider.value = health / MaxHealth;
    }

    public void PlayRandomDanceAnimation()
    {
        int random = Random.Range(0, 4);
        animator.speed = Random.Range(0.9f, 1.25f);
        animator.SetFloat(danceRandomizerParamName, random);
        animator.SetBool(danceAnimParamName, true);
    }
}
