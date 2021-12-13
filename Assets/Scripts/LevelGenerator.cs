using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Parameters")] 
    //Number of obstacles
    [SerializeField] private int ObstaclesAmount;
    //Distance between obstacles
    [SerializeField] private float YOffset;
    //Range of the tube
    [SerializeField] private float XRange;
    [SerializeField] private float ZRange;
    //The offset of corners of tube
    [SerializeField] private float RangeOffset;
    //Number of coins
    [SerializeField] private int CoinAmount;
    //The offset of coin position to obstacle position
    [SerializeField] private float CoinOffset;
    //A coin spawning chance
    [SerializeField] private float CoinChance;

    [Header("References")]
    //References
    [SerializeField] private GameObject ObstaclePrefab;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject trubaParent;
    //Holder for previous Object
    private Transform previousObj;
    //A list of corners of tube
    private List<Vector2> Corners = new List<Vector2>();
    void Start()
    {
        //Adding 4 "corners" of a tube
        Vector2 frstCorner = new Vector2(-XRange + RangeOffset, ZRange - RangeOffset); //
        Corners.Add(frstCorner);
        Vector2 scndCorner = new Vector2(XRange - RangeOffset, ZRange - RangeOffset); //
        Corners.Add(scndCorner);
        Vector2 thrdCorner = new Vector2(XRange - RangeOffset, -ZRange + RangeOffset);
        Corners.Add(thrdCorner);
        Vector2 frthCorner = new Vector2(-XRange + RangeOffset, -ZRange + RangeOffset);
        Corners.Add(frthCorner);

        for (int i = 0; i <= ObstaclesAmount; i++)
        {
            #region ObstacleSpawner

            //Random values on the range
            //float xVal = Random.Range(-XRange, XRange);
            //float zVal = Random.Range(-ZRange, ZRange);

            //Obstacle position 
            Vector3 ObstPos;
            //Coint position
            Vector3 CoinPos;

            //If there is no previously spwaned object => spawn with offset from start point, else, offset from previous.
            if (!previousObj)
            {
                ObstPos = new Vector3(Corners[Random.Range(0, Corners.Capacity)].x, startPoint.position.y - YOffset, Corners[Random.Range(0, Corners.Capacity)].y);
            }
            else
            {
                ObstPos = new Vector3(Corners[Random.Range(0, Corners.Capacity)].x, previousObj.position.y - YOffset, Corners[Random.Range(0, Corners.Capacity)].y);
            }

            //Obstacle Rotation
            Quaternion rot = Quaternion.Euler(0f, 0f, 0f);

            GameObject ObstObj = Instantiate(ObstaclePrefab, ObstPos, rot);
            ObstObj.transform.SetParent(trubaParent.transform);
            
            //Hardcoded offset
            CoinPos = new Vector3(ObstPos.x, ObstPos.y + CoinOffset, ObstPos.z);

            GameObject CoinObj = Instantiate(CoinPrefab, CoinPos, rot);
            CoinObj.transform.SetParent(trubaParent.transform);
            CoinAmount++;

            previousObj = ObstObj.transform;

            #endregion
        }
    }
}
