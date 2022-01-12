using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutTest : MonoBehaviour
{
    MeshRenderer thisMesh;
    Material[] thisMats;

    bool faded = false;
    void Start()
    {
        thisMesh = GetComponentInChildren<MeshRenderer>();
        thisMats = thisMesh.materials;
    }

    void Update()
    {
        if(transform.position.y > 0 && !faded)
        {
            Debug.Log("Just Pased Player!");
            StartCoroutine(startFading());
            faded = true;
        }
    }

    IEnumerator startFading()
    {
        float fadeAmount = 0.05f;

        for (int i = 0; i < thisMats.Length; i++)
        {
            for (float f = 1; f >= -0.05f; f -= fadeAmount)
            {
                Color temp = thisMats[i].color;
                temp.a = f;

                thisMats[i].color = temp;
                yield return new WaitForSeconds(fadeAmount);
            }
        }
    }
}
