using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrubaGenerator : MonoBehaviour
{
    #region Sigleton
    private static TrubaGenerator instance;
    public static TrubaGenerator Instance => instance;
    #endregion

    [Header("References")]
    //Truba prefabs
    [SerializeField] private GameObject startTruba;
    [SerializeField] private GameObject finishLane;
    [SerializeField] private List<GameObject> trubaList = new List<GameObject>();

    [Header("Materials")]
    //Materials
    [SerializeField] private Material firstLevelM;
    [SerializeField] private Material secondLevelM;
    [SerializeField] private Material thirdLevelM;

    [Header("Parameters")]
    [SerializeField] private int pipesPerLevel;

    void Start()
    {
        instance = this;
    }

    //Level generator
    public void generateLevel(int _gameLevel)
    {
        if (!startTruba) Debug.LogError("Can't find starting truba.");
        //If the array is empty, generate error message
        if (trubaList.Count > 0)
        {
            //References
            GameObject previosObj = null;
            GameObject currentObj = null;
            Vector3 pos = new Vector3(0, 0, 0);
            Quaternion rot = Quaternion.Euler(0, 0, 0);

            //Determine how much tubes to spawn
            for (int i = 0; i < pipesPerLevel + 1; i++)
            {
                //Static initial spawn at the start
                if (i == 0)
                {
                    currentObj = Instantiate(startTruba, pos, rot);
                    previosObj = currentObj;
                }
                else //Or spawn relative to previous spawned object
                {
                    //Hardcoded offset for now - need to change
                    pos = new Vector3(0, previosObj.transform.position.y - 90f, 0);
                    rot = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    //Holder of generated random gameObject for name switch
                    var nameHolder = randomObjFromList(trubaList, previosObj);

                    //Instantiating
                    currentObj = Instantiate(nameHolder, pos, rot);

                    //To compare names later
                    currentObj.name = nameHolder.name;

                    //For offset
                    previosObj = currentObj;
                }

                //Last tube
                if (i == pipesPerLevel)
                {
                    //Instantiating a finish lane
                    pos = new Vector3(0, previosObj.transform.position.y - 45f, 0);
                    var tempObj = Instantiate(finishLane, pos, rot);
                }

                //Try to get to the LevelController of the tube to set the speed
                if (currentObj.TryGetComponent(out LevelController controller))
                {
                    controller.levelSpeed = GameController.Instance.getSpeed();
                }

                //To add just created object to game manager array
                GameController.Instance.inGameTubes.Add(currentObj);
            }

        }
        else
        {
            Debug.LogError("Can't find any tubes to spawn. Please fill out the array.");
        }
    }

    //Takes list of tubes and previously spawned object, and if the new generated tube has different name - return gameobject
    private GameObject randomObjFromList(List<GameObject> _list, GameObject _prevObj)
    {
        //An object for comparison
        GameObject tempObj = _list[Random.Range(0, trubaList.Count)];

        //If the names are identical, cycle through the list until the name is different
        while (tempObj.name == _prevObj.name) tempObj = _list[Random.Range(0, trubaList.Count)];

        return tempObj;
    }
}
