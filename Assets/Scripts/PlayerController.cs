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
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem moveEffect;

    [Header("DON'T CHANGE")]
    [SerializeField] private Vector3 velocity;

    [HideInInspector] public bool dashing = false;

    //References
    private Rigidbody thisRB;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        thisRB = GetComponent<Rigidbody>();
        thisRB.useGravity = false;

        if (!dashEffect || !moveEffect) Debug.LogError("Can't find particles! Please add them in the inspector.");
    }

    void Update()
    {
        if (dashing == true)
        {
            Move();
            dashEffect.Play();
            dashing = false;
        }

        moveEffect.Play();
        velocity = GetComponent<Rigidbody>().velocity;
    }
    
    //Actual movement of the player
    public void Move()
    {
        thisRB.velocity = new Vector3(0, 0, 0);
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
        //Push the player to the opposite direction
        thisRB.AddForce(-1 * Dir() * 0.2f, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        //If collided with coin
        if (other.gameObject.GetComponentInParent<Coin>())
        {
            GameController.Instance.score++;
            Destroy(other.gameObject);
        }

        if (other.gameObject.GetComponent<NextLevel>())
        {
            GameController.Instance.NextLevel();
        }
    }


}
