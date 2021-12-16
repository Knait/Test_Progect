using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrubaGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> trubaList = new List<GameObject>();

    //TODO: GENERATE TUBES AT RANDOM! WITHOUTH REPEATING (check the name
    void Start()
    {
        //If the array is empty, generate error message
        if(trubaList.Count > 0)
        {
            Debug.Log("Generating Levels");
            GameObject previosObj = null;
            Vector3 pos = new Vector3(0,0,0);
            Quaternion rot = Quaternion.Euler(0,0,0);

            //Determine how much tubes to spawn
            for (int i = 0; i < trubaList.Count; i++)
            {
                //Static initial spawn at the start
                if (i == 0)
                {
                    pos = new Vector3(0, -45f, 0);
                }
                else //Or spawn relative to previous spawned object
                {
                    //Hardcoded offset for now - need to change
                    pos = new Vector3(0, previosObj.transform.position.y - 90f, 0);
                }

                var obj = Instantiate(trubaList[i], pos, rot);
                previosObj = obj;

                //Try to get to the LevelController of the tube
                if(obj.TryGetComponent(out LevelController controller))
                {
                    controller.levelSpeed = GameController.Instance.getSpeed();
                }

                GameController.Instance.inGameTubes.Add(obj);
            }

        } else
        {
            Debug.LogError("Can't find any tubes to spawn. Please fill out the array.");
        }
    }
}
