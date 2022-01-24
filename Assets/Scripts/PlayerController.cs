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
    [SerializeField] private ParticleSystem bladeEffect;
    [SerializeField] private ParticleSystem coinEffect;
    //Position for blade effect
    [SerializeField] private Transform bladePos;

    //Inside refs
    private ParticleSystem dashRef;
    private ParticleSystem crashRef;
    private ParticleSystem bladeRef;
    private ParticleSystem coinRef;

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
    Vector3 pos;
    Quaternion rot;
    void Awake()
    {
        instance = this;
        thisRB = GetComponent<Rigidbody>();
        startingPlayerPosition = transform.position;

        //Effects references
        crashRef = Instantiate(crashEffect, gameObject.transform);
        dashRef = Instantiate(dashEffect, gameObject.transform);
        bladeRef = Instantiate(bladeEffect, bladePos);
        coinRef = Instantiate(coinEffect, gameObject.transform);

        //Dash effect position and rotation
        dashRef.transform.position -= new Vector3(0, 0, 0.1f);
        dashRef.transform.localRotation = new Quaternion(0, -180, 0, 0);

        //References to instantiated effects
        crashRef.Pause();
        dashRef.Pause();
        bladeRef.Pause();
        coinRef.Pause();

        playerAnimator = GetComponentInChildren<Animator>();
        pos = transform.position;
    }

    void Start()
    {
        thisRB.useGravity = false;
        bladeRef.Play();
        if (!dashEffect || !crashEffect) Debug.LogError("Can't find particles! Please add them in the inspector.");
    }

    void Update()
    {
        pos = transform.position;
        pos.y = Mathf.Clamp(0, 0, 0);

        velocity = thisRB.velocity;
        //Dashing trigger
        if (dashing && !flying)
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

            //Stopping particles
            crashRef.Stop();
            bladeRef.Stop();
            coinRef.Stop();
        }

        transform.position = pos;
    }

    public Vector3 GetStartingPosition()
    {
        return startingPlayerPosition;
    }

    public void SetStartingPosition(Vector3 _position)
    {
        StopPlayer();
        startingPlayerPosition = _position;
    }

    public void IncreaseStartingPosition(Vector3 _position)
    {
        StopPlayer();
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
        bladeRef.Play();
        //Debug.Log(collision.collider.name);
        //Debug.Log("Collision");
        //If player collided with obstacle
        if (collision.gameObject.GetComponent<ObstacleWallController>())
        {
            playerAnimator.SetBool("death", true);
            playerAnimator.SetBool("dashing", false);
            playerAnimator.SetBool("attached", false);
            //Stopping dash effect
            dashRef.Stop();
            coinRef.Stop();
            return;
        }
          
        //Debug
        //Debug.Log("Collision" + collision.transform.position);
        //

        //If the player collided with the same collider
        if (prevCol == collision)
        {
            //Debug.Log("SameCollision " + prevCol.transform.position);
            flying = false;
            //Reset the velocity
            thisRB.velocity = Vector3.zero;
            //Stop rotating
            thisRB.angularVelocity = Vector3.zero;
            //Don't play dashing animation
            playerAnimator.SetBool("dashing", false);
            //Stopping dash effect
            dashRef.Stop();
            coinRef.Stop();
        }

        //To track the previous collision
        prevCol = collision;

        crashRef.transform.position = collision.collider.ClosestPoint(transform.position);

        //Making sure the player looks at the collided object
        //ONLY ROTATE ON Y AXIS
        Vector3 dir = collision.collider.ClosestPoint(transform.position) - transform.position;
        //Works now? -NO!
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        lookRotation.x = 0;
        lookRotation.z = 0;

        //Debug.Log("Look " + lookRotation);
        transform.rotation = lookRotation;

        //Push the player to the opposite direction
        StartCoroutine(PushPlayer());

        StopPlayer();

        //LEAVE IT FOR EFFECTS
        //crashRef.Play();

        //Flying flag
        flying = false;
        //Don't Play dashing animation
        playerAnimator.SetBool("dashing", false);
        playerAnimator.SetBool("attached", true);

        //Stopping dash effect
        dashRef.Stop();
        coinRef.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        //If collided with coin
        if (other.gameObject.GetComponentInParent<Coin>())
        { 
            coinRef.Play();
            GameController.Instance.CollectGold();
            GameController.Instance.currentCoins--;
            other.gameObject.SetActive(false);
        }

        //If collided with finishlane
        if (other.gameObject.GetComponent<NextLevel>())
        {
            transform.LookAt(new Vector3(0, 0, 0));
            playerAnimator.SetBool("win", true);
            GameController.Instance.NextLevel();
        }
    }
    #endregion

    //A coroutine to slightly push the player in the opposite direction
    IEnumerator PushPlayer()
    {
        thisRB.AddForce(-Dir() * 0.3f, ForceMode.Impulse);

        yield return null;
    }

    //Reset player animation back to "Idle"
    public void ResetPlayer()
    {
        StopPlayer();
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.position = new Vector3(0, 0, 6);

        StopAnimations();
        StopEffects();
        bladeRef.Play();
        flying = false;
    }

    public void StopPlayer()
    {
        //Reset the velocity
        thisRB.velocity = Vector3.zero;
        //Stop rotating
        thisRB.angularVelocity = Vector3.zero;
        thisRB.Sleep(); //Important bit, that helped somehow stop movement of player
    }

    public void StopEffects()
    {
        dashRef.Stop();
        crashRef.Stop();
        bladeRef.Stop();
        coinRef.Stop();
    }

    void StopAnimations()
    {
        playerAnimator.SetBool("dashing", false);
        playerAnimator.SetBool("death", false);
        playerAnimator.SetBool("win", false);
        playerAnimator.SetBool("attached", false);
    }
}
