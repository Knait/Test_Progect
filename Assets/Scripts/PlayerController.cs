using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Could use the singleton
    //Need to rewrite joystick then
    public Joystick joystick;
    [SerializeField] private float speed;
    [SerializeField] private float dashRate;
    [SerializeField] private float restBetweenDash;
    [HideInInspector] public bool dashing;

    private Rigidbody thisRB;

    void Start()
    {
        thisRB = GetComponent<Rigidbody>();
        dashing = false;
    }

    void Update()
    {
        //Move();

        //Cooldown
        if(restBetweenDash > 0) restBetweenDash -= Time.deltaTime;

        //If cooldown is 0 and the joystick is pressed
        if(restBetweenDash <= 0 && dashing)
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
        //Push the player to the opposite direction
        thisRB.AddForce(-1 * Dir() * speed, ForceMode.Impulse);
    }
}
