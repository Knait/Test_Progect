using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using AndroidNativeCore;
public class PlayerController : MonoBehaviour
{
    #region Singleton
    //Need to rewrite joystick then
    private static PlayerController instance;
    public static PlayerController Instance => instance;
    #endregion

    [Header("Paramaeters")]
    [Tooltip("Скорость рывка.")]
    [SerializeField] private float dashSpeed;
    [Tooltip("Скорость игрока при подходе к кристалу.")]
    [SerializeField] private float walkingSpeed;

    [Header("References")]
    //Refs
    public Joystick joystick;
    [Header("Positions")]
    //Positions 
    [Tooltip("Позиция меча.")]
    private Transform swordPos;

    [Tooltip("Позиция кристала, для финишной анимации.")]
    private Transform gemPos;

    [Tooltip("Позиция пояса, для финишной анимации.")]
    private Transform beltPos;

    [Header("Effects")]

    [Tooltip("Эффект столкновения.")]
    [SerializeField] private ParticleSystem crashEffect;
    [Tooltip("Эффект искр меча.")]
    [SerializeField] private ParticleSystem bladeEffect;
    [Tooltip("Эффект подбора кристала.")]
    [SerializeField] private ParticleSystem coinEffect;

    [Header("Effect Parameters")]
    [Tooltip("Время после которого исчезнет эффект подбора кристала.")]
    [SerializeField] private float crystalEffectDisappearTime;
    [Tooltip("Время после которого исчезнет эффект столкновения.")]
    [SerializeField] private float crashEffectDisappearTime;

    [Header("Skins")]
    [SerializeField] private List<GameObject> skinList = new List<GameObject>();
    [Header("Swords")]
    [SerializeField] private List<GameObject> swordList = new List<GameObject>();

    //Hidden
    //Inside refs
    private ParticleSystem crashRef;
    private ParticleSystem bladeRef;
    private ParticleSystem coinRef;
    private GameObject swordRef;

    //Temporary public - set to private later
    [HideInInspector] public Animator playerAnimator;
    private Rigidbody thisRB;

    private Vector3 startingPlayerPosition;

    private Transform gemRef;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;

    //Flags
    [HideInInspector] public bool dashing = false;
    //To track if player is attached to the wall

    [Header("DEBUG")]
    public Transform particlePos;
    public bool flying = false;
    public bool attached;

    public bool vibrate = false;
    [HideInInspector]public bool playerControllsBlocked = false;

    private Collision prevCol;
    private ControllerColliderHit test;
    private Vector3 pos;
    private Quaternion rot;

    void Awake()
    {
        instance = this;

        //If there are loaded skins - turn them all off
        if (skinList.Count > 0)
        {
            //Turning every skin off
            foreach (GameObject targetSkin in skinList)
            {
                targetSkin.SetActive(false);
            }
        }

        if (swordList.Count > 0)
        {
            //Turning every sword off
            foreach (GameObject targetSword in swordList)
            {
                targetSword.SetActive(false);
            }
        }

        //Temp variable to hold skin id
        var skinId = PlayerPrefs.GetInt("BodySkin_ID");
        //Setting the right skin
        skinList[skinId].SetActive(true);

        //Getting animator
        playerAnimator = skinList[skinId].GetComponent<Animator>();
        //Getting the required positions
        swordPos = skinList[skinId].GetComponent<PositionsHolder>().swordPos;
        beltPos = skinList[skinId].GetComponent<PositionsHolder>().beltPos;
        gemPos = skinList[skinId].GetComponent<PositionsHolder>().crystalPos;

        //Temp variable to hold sword id
        var swordId = PlayerPrefs.GetInt("SwordSkin_ID");
        //Setting the right skin
        swordList[swordId].SetActive(true);
        swordRef = swordList[swordId];

        //particlePos = swordList[swordId].

        //Assing the activated sword the right position
        swordRef.transform.SetParent(swordPos);
        //Effect position on sword
        particlePos = swordRef.GetComponent<Sword>().effectPos;
        bladeRef = Instantiate(bladeEffect, particlePos);

        swordRef.transform.rotation = swordPos.rotation;
        swordRef.transform.position = swordPos.position;
        swordRef.transform.localScale = swordPos.localScale;

        //Getting a reference to this RigidBody
        thisRB = GetComponent<Rigidbody>();

        //Setting current position as a starting position
        startingPlayerPosition = transform.position;
        transform.position = startingPlayerPosition;

        //Effects references
        crashRef = Instantiate(crashEffect, skinList[skinId].GetComponent<PositionsHolder>().effectPos);
        coinRef = Instantiate(coinEffect);

        //References to instantiated effects
        crashRef.Pause();
        bladeRef.Pause();
        coinRef.Pause();
    }

    void Start()
    {
        //Debug.Log(PlayerPrefs.GetInt("BodySkin_ID"));

        thisRB.useGravity = false;
        bladeRef.Play();

        //Run various checks
        if (crystalEffectDisappearTime == 0) crystalEffectDisappearTime = 0.04f;
        if (crashEffectDisappearTime == 0) crashEffectDisappearTime = 0.02f;

        //Find winning crystal on the level
        gemRef = GameObject.Find("Crystal7").transform;
    }

