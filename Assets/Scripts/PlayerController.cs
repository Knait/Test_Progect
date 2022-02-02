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

    [Header("Paramaeters")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float walkingSpeed;

    [Header("References")]
    //Refs
    public Joystick joystick;
    [Header("Positions")]
    //Position for blade effect
    [SerializeField] private Transform bladePos;
    [SerializeField] private Transform gemPos;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem crashEffect;
    [SerializeField] private ParticleSystem bladeEffect;
    [SerializeField] private ParticleSystem coinEffect;

    [Header("Skins")]
    [SerializeField] private List<GameObject> skinList = new List<GameObject>();

    //Hidden
    //Inside refs
    private ParticleSystem dashRef;
    private ParticleSystem crashRef;
    private ParticleSystem bladeRef;
    private ParticleSystem coinRef;

    //Temporary public - set to private later
    public Animator playerAnimator;
    private Rigidbody thisRB;

    private Vector3 startingPlayerPosition;

    private Transform gemRef;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;

    //Flags
    [HideInInspector] public bool dashing = false;
    //To track if player is attached to the wall
    public bool flying = false;
    public bool attached;
    [HideInInspector]public bool playerControllsBlocked = false;

    private Collision prevCol;
    private ControllerColliderHit test;
    private Vector3 pos;
    private Quaternion rot;

    void Awake()
    {
        instance = this;
        thisRB = GetComponent<Rigidbody>();
        startingPlayerPosition = transform.position;
        transform.position = startingPlayerPosition;

        //Effects references
        crashRef = Instantiate(crashEffect, gameObject.transform);
        dashRef = Instantiate(dashEffect, gameObject.transform);
        bladeRef = Instantiate(bladeEffect, bladePos);
        coinRef = Instantiate(coinEffect);

        //Dash effect position and rotation
        dashRef.transform.position -= new Vector3(0, 0, 0.1f);
        dashRef.transform.localRotation = new Quaternion(0, -180, 0, 0);

        //References to instantiated effects
        crashRef.Pause();
        dashRef.Pause();
        bladeRef.Pause();
        coinRef.Pause();

        if (skinList.Count > 0)
        {
            //Turning every skin off
            foreach (GameObject targetSkin in skinList)
            {
                targetSkin.SetActive(false);
            }
        }

        //Setting the right skin
        skinList[PlayerPrefs.GetInt("BodySkin_ID")].SetActive(true);
        playerAnimator = skinList[PlayerPrefs.GetInt("BodySkin_ID")].GetComponent<Animator>();
    }

    void Start()
    {
        //Debug.Log(PlayerPrefs.GetInt("BodySkin_ID"));

        thisRB.useGravity = false;
        bladeRef.Play();
        if (!dashEffect || !crashEffect) Debug.LogError("Can't find particles! Please add them in the inspector.");

        //Find winning crystal on the level
        //gemRef = GameObject.Find("Crystal7").transform;
    }

    void Update()
    {
        pos.y = Mathf.Clamp(0, 0, 0);

        velocity = thisRB.velocity;
        pos = transform.position;

        //if (!flying && !GameController.Instance.endGame) StopPlayer();

        //Dashing trigger
        if (dashing && !attached && !GameController.Instance.paused)
        {
            thisRB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            Move();
            Quaternion lookRotation = Quaternion.LookRotation(Dir());
            //Vector3 rotation = Quaternion.Lerp(crashRef.transform.rotation, lookRotation, 1).eulerAngles;
            transform.rotation = lookRotation;
            //Dash Effect play
            dashRef.Play();
            //Animation

            //Flags
            dashing = false;

            //Stopping particles
            crashRef.Stop();
            bladeRef.Stop();
        }

        if(flying)
        {
            playerAnimator.SetBool("dashing", true);
            playerAnimator.SetBool("attached", false);
        }
    }

    public Vector3 GetStartingPosition()
    {
        return startingPlayerPosition;
    }

    public void SetStartingPosition(Vector3 _position)
    {
        StopPlayer();
        startingPlayerPosition = _position;
        transform.position = startingPlayerPosition;
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
        thisRB.velocity = Vector3.zero;
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
        if(coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(0.4f));

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
            if (coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(0.4f));
            //StopPlayer();
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
            //StopPlayer();
            //Don't play dashing animation
            playerAnimator.SetBool("dashing", false);
            //Stopping dash effect
            dashRef.Stop();
            if (coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(0.4f));
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

        //StopPlayer();

        //Push the player to the opposite direction
        StartCoroutine(PushPlayer());

        //LEAVE IT FOR EFFECTS
        //crashRef.Play();

        //Flying flag
        flying = false;
        //Don't Play dashing animation
        playerAnimator.SetBool("dashing", false);
        playerAnimator.SetBool("attached", true);

        //Stopping dash effect
        dashRef.Stop();
    }

    void LateUpdate()
    {
        //Find winning crystal on the level
        if (gemRef == null)
        {
            //Find gem
            gemRef = GameObject.Find("Crystal7").transform;
            //Assign it a random color
            gemRef.GetComponent<MeshRenderer>().material.SetColor("_Color", GameController.Instance.gemColors[Random.Range(0, GameController.Instance.gemColors.Count)]);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //If collided with coin
        if (other.gameObject.GetComponentInParent<Coin>())
        {
            //Set the position of coin to the visual
            coinRef.transform.position = other.transform.position;
            //Then attach it to the tube for movement
            coinRef.transform.SetParent(GameController.Instance.inGameTubes[0].transform);

            coinRef.Play();
            GameController.Instance.CollectGold();
            GameController.Instance.currentCoins--;
            other.gameObject.SetActive(false);
            StartCoroutine(StopCoinAfterSomeTime(0.4f));
        }

        //If collided with finishlane
        if (other.gameObject.GetComponent<NextLevel>())
        {
            transform.LookAt(new Vector3(0, 0, 0));
            playerAnimator.SetBool("win", true);
            GameController.Instance.NextLevel();
        }

        //When player reached pedestal
        if (other.gameObject.name == "GameObject")
        {
            playerAnimator.Play("поднял");
            StopPlayer();
            gemRef.SetParent(gemPos);
        }

        //If player reached finish lane
        if(other.gameObject.name == "Blockade")
        {
            playerControllsBlocked = true;
        }
    }
    #endregion

    //A coroutine to slightly push the player in the opposite direction
    IEnumerator PushPlayer()
    {
        Debug.Log("Pushin");
        thisRB.AddForce(new Vector3(0, 10f, 0), ForceMode.Impulse);

        yield return null;
    }

    IEnumerator StopCoinAfterSomeTime(float _time)
    {
        yield return new WaitForSeconds(_time);
        coinRef.Stop();
    }

    //Reset player animation back to "Idle"
    public void ResetPlayer()
    {
        StopPlayer();
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.position = new Vector3(0, 0, 6);

        StopAnimations();
        StopEffects();

        //Reset coinRef position back to player
        coinRef.transform.SetParent(gameObject.transform);

        bladeRef.Play();
        playerAnimator.SetBool("pickUp", false);
       
        playerControllsBlocked = false;
        flying = false;
    }

    public void DestroyGem()
    {
        Destroy(gemRef.gameObject);
    }

    public void StopPlayer()
    {
        //Reset the velocity
        thisRB.velocity = Vector3.zero;
        //Stop rotating
        thisRB.angularVelocity = Vector3.zero;
        thisRB.Sleep(); //Important bit, that helped somehow stop movement of player
        //Debug.Log("STOPPED");
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

    //A fucntion for moving player towards crystal
    public void WalkTowardsCrystal()
    {
        StopEffects();
        StopPlayer();
        Vector3 dir = new Vector3(0, 0, 0) - gameObject.transform.position;

        for(float i = 0; i < 4; i += 0.5f)
        {
            thisRB.AddForce(dir.normalized, ForceMode.Impulse);
        }
    }
}
