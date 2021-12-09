using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Could use the singleton
    public Joystick joystick;
    [SerializeField] private float speed;

    private Rigidbody thisRB;

    void Start()
    {
        thisRB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        thisRB.AddForce(Dir() * speed);

        /* Without RigidBody
        Vector3 pos = transform.position;

        pos += Dir() * speed * Time.deltaTime;

        transform.position = pos;
        */
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
