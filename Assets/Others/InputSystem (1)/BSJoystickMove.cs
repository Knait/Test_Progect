using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class BSJoystickMove : MonoBehaviour, IDragHandler,
    IPointerDownHandler, IPointerUpHandler
{

    private static BSJoystickMove instance;
    public static BSJoystickMove Instance => instance;

    [SerializeField] private bool useVerticalAxies = true;
    [SerializeField] private bool useHorizontalAxis = true;
    [SerializeField] private Image tr_Joystick;
    [SerializeField] private Image tr_Stick;
    private Vector2 inputVector;

    [HideInInspector] public UnityEvent<Vector2> stickPosition = new UnityEvent<Vector2>();

    private void Awake()
    {
        instance = this;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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

        inputVector = new Vector2(stickPos.x, stickPos.y);
        inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
        tr_Stick.rectTransform.anchoredPosition = new Vector2(inputVector.x * (-tr_Stick.rectTransform.sizeDelta.x), inputVector.y * (-tr_Stick.rectTransform.sizeDelta.y));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
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