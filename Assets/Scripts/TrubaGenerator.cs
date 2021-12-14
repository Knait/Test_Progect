using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrubaGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> trubaList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        if(trubaList != null)
        {
            Debug.Log("Generating Levels");

        } else
        {
            Debug.LogError("Can't find any tubes to spawn. Please fill out the array.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
