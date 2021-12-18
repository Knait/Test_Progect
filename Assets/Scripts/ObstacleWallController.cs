using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleWallController : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        GameController.Instance.death = true;
    }
}
