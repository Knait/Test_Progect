using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Singleton
    //Need to rewrite joystick then
    private static PlayerController instance;
    public static PlayerController Instance => instance;
    #endregion

    //Refs
    public Joystick joystick;
    [SerializeField] private float dashSpeed;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem crashEffect;

    //Inside refs
    private ParticleSystem dashRef;
    private ParticleSystem crashRef;
    private Animator playerAnimator;
    private Rigidbody thisRB;

    //Hidden
    private Vector3 startingPlayerPosition;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;

    [HideInInspector] public bool dashing = false;
    //To track if player is attached to the wall
    public bool flying = false;
    Collision prevCol;
    void Awake()
    {
        instance = this;
        startingPlayerPosition = transform.position;

        crashRef = Instantiate(crashEffect, gameObject.transform);
        dashRef = Instantiate(dashEffect, gameObject.transform);

        //Dash effect position and rotation
        dashRef.transform.position -= new Vector3(0, 0, 0.1f);
        dashRef.transform.localRotation = new Quaternion(0, -180, 0, 0);

        //References to instantiated effects
        crashRef.Pause();
        dashRef.Pause();

        playerAnimator = GetComponentInChildren<Animator>();
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
        if (dashing  && !flying)
        {
            thisRB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            Move();
            Quaternion lookRotation = Quaternion.LookRotation(Dir());
            //Vector3 rotation = Quaternion.Lerp(crashRef.transform.rotation, lookRotation, 1).eulerAngles;
            transform.rotation = lookRotation;
            //Dash Effect play
            dashRef.Play();
            //Animation
            playerAnimator.SetBool("dashing", true);
            playerAnimator.SetBool("attached", false);
            //Flags
            dashing = false;
            flying = true;
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
    }

    public void IncreaseStartingPosition(Vector3 _position)
    {
        //Reset the velocity, so the speed will remain the same
        thisRB.velocity = new Vector3(0, 0, 0);
        startingPlayerPosition += _position;
    }

    #region Movement
    //Actual movement of the player
    public void Move()
    {
        //Reset the velocity, so the speed will remain the same
        thisRB.velocity = new Vector3(0, 0, 0);
        thisRB.AddForce(Dir() * dashSpeed, ForceMode.Impulse);
        //Debug.Log("Direction " + Dir());
    }

    //Calculate direction
    private Vector3 Dir()
    {
        Vector3 dir = Vector3.zero;

        dir.x = joystick.Horizontal();
        dir.z = joystick.Vertical();

        //DEBUG!!
        //Debug.Log(dir);
        if (dir.magnitude > 1) dir.Normalize();
        return dir;
    }
    #endregion

    #region Collision
    void OnCollisionEnter(Collision collision)
    {
        //Debug
        //Debug.Log("Collision" + collision.transform.position);
        //

        //If the player collided with the same collider
        if(prevCol == collision)
        {
            //Debug.Log("SameCollision " + prevCol.transform.position);
            flying = false;
            //Reset the velocity
            thisRB.velocity = Vector3.zero;
            //Stop rotating
            thisRB.angularVelocity = Vector3.zero;
            //Flying flag
            flying = false;
            //Play dashing animation
            playerAnimator.SetBool("dashing", false);
            //Stopping dash effect
            dashRef.Stop();
        }

        //To track the previous collision
        prevCol = collision;

        //Push the player to the opposite direction
        StartCoroutine(PushPlayer());

        //crashRef.transform.position = collision.collider.ClosestPoint(transform.position);

        //Making sure the player looks at the collided object
        Vector3 dir = collision.collider.ClosestPoint(transform.position) - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir.normalized);
        //Vector3 rotation = Quaternion.Lerp(crashRef.transform.rotation, lookRotation, 1).eulerAngles;
        transform.rotation = lookRotation;

        //Reset the velocity
        thisRB.velocity = Vector3.zero;
        //Stop rotating
        thisRB.angularVelocity = Vector3.zero;

        //LEAVE IT FOR EFFECTS
        //crashRef.Play();

        //Flying flag
        flying = false;
        //Play dashing animation
        playerAnimator.SetBool("dashing", false);
        playerAnimator.SetBool("attached", true);
        //Stopping dash effect
        dashRef.Stop();
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

    //A coroutine to slightly push the player in the opposite direction
    IEnumerator PushPlayer()
    {
        thisRB.AddForce(-Dir() * 0.2f, ForceMode.Impulse);

        //Reset the velocity
        thisRB.velocity = Vector3.zero;
        //Stop rotating
        thisRB.angularVelocity = Vector3.zero;

        yield return null;
    }

    //Reset player animation back to "Idle"
    public void ResetPlayer()
    {
        transform.rotation = new Quaternion(0, 0, 0, 0);
        playerAnimator.SetBool("dashing", false);
        flying = false;

        //Reset the velocity
        thisRB.velocity = Vector3.zero;
        //Stop rotating
        thisRB.angularVelocity = Vector3.zero;
    }
}
