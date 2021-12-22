using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        GameController.Instance.NextLevel();
    }
}
