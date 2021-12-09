using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Numbers")] 
    //Numbers
    [SerializeField] private int Obstacles;
    [SerializeField] private float YOffset;
    [SerializeField] private float XRange;
    [SerializeField] private float ZRange;

    [Header("References")]
    //References
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    private Transform previousObj;

    void Start()
    {
        for(int i = 0; i < Obstacles; i++)
        {
            //Random values on the range
            float xVal = Random.Range(-XRange, XRange);
            float zVal = Random.Range(-ZRange, ZRange);

            //Position 
            Vector3 pos;

            //If there is no previously spwaned object => random spawn, else, offset.
            if (!previousObj)
            {
                pos = new Vector3(xVal, startPoint.position.y - YOffset, zVal);
            }
            else
            {
                pos = new Vector3(xVal, previousObj.position.y - YOffset, zVal);
            }

            //Rotation
            Quaternion rot = Quaternion.Euler(0f, 0f, 0f);

            GameObject obj = Instantiate(prefab, pos, rot);

            previousObj = obj.transform;
        }
    }
}
