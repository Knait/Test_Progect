using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class FSJoystickStick : MonoBehaviour, IDragHandler,
    IPointerDownHandler, IPointerUpHandler
{

    private static FSJoystickStick instance;
    public static FSJoystickStick Instance => instance;

    [SerializeField] private bool useVerticalAxies = true;
    [SerializeField] private bool useHorizontalAxis = true;

    [SerializeField] private Image tr_plane;
    [SerializeField] private Image tr_Joystick;
    [SerializeField] private Image tr_Stick;
    private Vector2 inputVector;

    [Header("PlayerClass")]
    [SerializeField] private FlySerferController player;

    //[HideInInspector] public UnityEvent<Vector2> stickPosition = new UnityEvent<Vector2>();
    public Vector2 _stickPos;
    public Vector2 startJoystickPos;

    private void Awake()
    {
        instance = this;
        startJoystickPos = tr_Joystick.rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 stickPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tr_plane.rectTransform, eventData.position, eventData.pressEventCamera, out stickPos))
            tr_Joystick.rectTransform.anchoredPosition = stickPos;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 stickPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tr_Joystick.rectTransform, eventData.position, eventData.pressEventCamera, out stickPos))
        {
            if (useHorizontalAxis) stickPos.x = stickPos.x / tr_Joystick.rectTransform.sizeDelta.x * 2;
            if (useVerticalAxies) stickPos.y = stickPos.y / tr_Joystick.rectTransform.sizeDelta.y * 2;
        }

        _stickPos = stickPos;
        inputVector = new Vector2(stickPos.x, stickPos.y);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
        tr_Stick.rectTransform.anchoredPosition = new Vector2(inputVector.x * (-tr_Stick.rectTransform.sizeDelta.x), inputVector.y * (-tr_Stick.rectTransform.sizeDelta.y));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        player.SetShesh(Mathf.Clamp(inputVector.x, -1f, 1f), Mathf.Clamp(inputVector.y, -1f, 1f));

        tr_Joystick.rectTransform.anchoredPosition = startJoystickPos;
        inputVector = Vector2.zero;
        _stickPos = Vector2.zero;
        tr_Stick.rectTransform.anchoredPosition = Vector2.zero;
    }

    public float HorizontalAxis()
    {
        return Mathf.Clamp(inputVector.x, -1f, 1f);
    }

    public float VerticalAxis()
    {
        return Mathf.Clamp(inputVector.y, -1f, 1f);
    }

}
