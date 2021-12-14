using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrubaGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> trubaList = new List<GameObject>();

    //TODO: SYNCHRONIZE LEVEL SPEED WITH GLOBAL VARIABLE FROM GAME MANAGER
    //TODO: GENERATE TUBES AT RANDOM! WITHOUTH REPEATING
    void Start()
    {
        //If the array is empty, generate error message
        if(trubaList.Capacity > 0)
        {
            Debug.Log("Generating Levels");
            GameObject previosObj;

            for (int i = 0; i < trubaList.Capacity; i++)
            {
                var obj = Instantiate(trubaList[i]);
                previosObj = obj;

                //Static initial spawn at the start
                if (i == 0)
                {
                    obj.transform.position = new Vector3(0, -42, 0);
                }
                else //Or spawn relative to previous spawned object
                {
                    obj.transform.position = new Vector3(0, previosObj.transform.position.y - 1f, 0);
                }
            }

        } else
        {
            Debug.LogError("Can't find any tubes to spawn. Please fill out the array.");
        }
    }
}
