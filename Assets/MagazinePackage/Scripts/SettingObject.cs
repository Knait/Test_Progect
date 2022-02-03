using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateProduct
{
    Locked,
    Use,
    InUse,
    Price
}

public class SettingObject : MonoBehaviour
{
    [Header("ID Object Product")]
    public int objectID;
    [Header("Object Product")]
    public GameObject objectProduct;
    [Header("State Product")]
    public StateProduct stateProduct;
    [Header("PriceProduct")]
    public int priceProduct;

    private Vector3 vectorRotateObject = new Vector3(0.0f, 10.0f, 0.0f);
    private Transform pointObjectProduct;
    private TextMesh textMesh;

    [SerializeField] public bool skin = true;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMesh>();
        pointObjectProduct = transform.Find("PointObject");

        if (objectProduct != null)
        {
            objectProduct.transform.position = pointObjectProduct.transform.position;
        }
    }

    void Update()
    {
        RotateObject();
        UpdateText();
    }

    [SerializeField] private GameObject particle;
    public void SetNewInUseObj()
    {
        //particle.SetActive(true);

        if (skin)
            PlayerPrefs.SetInt("BodySkin_ID", objectID);
        else
            PlayerPrefs.SetInt("SwordSkin_ID", objectID);

        PlayerPrefs.Save();
    }

    public void DeactiveParticle(SettingObject obj)
    {
        //if (skin == obj.skin)
            //particle.SetActive(false);
    }

    /// <summary>
    /// ���������� ������ �� �������
    /// </summary>
    //Updates the text on Bars
    void UpdateText()
    {
        string currentStateProduct;

        if (stateProduct == StateProduct.Price)
        {
            currentStateProduct = priceProduct.ToString();
        }
        else
        {
            currentStateProduct = stateProduct.ToString();
        }
        textMesh.text = currentStateProduct;
    }

    /// <summary>
    /// �������� ������� ��� �������
    /// </summary>
    void RotateObject()
    {
        if (objectProduct != null)
        {
            objectProduct.transform.Rotate(vectorRotateObject * Time.deltaTime);
        }
    }

   
}
