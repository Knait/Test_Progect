using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleWallController : MonoBehaviour
{
    MeshRenderer thisMesh;
    Material[] thisMats;

    bool faded = false;
    float fadeAmount;
    void Start()
    {
        thisMesh = GetComponentInChildren<MeshRenderer>();
        thisMats = thisMesh.materials;
    }

    void Update()
    {
        if (transform.position.y > 0 && !faded && !GameController.Instance.paused)
        {
            StartCoroutine(startFading());
            faded = true;
        }

        if(faded && GameController.Instance.paused)
        {
            for (int i = 0; i < thisMats.Length; i++)
            {
                Color temp = thisMats[i].color;
                temp.a = 255;

                thisMats[i].color = temp;
            }

            faded = false;
        }
    }

    IEnumerator startFading()
    {
        //The speed of fading (default 0.05f)
        if (GameController.Instance.fadeSpeed != 0) fadeAmount = GameController.Instance.fadeSpeed; else fadeAmount = 0.05f;

        for (int i = 0; i < thisMats.Length; i++)
        {
            for (float f = 1; f >= -fadeAmount; f -= fadeAmount)
            {
                Color temp = thisMats[i].color;
                temp.a = f;

                thisMats[i].color = temp;

                yield return null;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameController.Instance.death = true;
    }
}
