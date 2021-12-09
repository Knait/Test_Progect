using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    private static Joystick instance;
    public static Joystick Instance => instance;

    [SerializeField] private Image img_Joystick;
    [SerializeField] private Image img_Stick;

    [SerializeField] private PlayerController player;

    private Vector3 inputVector;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(img_Joystick.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            // Stick restrictions
            pos.x = (pos.x / img_Joystick.rectTransform.sizeDelta.x);
            pos.y = (pos.y / img_Joystick.rectTransform.sizeDelta.y);

            // To make the pos go above 0
            inputVector = new Vector3(pos.x * 2 + 1, 0, pos.y * 2 - 1);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Moving joystick img
            img_Stick.rectTransform.anchoredPosition = new Vector3(inputVector.x * (img_Joystick.rectTransform.sizeDelta.x / 2), inputVector.z * (img_Joystick.rectTransform.sizeDelta.y / 2));

            //Debug.Log(inputVector);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        img_Stick.rectTransform.anchoredPosition = Vector3.zero;
    }

    // For player movement
    public float Horizontal()
    {
        return inputVector.x;
    }
    public float Vertical()
    {
        return inputVector.z;
    }
}
