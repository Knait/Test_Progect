using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveData
{
    // ������ ������� ��� ���������� ��������� ��������� � ������
    public int[] priceProduct = new int[27];
    public StateProduct[] stateProduct = new StateProduct[27];
}


public class MarketManager : MonoBehaviour
{
    private RaycastHit hit;
    private Ray MyRay;
    private GameObject currentObjectInUse;  // ������� ������ ��������
   // [SerializeField]
    private int ID;             // ������� ID ������� ��������
    // [SerializeField]
    // private int LoadID;

    [SerializeField]
    private Transform cameraPosition;   //������
    [SerializeField]
    private Text textAllPoints;         

   // [SerializeField]
    private int AllPoints;  // ������� ������

    [Header("Array Bars")]
    [SerializeField]
    private GameObject[] Bars;           //������ 3d ������ ������

    [Header("Array Sprites Background")]
    [SerializeField]
    private Sprite[] backGround;         // ������ ���� ������

    [SerializeField] private UnityEngine.UI.Button closeButton;

    void Start()
    {
        closeButton.onClick.AddListener(() => ReturnScene(0));

        LoadData();

        if (cameraPosition != null) cameraPos = new Vector3(-7.0f, cameraPosition.position.y, cameraPosition.position.z);

        //������� ������� InUse 
        foreach (GameObject gObject in Bars)
        {
            if (GetStateProduct(gObject) == StateProduct.InUse)
            {
                currentObjectInUse = gObject;           // ����� ������� ������ InUse
                ID = GetIDObject(currentObjectInUse); // ��������� ID �������� InUse

               break;
            }
        }

        if (!PlayerPrefs.HasKey("NewTargetPrice"))
            PlayerPrefs.SetInt("NewTargetPrice", 50);

        foreach (GameObject gObject in Bars)
        {
            if (gObject.GetComponent<SettingObject>().priceProduct > PlayerPrefs.GetInt("allCoins"))
            {
                PlayerPrefs.SetInt("NewTargetPrice", gObject.GetComponent<SettingObject>().priceProduct);
                break;
            }
        }
    }

    void Update()
    {
        UpdateMouse();                    // ��������� ����
        UpdateSpritesBackGround(Bars, backGround);         // ���������� �������� ������
        UpdateTextAllPoints();

        if (cameraPosition != null) cameraPosition.position = Vector3.Lerp(cameraPosition.position, cameraPos, 0.05f);
    }

    /// <summary>
    /// ���������� � ������� �� �����
    /// </summary>
    private void ReturnScene(int indexScene)
    {
        SceneManager.LoadScene(indexScene);    // �������� ���� �����
    }

    /// <summary>
    /// ���������� ������ �� ������� ������ ��� �����
    /// </summary>
    /// <returns></returns>
    private GameObject GetChoiceOBject()
    {
        GameObject result = null;

        MyRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(MyRay.origin, MyRay.direction * 10, Color.yellow);

        if (Physics.Raycast(MyRay, out hit, 200))
        {
            MeshFilter filter = hit.collider.GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (filter)
            {
                result = filter.gameObject;                // ������ �� �������� �������� �����               
            }
        }
        Debug.Log(result);
        return result;
    }

    /// <summary>
    /// �������  ID ������
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    private int GetIDObject(GameObject _gameObject)
    {
        int result = 0;
        result = _gameObject.GetComponent<SettingObject>().objectID;
        _gameObject.GetComponent<SettingObject>().SetNewInUseObj();
        return result;
    }

    /// <summary>
    /// ����� �������� � ������
    /// </summary>
    /// <param name="Object"></param>
    private void ChangeProduct(GameObject gObject)
    {
        StateProduct stateProduct = GetStateProduct(gObject);

        if (stateProduct == StateProduct.InUse || stateProduct == StateProduct.Locked) // ���� ���������� ��� ������������ �� �����
        {
            Debug.Log("StateObject " + stateProduct);
            return;
        }


        if (stateProduct == StateProduct.Price)                                       // ���� ���� ������ �� ��������
        {
            int currentPriceObject = gObject.GetComponent<SettingObject>().priceProduct;

            if (AllPoints >= currentPriceObject)
            {
                AllPoints -= currentPriceObject;
                //gObject.GetComponent<SettingObject>().stateProduct = StateProduct.Use;

                SetStateProduct(gObject, StateProduct.Use);

            }
            Debug.Log("StateObject " + stateProduct);

            UpdateSpritesBackGround(Bars, backGround);         // ���������� �������� ������
            return;
        }

        if (stateProduct == StateProduct.Use)
        {
            SetStateProduct(gObject, StateProduct.InUse);               //���� ���� ������������ �� ���������� 

            if (currentObjectInUse)
            {
                SetStateProduct(currentObjectInUse, StateProduct.Use);
            }
            currentObjectInUse = gObject;                          // ��������� ������� ������ InUse
            ID = GetIDObject(currentObjectInUse);                 // ��������� ID �������� InUse
            UpdateSpritesBackGround(Bars, backGround);         // ���������� �������� ������
        }
    }


