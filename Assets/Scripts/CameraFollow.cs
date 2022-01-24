using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Static
    public static CameraFollow cam;

    public Transform target;

    public float timeBeforeCamera;

    //Standard offset from player
    public Vector3 classicOffset;
    //Finishing offset
    public Vector3 finishOffset;

    void Awake()
    {
        cam = this;
    }
    // Late update to make camera movement smooth
    void LateUpdate()
    {
        if(!GameController.Instance.paused) FollowingPlayer();
        //Debug.Log(GameController.Instance.win);
        //if(GameController.Instance.paused && GameController.Instance.win) PlayerShowcase();
    }

    void FollowingPlayer()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x / 1.7f, target.position.y + classicOffset.y, target.position.z / 1.7f);
        }
    }

    public IEnumerator PlayerShowcase()
    {
        //transform.position = new Vector3(0, 0, 0);

        //var dir = target.position - gameObject.transform.position;
        yield return new WaitForSeconds(timeBeforeCamera);

        transform.position = new Vector3(0 - target.position.x, 0 + finishOffset.y, 0 - target.position.z);
        transform.LookAt(target);
    }

    //Reset camera position and rotation
    public void ResetCamera()
    {
        transform.position = Vector3.zero;
        transform.localRotation = Quaternion.Euler(90, 0, 0);
    }
}