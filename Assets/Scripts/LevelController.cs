using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public float levelSpeed;
    public bool moving = false;

    public void Update()
    {
        if(moving) transform.Translate(new Vector3(0, levelSpeed, 0) * Time.deltaTime);
    }
}
