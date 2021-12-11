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
    [SerializeField] private float speed;
    [SerializeField] private float dashRate;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;
    public float restBetweenDash;

    [HideInInspector] public bool dashing;

    //References
    private Rigidbody thisRB;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        thisRB = GetComponent<Rigidbody>();
        dashing = false;
    }

    void Update()
    {
        //Move();
        velocity = GetComponent<Rigidbody>().velocity;

        //Cooldown
        if(restBetweenDash > 0) restBetweenDash -= Time.deltaTime;

        //For visual purposes
        if (restBetweenDash < 0)
        {
            restBetweenDash = 0;
        }

        //If cooldown is 0 and the joystick is pressed
        if (restBetweenDash <= 0 && dashing)
        {
            Move();
            dashing = false;
            restBetweenDash = 2f / dashRate;
        }
    }
    
    //Actual movement of the player
    public void Move()
    {
        thisRB.AddForce(Dir() * speed, ForceMode.Impulse);
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

    void OnCollisionEnter(Collision collision)
    {
        //If collided with coin
        if(collision.gameObject.GetComponent<Coin>())
        {
            GameController.Instance.score++;
            restBetweenDash = 0;
            Destroy(collision.gameObject);

        } else
        {
            //Push the player to the opposite direction
            thisRB.AddForce(-1 * Dir() * speed, ForceMode.Impulse);
        }

    }
}
