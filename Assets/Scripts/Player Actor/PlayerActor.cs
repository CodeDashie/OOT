using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerActor : MonoBehaviour
{
    // --

    public enum State
    {
        WALKING,
        ON_LEDGE,
        PUSHING_OBJECT,
        //HOLDING_OBJECT
        LADDER
    }
    
    public State state { get; set; }

    // --

    // flashlight
    public bool flashlightEnabled;

    // attributes
    public float walkSpeed = 5.0f;
    public float runSpeed = 10.0f;
    public float turnSpeed = 17.5f;

    public float jumpVelocity = 10.0f;
    public float gravity = -0.4f;
    public float groundedGravity = -4.0f;

    public float pushPower = 2.0f;
    public float throwPower = 250.0f;

    // default character controller radius
    public float DEFAULT_CHARACTER_CONTROLLER_RADIUS { get; private set; }
    
    //[HideInInspector]
    public /*new*/ Camera camera;

    public bool isController = true;

    // state
    public bool isWalking       { get; set; } = false;
    public bool isJumping       { get; set; } = false;
    public bool isOnLedge       { get; set; } = false;
    public bool isClimbing      { get; set; } = false;
    public bool isHoldingObject { get; set; } = false;

    public float fallVelocity { get; set; } = 0.0f;
    
    public Transform head;
    public Animator anim { get; set; }
    public CharacterController controller { get; set; }
    [SerializeField]
    private GameObject _grabLedgeBox;
    [SerializeField]
    private GameObject _grabObjectBox;

    public float energy;
    public int minimumRespawnEnergy;

    [Range(1, 10)]
    public int standardEnergyMultiplier;

    [Range(1, 10)]
    public int flashlightEnergyMultiplier;

    public Transform testObject;

    public delegate void OnPlayerDeathDelegate();
    //public static event OnPlayerDeathDelegate onPlayerDeathEvent;
    public OnPlayerDeathDelegate onPlayerDeath;

    void Awake()
    {
        camera = FindObjectOfType<Camera>();

        // cursor settings
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // grab components for public use
        anim = GetComponentInChildren<Animator>();
        controller = gameObject.GetComponent<CharacterController>();

        // set default state
        state = State.WALKING;
        DEFAULT_CHARACTER_CONTROLLER_RADIUS = controller.radius;

        // attach and setup script components
        //GrabLedge grabLedge = _grabBox.AddComponent<GrabLedge>();
        gameObject.AddComponent<MovementJumpGravity>().SetValues(this);
        _grabLedgeBox.AddComponent<LadderState>().SetValues(this);
        //_grabLedgeBox.AddComponent<GrabLedge>().SetValues(this);
        //_grabObjectBox.AddComponent<GrabObject>().SetValues(this);
        //gameObject.AddComponent<MoveObject>().SetValues(this);
        gameObject.AddComponent<AnimationState>().SetValues(this);
        //gameObject.AddComponent<FallingPlatformCollide>();
        //gameObject.AddComponent<Button>().SetValues(this);
    }

    private void OnLevelWasLoaded(int level)
    {
        camera = FindObjectOfType<Camera>();
    }
}
