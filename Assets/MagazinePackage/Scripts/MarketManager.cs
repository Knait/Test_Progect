using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveData
{
    // Размер массива для сохранения состаяния продуктов в магазе
    public int[] priceProduct = new int[27];
    public StateProduct[] stateProduct = new StateProduct[27];
}


public class MarketManager : MonoBehaviour
{
    private RaycastHit hit;
    private Ray MyRay;
    private GameObject currentObjectInUse;  // текущий обьект продукта
   // [SerializeField]
    private int ID;             // текущий ID обьекта продукта
    // [SerializeField]
    // private int LoadID;

    [SerializeField]
    private Transform cameraPosition;   //Камера
    [SerializeField]
    private TMPro.TextMeshProUGUI textAllPoints;         

   // [SerializeField]
    private int AllPoints;  // Игровая валюта

    [Header("Array Bars")]
    [SerializeField]
    private GameObject[] Bars;           //массив 3d кнопок плашек

    [Header("Array Sprites Background")]
    [SerializeField]
    private Sprite[] backGround;         // массив фона текста

    [SerializeField] private UnityEngine.UI.Button closeButton;

    void Start()
    {
        closeButton.onClick.AddListener(() => ReturnScene(0));

        LoadData();

        if (cameraPosition != null) cameraPos = new Vector3(-7.0f, cameraPosition.position.y, cameraPosition.position.z);

        //находим продукт InUse 
        foreach (GameObject gObject in Bars)
        {
            if (GetStateProduct(gObject) == StateProduct.InUse)
            {
                currentObjectInUse = gObject;           // устан текущий обьект InUse
                ID = GetIDObject(currentObjectInUse); // установим ID текущего InUse

               break;
            }
        }

        if (!PlayerPrefs.HasKey("NewTargetPrice"))
            PlayerPrefs.SetInt("NewTargetPrice", 50);

        foreach (GameObject gObject in Bars)
        {
            if (gObject.GetComponent<SettingObject>().priceProduct > PlayerPrefs.GetInt("AllPoint"))
            {
                PlayerPrefs.SetInt("NewTargetPrice", gObject.GetComponent<SettingObject>().priceProduct);
                break;
            }
        }
    }

    void Update()
    {
        UpdateMouse();                    // проверяем мышь
        UpdateSpritesBackGround(Bars, backGround);         // обновление подложки текста
        UpdateTextAllPoints();

        if (cameraPosition != null) cameraPosition.position = Vector3.Lerp(cameraPosition.position, cameraPos, 0.05f);
    }

    /// <summary>
    /// сохранение и возврат со сцены
    /// </summary>
    private void ReturnScene(int indexScene)
    {
        SaveData();
        SceneManager.LoadScene(indexScene);    // загрузка пред сцены
    }

    /// <summary>
    /// Возвращает обьект по нажатию кнопки под мышей
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
                result = filter.gameObject;                // обьект по которому щелкнули мышей               
            }
        }
        Debug.Log(result);
        return result;
    }

    /// <summary>
    /// Возврат  ID планки
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
    /// Смена продукта в магазе
    /// </summary>
    /// <param name="Object"></param>
    private void ChangeProduct(GameObject gObject)
    {
        StateProduct stateProduct = GetStateProduct(gObject);

        if (stateProduct == StateProduct.InUse || stateProduct == StateProduct.Locked) // если используем или заблокирован то выход
        {
            Debug.Log("StateObject " + stateProduct);
            return;
        }


        if (stateProduct == StateProduct.Price)                                       // если есть деньги то покупаем
        {
            int currentPriceObject = gObject.GetComponent<SettingObject>().priceProduct;

            if (AllPoints >= currentPriceObject)
            {
                AllPoints -= currentPriceObject;
                //gObject.GetComponent<SettingObject>().stateProduct = StateProduct.Use;

                SetStateProduct(gObject, StateProduct.Use);

            }
            Debug.Log("StateObject " + stateProduct);

            UpdateSpritesBackGround(Bars, backGround);         // обновление подложки текста
            return;
        }

        if (stateProduct == StateProduct.Use)
        {
            SetStateProduct(gObject, StateProduct.InUse);               //если мона использовать то используем 

            if (currentObjectInUse)
            {
                SetStateProduct(currentObjectInUse, StateProduct.Use);
            }
            currentObjectInUse = gObject;                          // установим текущий обьект InUse
            ID = GetIDObject(currentObjectInUse);                 // установим ID текущего InUse
            UpdateSpritesBackGround(Bars, backGround);         // обновление подложки текста
        }
    }


    /// <summary>
    /// возврат состояния продукта
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
    /// Установка статуса плашки
    /// </summary>
    /// <param name="gObject"></param>
    /// <param name="stateProduct"></param>
    private void SetStateProduct(GameObject gObject, StateProduct stateProduct)
    {
        gObject.GetComponent<SettingObject>().stateProduct = stateProduct;
        gObject.GetComponent<SettingObject>().DeactiveParticle(gObject.GetComponent<SettingObject>());
    }

    /// <summary>
    /// Смена подложки текста на плашках (барах)
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
    /// Загрузка данных магазина
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
    /// сохранение данных магазина
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

        key = "allCoins";
        PlayerPrefs.SetInt("allCoins", AllPoints);
        PlayerPrefs.SetInt("SavedID", ID);
        PlayerPrefs.Save();
        Debug.Log("Data saved!");
    }

    /// <summary>
    /// Удаление всех данных
    /// </summary>
    void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        AllPoints = 0;
        Debug.Log("Data reset complete");
    }


    /// <summary>
    /// Обработка мыши
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

                    if (nameObject == "ButtonSection(1)")       //перемещаем камеру слево
                    {
                        cameraPos = new Vector3(-7.0f, cameraPosition.position.y, cameraPosition.position.z);
                    }

                    if (nameObject == "ButtonSection(2)")       //перемещаем камеру  середина
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
    }

}

