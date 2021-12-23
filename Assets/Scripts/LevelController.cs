using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{

    [SerializeField] private float levelSpeed;

    void Start()
    {
        levelSpeed = GameController.Instance.getSpeed();
    }

    public void Update()
    {
        levelSpeed = GameController.Instance.getSpeed();
        transform.Translate(new Vector3(0, levelSpeed, 0) * Time.deltaTime);
    }
}
