using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleWallController : MonoBehaviour
{
    //An array of MeshRenderers of objects
    public MeshRenderer[] thisMesh;
    //A list of materials on MeshRenderers
    public List<Material> matList = new List<Material>();

    //public Collider[] collidersOnThisObject;

    //Flag
    public bool faded = false;
    //The speed of fading
    float fadeAmount;

    void Start()
    {
        //Add all of the MeshRenderers in this object's children
        thisMesh = GetComponentsInChildren<MeshRenderer>();
        //Add all of the Colliders on this object
        //collidersOnThisObject = GetComponents<Collider>();

        //Cycle through these MeshRenderers, and add their Materials to the list
        for (int i = 0; i < thisMesh.Length; i++)
        {
            matList.Add(thisMesh[i].material);
        }
    }

    void Update()
    {
        //A condition for an object to start fading
        if (transform.position.y > 0 && !faded && !GameController.Instance.paused)
        {
            faded = true;

            /*for(int i = 0; i < collidersOnThisObject.Length; i++)
            {
                collidersOnThisObject[i].enabled = false;
            }*/

            StartCoroutine(startFading());
        }

        //A condition for an object to be visible again
        if(faded && GetComponentInParent<Transform>().position.y < 0)
        {
            for (int i = 0; i < matList.Count; i++)
            {
                Color temp = matList[i].color;
                temp.a = 255;

                matList[i].color = temp;
            }

            /*for (int i = 0; i < collidersOnThisObject.Length; i++)
            {
                collidersOnThisObject[i].enabled = true;
            }*/

            faded = false;
        }
    }

    //Fading function
    IEnumerator startFading()
    {
        //The speed of fading (default 0.05f)
        if (GameController.Instance.fadeSpeed != 0) fadeAmount = GameController.Instance.fadeSpeed; else fadeAmount = 0.05f;

        for (int i = 0; i < matList.Count; i++)
        {
            for (float f = 1; f >= -fadeAmount; f -= fadeAmount)
            {
                Color temp = matList[i].color;
                temp.a = f;

                matList[i].color = temp;

                yield return null;
            }
        }
    }

    //A collision function, if player collides
    void OnCollisionEnter(Collision collision)
    {
        GameController.Instance.death = true;
    }
}
