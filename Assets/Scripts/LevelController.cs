using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    //[SerializeField] private float levelGameTime = 15;

    /*void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Next Level");
        collision.transform.position = new Vector3(0, 0, 0);
    }*/

    [HideInInspector] public float levelSpeed;

    public void Update()
    {
        transform.Translate(new Vector3(0, levelSpeed, 0) * Time.deltaTime);
    }
}
