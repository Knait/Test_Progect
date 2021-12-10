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
    [SerializeField] private float RangeOffset;

    [Header("References")]
    //References
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    //Holder for previous Object
    private Transform previousObj;
    //A list of corners of tube
    private List<Vector2> Corners = new List<Vector2>();
    void Start()
    {
        //Adding 4 "corners" of a tube
        Vector2 frstCorner = new Vector2(-XRange + RangeOffset, ZRange - RangeOffset);
        Corners.Add(frstCorner);
        Vector2 scndCorner = new Vector2(XRange - RangeOffset, ZRange - RangeOffset);
        Corners.Add(scndCorner);
        Vector2 thrdCorner = new Vector2(XRange - RangeOffset, -ZRange + RangeOffset);
        Corners.Add(thrdCorner);
        Vector2 frthCorner = new Vector2(-XRange + RangeOffset, -ZRange + RangeOffset);
        Corners.Add(frthCorner);

        for (int i = 0; i <= Obstacles; i++)
        {
            //Random values on the range
            //float xVal = Random.Range(-XRange, XRange);
            //float zVal = Random.Range(-ZRange, ZRange);
            Debug.Log(Corners.Capacity);

            //Position 
            Vector3 pos;

            //If there is no previously spwaned object => random spawn, else, offset.
            if (!previousObj)
            {
                pos = new Vector3(Corners[Random.Range(0, Corners.Capacity)].x, startPoint.position.y - YOffset, Corners[Random.Range(0, Corners.Capacity)].y);
            }
            else
            {
                pos = new Vector3(Corners[Random.Range(0, Corners.Capacity)].x, previousObj.position.y - YOffset, Corners[Random.Range(0, Corners.Capacity)].y);
            }

            //Rotation
            Quaternion rot = Quaternion.Euler(0f, 0f, 0f);

            GameObject obj = Instantiate(prefab, pos, rot);

            previousObj = obj.transform;
        }
    }
}
