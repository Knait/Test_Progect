using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Joystick : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    private static Joystick instance;
    public static Joystick Instance => instance;

    [SerializeField] private Image img_Panel;
    [SerializeField] private Image img_Joystick;
    [SerializeField] private Image img_Stick;

    private Vector3 inputVector;
    public Vector2 _stickPos;
    //Test
    private bool dragging = false;

    void Awake()
    {
        instance = this;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(img_Joystick.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {

            dragging = true;

            // Stick restrictions
            pos.x = (pos.x /img_Joystick.rectTransform.sizeDelta.x);
            pos.y = (pos.y / img_Joystick.rectTransform.sizeDelta.y);

            // To make the pos go above 0
            inputVector = new Vector3(pos.x * 2 + 1, 0, pos.y * 2 - 1);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Moving joystick img
            img_Stick.rectTransform.anchoredPosition = new Vector3(inputVector.x * (img_Joystick.rectTransform.sizeDelta.x / 2), inputVector.z * (img_Joystick.rectTransform.sizeDelta.y / 2));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 stickPos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(img_Panel.rectTransform, eventData.position, eventData.pressEventCamera, out stickPos))
            img_Joystick.rectTransform.anchoredPosition = stickPos;
        //Clickin bug!
        //OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (dragging)
        {
            PlayerController.Instance.dashing = true;
            dragging = false;
        }
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
