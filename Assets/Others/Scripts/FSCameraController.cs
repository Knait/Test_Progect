using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSCameraController : MonoBehaviour
{
    [SerializeField] private Transform car;
    [SerializeField] private float moveSpeed = 5;
    [SerializeField] private float rotateSpeed = 5;

    private Camera _camera;
    //[SerializeField] private float maxCamFOV = 80;
    private float factCamFOV = 60;

    private bool useTransform = false;

    private void Awake()
    {
        if (!GetComponent<Camera>())
        {
            Debug.LogError("Need Camera Component At Obj");
            return;
        }

        _camera = GetComponent<Camera>();
        factCamFOV = _camera.fieldOfView;

        useTransform = true;
    }

    private void Update()
    {
        if (!useTransform) return;

        MoveCameraAtPos();
    }

    private void MoveCameraAtPos()
    {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, new Vector3(car.position.x, car.position.y + 10, car.position.z), moveSpeed);
        gameObject.transform.LookAt(car, Vector3.forward * rotateSpeed);
    }
}