    /// <summary>
    /// ������� ��������� ��������
    /// </summary>
    /// <param name="gObject"></param>
    /// <returns></returns>
    private StateProduct GetStateProduct(GameObject gObject)
    {
        StateProduct result;

        result = gObject.GetComponent<SettingObject>().stateProduct;
        return result;
    }

    /// <summary>
    /// ��������� ������� ������
    /// </summary>
    /// <param name="gObject"></param>
    /// <param name="stateProduct"></param>
    private void SetStateProduct(GameObject gObject, StateProduct stateProduct)
    {
        gObject.GetComponent<SettingObject>().stateProduct = stateProduct;
        gObject.GetComponent<SettingObject>().DeactiveParticle(gObject.GetComponent<SettingObject>());
    }

    /// <summary>
    /// ����� �������� ������ �� ������� (�����)
    /// </summary>
    /// <param name="arrayObjects"></param>
    /// <param name="arraySprites"></param>
    void UpdateSpritesBackGround(GameObject[] arrayObjects, Sprite[] arraySprites)
    {
        foreach (GameObject gameObject in arrayObjects)
        {
            int indexArraySprites = (int)gameObject.GetComponent<SettingObject>().stateProduct;

            gameObject.GetComponentInChildren<SpriteRenderer>().sprite = arraySprites[indexArraySprites];

        }

    }

    /// <summary>
    /// �������� ������ ��������
    /// </summary>
    void LoadData()
    {
        string key = "SavedStateProduct";

        if (PlayerPrefs.HasKey(key))
        {
            string value = PlayerPrefs.GetString(key);
            SaveData data = JsonUtility.FromJson<SaveData>(value);

            for (int index = 0; index < Bars.Length; index++)
            {
                Bars[index].GetComponent<SettingObject>().stateProduct = data.stateProduct[index];
                Bars[index].GetComponent<SettingObject>().priceProduct = data.priceProduct[index];

            }
        }

        key = "allCoins";
        if (PlayerPrefs.HasKey(key))
        {
            AllPoints = PlayerPrefs.GetInt("allCoins");
            // LoadID = PlayerPrefs.GetInt("SavedID");
            Debug.Log("Data loaded!");
        }
        else
            Debug.LogError("There is no save data!");
    }

    /// <summary>
    /// ���������� ������ ��������
    /// </summary>
    void SaveData()
    {
        string key = "SavedStateProduct";
        //currentIDInUse = GetIDObject(currentObjectInUse);
        SaveData data = new SaveData();

        for (int index = 0; index < Bars.Length; index++)
        {
            Debug.Log(index);
            data.stateProduct[index] = Bars[index].GetComponent<SettingObject>().stateProduct;
            data.priceProduct[index] = Bars[index].GetComponent<SettingObject>().priceProduct;
        }

        string value = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, value);

        Debug.Log("All points" + AllPoints);
        key = "allCoins";
        PlayerPrefs.SetInt("allCoins", AllPoints);
        //PlayerPrefs.SetInt("SavedID", ID);
        PlayerPrefs.Save();
        Debug.Log("Data saved!");
    }

    /// <summary>
    /// �������� ���� ������
    /// </summary>
    void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        AllPoints = 0;
        Debug.Log("Data reset complete");
    }


    /// <summary>
    /// ��������� ����
    /// </summary>
    void UpdateMouse()
    {
        if (Input.GetMouseButton(0))
        {
            GameObject currentObject = GetChoiceOBject();

            if (currentObject != null)
            {
                string nameObject = currentObject.name;

                if (cameraPosition != null)
                {
                    if (nameObject == "ButtonReturn")
                    {
                        SceneManager.LoadScene(0);
                    }

                    if (nameObject == "ButtonSection(1)")       //���������� ������ �����
                    {
                        cameraPos = new Vector3(-7.0f, cameraPosition.position.y, cameraPosition.position.z);
                    }

                    if (nameObject == "ButtonSection(2)")       //���������� ������  ��������
                    {
                        cameraPos = new Vector3(0.0f, cameraPosition.position.y, cameraPosition.position.z);
                    }
                }

                if (currentObject.GetComponent<SettingObject>())
                {
                    ChangeProduct(currentObject);

                    //Debug.Log("ID " + GetIDObject(currentObject));
                }
            }
        }
    }

    private Vector3 cameraPos;

    void UpdateTextAllPoints()
    {
        textAllPoints.text = AllPoints.ToString();

        SaveData();
    }

}

