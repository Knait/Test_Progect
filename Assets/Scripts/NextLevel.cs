using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextLevel : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Next Level");
        GameController.Instance.win = true;
    }
}
