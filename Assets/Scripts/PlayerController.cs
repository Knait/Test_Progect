using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Could use the singleton
    //Need to rewrite joystick then
    private static PlayerController instance;
    public static PlayerController Instance => instance;

    //Parameters
    public Joystick joystick;
    [SerializeField] private float dashSpeed;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem crashEffect;

    //Inside refs
    private ParticleSystem dashRef;
    private ParticleSystem crashRef;
    private Animator playerAnimator;

    //Hidden
    private Vector3 startingPlayerPosition;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;

    [HideInInspector] public bool dashing = false;
    //To track if player is attached to the wall
    private bool attached = true;

    //References
    private Rigidbody thisRB;

    void Awake()
    {
        instance = this;
        startingPlayerPosition = transform.position;

        crashRef = Instantiate(crashEffect, gameObject.transform);
        dashRef = Instantiate(dashEffect, gameObject.transform);

        //References to instantiated effects
        crashRef.Pause();
        dashRef.Pause();

        playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        thisRB = GetComponent<Rigidbody>();
        thisRB.useGravity = false;

        if (!dashEffect || !crashEffect) Debug.LogError("Can't find particles! Please add them in the inspector.");
    }

    void Update()
    {
        //Dashing trigger
        if (dashing == true && !GameController.Instance.paused && attached)
        {
            Move();
            dashRef.transform.localPosition = -Dir() + new Vector3(0.2f, 0, 0.2f); //Added a temporary offset
            dashRef.Play();
            dashing = false;
            attached = false;

            //Unfreeze everything when jumping of the wall
            thisRB.constraints = RigidbodyConstraints.None;
            //Then add the required constrains
            thisRB.constraints = RigidbodyConstraints.FreezeRotation;
            thisRB.constraints = RigidbodyConstraints.FreezePositionY;
        }

        velocity = GetComponent<Rigidbody>().velocity;
    }

    public Vector3 GetStartingPosition()
    {
        return startingPlayerPosition;
    }

    public void SetStartingPosition(Vector3 _position)
    {
        //Reset the velocity, so the speed will remain the same
        thisRB.velocity = new Vector3(0, 0, 0);
        startingPlayerPosition = _position;
        attached = true;
    }

    public void IncreaseStartingPosition(Vector3 _position)
    {
        //Reset the velocity, so the speed will remain the same
        thisRB.velocity = new Vector3(0, 0, 0);
        startingPlayerPosition += _position;
        attached = true;
    }

    #region Movement
    //Actual movement of the player
    public void Move()
    {
        //Reset the velocity, so the speed will remain the same
        thisRB.velocity = new Vector3(0, 0, 0);
        thisRB.AddForce(Dir() * dashSpeed, ForceMode.Impulse);
    }

    //Calculate direction
    private Vector3 Dir()
    {
        Vector3 dir = Vector3.zero;

        dir.x = joystick.Horizontal();
        dir.z = joystick.Vertical();

        if (dir.magnitude > 1) dir.Normalize();

        return dir;
    }
    #endregion

    #region Collision
    void OnCollisionEnter(Collision collision)
    {
        crashRef.transform.position = collision.collider.ClosestPoint(transform.position);

        Vector3 dir = collision.collider.ClosestPoint(transform.position) - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(crashRef.transform.rotation, lookRotation, 1).eulerAngles;
        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        //crashRef.Play();
        //Push the player to the opposite direction
        //thisRB.AddForce(-1 * Dir() * 0.5f, ForceMode.Impulse);

        //The player is attached to the wall
        attached = true;
        //Freeze the player translations as a whole (to avoid wall surfing)
        thisRB.constraints = RigidbodyConstraints.FreezeAll;
    }

    void OnTriggerEnter(Collider other)
    {
        //If collided with coin
        if (other.gameObject.GetComponentInParent<Coin>())
        {
            GameController.Instance.score++;
            GameController.Instance.currentCoins--;
            other.gameObject.SetActive(false);
        }

        //If collided with finishlane
        if (other.gameObject.GetComponent<NextLevel>())
        {
            GameController.Instance.NextLevel();
        }
    }
    #endregion
}
