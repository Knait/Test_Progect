using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TrubaGenerator : MonoBehaviour
{
    #region Sigleton
    private static TrubaGenerator instance;
    public static TrubaGenerator Instance => instance;
    #endregion

    [Header("Parameters")]


    [Header("References")]
    //Truba prefabs
    [SerializeField] private GameObject startTruba;
    [SerializeField] private GameObject finishLane;
    [SerializeField] private GameObject bottom;
    [SerializeField] private List<GameObject> trubaList = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    //Level generator
    public void GenerateLevel(int _tubeAmount)
    {
        //If the array is empty, generate error message
        if (trubaList.Count > 0)
        {
            //References
            GameObject previosObj = null;
            GameObject currentObj = null;
            Vector3 pos = new Vector3(0, 0, 0);
            Quaternion rot = Quaternion.Euler(0, 0, 0);

            //Determine how much tubes to spawn (plus starting tube)
            for (int i = 0; i < _tubeAmount + 1; i++)
            {
                //Static initial spawn at the start
                if (i == 0)
                {
                    currentObj = Instantiate(startTruba, pos, rot);
                    previosObj = currentObj;
                    //To add just created object to game manager array
                    GameController.Instance.inGameTubes.Add(currentObj);
                }
                else //Or spawn relative to previous spawned object
                {
                    //Set of angles to choose from (to avoid visual artifacts)
                    int[] angles = {90, 180, 270, 360 };
                    //Hardcoded offset for now - need to change
                    pos = new Vector3(0, previosObj.transform.position.y - 90f, 0);

                    rot = Quaternion.Euler(0, angles[Random.Range(0, angles.Length)], 0);

                    //Holder of generated random gameObject for name switch
                    var nameHolder = randomObjFromList(trubaList, previosObj);

                    //Instantiating
                    currentObj = Instantiate(nameHolder, pos, rot);

                    //To compare names later
                    currentObj.name = nameHolder.name;

                    //For offset
                    previosObj = currentObj;

                    //To add just created object to game manager array
                    GameController.Instance.inGameTubes.Add(currentObj);
                }

                //Last tube
                if (i == _tubeAmount)
                {
                    //Instantiating a finish lane
                    pos = new Vector3(0, previosObj.transform.position.y - 45f, 0);
                    var tempObj = Instantiate(finishLane, pos, rot, previosObj.transform);
                    
                    //Two tubes
                    for(int k = 0; k < 2; k++)
                    {
                        pos = new Vector3(0, previosObj.transform.position.y - 90f, 0);
                        var lastTube = Instantiate(startTruba, pos, rot);
                        GameController.Instance.inGameTubes.Add(lastTube);
                        previosObj = lastTube;
                    }

                    pos = new Vector3(0, previosObj.transform.position.y - 45f, 0);
                    var lastObj = Instantiate(bottom, pos, rot, previosObj.transform);
                }
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

    //Delete everytube in the list, then clear it
    public void ClearLevel(List<GameObject> _current)
    {
        for (int i = 0; i < _current.Count; i++)
        {
            Destroy(_current[i]);
        }

        _current.Clear();
    }

    //Reset the position of current level
    public void ResetLevel()
    {
        Vector3 _pos = new Vector3(0, 0, 0);
        GameObject _prevObject = null;

        for(int i = 0; i < GameController.Instance.inGameTubes.Count; i++)
        {
            //First tube
            if (i == 0)
            {
                GameController.Instance.inGameTubes[i].transform.position = new Vector3(0, _pos.y, 0);
                _prevObject = GameController.Instance.inGameTubes[i];

            } else //The rest
            {
                GameController.Instance.inGameTubes[i].transform.position = new Vector3(0, _prevObject.transform.position.y - 90f, 0);
                _prevObject = GameController.Instance.inGameTubes[i];
            }
        }
    }
}
