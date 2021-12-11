using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleWallController : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("YOU ARE DEAD");
        collision.transform.position = new Vector3(0, 0, 0);

        GameController.Instance.death = true;
    }
}