    void Update()
    {
        pos.y = Mathf.Clamp(pos.y, 0, 1);

        velocity = thisRB.velocity;
        pos = transform.position;

        //if (!flying && !GameController.Instance.endGame) StopPlayer();

        //Dashing trigger
        if (dashing && !attached && !GameController.Instance.paused)
        {
            //swordPos.rotation = new Quaternion(0, 0, 0, 0);
            thisRB.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            Move();
            Quaternion lookRotation = Quaternion.LookRotation(Dir());
            //Vector3 rotation = Quaternion.Lerp(crashRef.transform.rotation, lookRotation, 1).eulerAngles;
            transform.rotation = lookRotation;

            //Flags
            dashing = false;
            attached = false;

            //Stopping particles
            crashRef.Stop();
            bladeRef.Stop();
        }

        if(flying)
        {
            playerAnimator.SetBool("dashing", true);
            playerAnimator.SetBool("attached", false);
        }

        if(vibrate)
        {
            Handheld.Vibrate();
            vibrate = false;
        }

        if(attached && !GameController.Instance.endGame)
        {
            StopPlayer();
        }

        if(GameController.Instance.paused && !GameController.Instance.endGame)
        {
            StopPlayer();
        }

        if(GameController.Instance.paused)
        {
            StopEffects();
        }
    }

    void FixedUpdate()
    {
        //To fix the bug with NO collision on idle (Just pushes player on and off all the time to activate collision)
        if(pos.y > 0)
        {
            thisRB.AddForce(new Vector3(0, -0.5f, 0), ForceMode.Impulse);

        } else
        {
            thisRB.AddForce(new Vector3(0, 0.5f, 0), ForceMode.Impulse);
        }
    }

    void LateUpdate()
    {
        if(GameController.Instance.paused && !GameController.Instance.endGame)
        {
            StopPlayer();
        }
        
        //Find winning crystal on the level
        if (gemRef == null)
        {
            //Find gem
            gemRef = GameObject.Find("Crystal7").transform;
            //Assign it a random color
            gemRef.GetComponent<MeshRenderer>().material.SetColor("_Color", GameController.Instance.gemColors[Random.Range(0, GameController.Instance.gemColors.Count)]);
        }
    }
    /// <summary>
    /// Get the default starting position
    /// </summary>
    public Vector3 GetStartingPosition()
    {
        return startingPlayerPosition;
    }

    /// <summary>
    /// Set new starting position
    /// </summary>
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
        //Vibrator.Vibrate(200);
        //Handheld.Vibrate();
        //LEAVE IT FOR EFFECTS
        crashRef.Play();
        vibrate = true;
        StopPlayer();
        if(coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(crystalEffectDisappearTime));
        
        bladeRef.Play();
        //Debug.Log(collision.collider.name);
        //Debug.Log("Collision");

        //If player collided with obstacle
        if (collision.gameObject.GetComponent<ObstacleWallController>())
        {
            playerAnimator.SetBool("death", true);
            playerAnimator.SetBool("dashing", false);
            playerAnimator.SetBool("attached", false);

            if (coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(crystalEffectDisappearTime));
            //StartCoroutine(StopCrashEffect(crashEffectDisappearTime));
            StopEffects();
            StopPlayer();
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

            if (coinRef.isPlaying) StartCoroutine(StopCoinAfterSomeTime(crystalEffectDisappearTime));
        }

        //To track the previous collision
        prevCol = collision;

        //crashRef.transform.position = collision.collider.ClosestPoint(transform.position);

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
        thisRB.AddForce(-Dir() * 1, ForceMode.Impulse);

        //StartCoroutine(StopCrashEffect(crashEffectDisappearTime));

        //Flags
        flying = false;
        attached = true;

        //Don't Play dashing animation
        playerAnimator.SetBool("dashing", false);
        playerAnimator.SetBool("attached", true);

        StopPlayer();
        //Vibrator.Cansel();
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
            StartCoroutine(StopCoinAfterSomeTime(crystalEffectDisappearTime));
        }

        //If collided with finishlane
        if (other.gameObject.GetComponent<NextLevel>())
        {
            //Assing the activated sword the right position
            swordRef.transform.SetParent(beltPos);
            swordRef.transform.rotation = beltPos.rotation;
            swordRef.transform.position = beltPos.position;

            //Player Winning 
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
            gemRef.position = gemPos.transform.position;
            gemRef.rotation = gemPos.transform.rotation;
        }

        //If player reached finish lane
        if(other.gameObject.name == "Blockade")
        {
            playerControllsBlocked = true;
        }
    }
    #endregion

    IEnumerator StopCoinAfterSomeTime(float _time)
    {
        yield return new WaitForSeconds(_time);
        coinRef.Stop();
    }

    IEnumerator StopCrashEffect(float _time)
    {
        yield return new WaitForSeconds(_time);
        crashRef.Stop();
    }

    //Reset player animation back to "Idle"
    public void ResetPlayer()
    {
        StopPlayer();
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.position = GetStartingPosition();

        StopAnimations();
        StopEffects();

        //Reset coinRef position back to player
        coinRef.transform.SetParent(gameObject.transform);

        bladeRef.Play();
        playerAnimator.SetBool("pickUp", false);
       
        playerControllsBlocked = false;
        flying = false;
        attached = true;

        //Assing the activated sword the right position
        swordRef.transform.SetParent(swordPos);
        swordRef.transform.rotation = swordPos.rotation;
        swordRef.transform.position = swordPos.position;
        swordRef.transform.localScale = swordPos.localScale;

        //swordRef.transform.position = swordPos.position;
        //Assing the activated sword the right position
        //swordRef.transform.SetParent(swordPos);
        //swordRef.transform.rotation = swordPos.rotation;
        //swordPos.localRotation =  new Quaternion(0, 30, 90, 0);
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
        transform.LookAt(Vector3.zero);

        for(float i = 0; i < 4; i += 0.5f)
        {
            thisRB.AddForce(dir.normalized, ForceMode.Impulse);
        }
    }

    IEnumerator VibrateOverTime(float _time)
    {
        yield return new WaitForSeconds(_time);
        vibrate = false;
    }
}
