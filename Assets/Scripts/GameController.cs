using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    //Parameters
    [Header("Globals")]
    [SerializeField] private int gameLevel;
    public int localScore;
    [SerializeField] private int goldPerCrystal;
    [SerializeField] private int levelGold;
    [SerializeField] private int TrubaAmountAtStart;

    [Header("UI Parameters")]
    public float fadeSpeed;
    [SerializeField] private float secondsBeforeTransfer;
    [SerializeField] private float secondsAfterTransfer;

    [Header("Speed")]
    [SerializeField] private float defaultLevelSpeed;
    [SerializeField] private float speedIncrease;

    [Header("UI")]
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private RectTransform startPanel;
    [SerializeField] private RectTransform endPanel;
    [SerializeField] private RectTransform winPanel;

    private TMP_Text levelTxt;
    private Text scoreTxt;

    //List of tubes
    public List<GameObject> inGameTubes = new List<GameObject>();

    //List of current coins
    [SerializeField] private List<Coin> CrystalList = new List<Coin>();
    //Currently not collected coins
    public int currentCoins;
    //Maximum amount of coins
    [SerializeField] private int maxCoins;

    [HideInInspector] public bool death = false;
    [HideInInspector] public bool win = false;
    [HideInInspector] public bool paused = false;

    private int allCoins;
    [HideInInspector] public bool endGame = false;
    void Awake()
    {
        instance = this;

        if(!PlayerPrefs.HasKey("allCoins"))
        {
            PlayerPrefs.SetInt("allCoins", 0);
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        if (speedIncrease == 0) Debug.LogError("SpeedIncrease need to be above 0, to work");

        gameLevel = 1;

        //Start panel setup
        startPanel.gameObject.SetActive(true);
        startPanel.GetComponentInChildren<Button>().onClick.AddListener(StartButton);
        levelTxt = startPanel.GetChild(0).GetComponent<TMP_Text>();
        textPanel.gameObject.SetActive(false);
        //PlayerController.Instance.ResetPlayer();

        //Death panel setup
        endPanel.gameObject.SetActive(false);
        endPanel.GetComponentInChildren<Button>().onClick.AddListener(EndButton);

        //Win panel setup
        winPanel.gameObject.SetActive(false);
        winPanel.GetComponentInChildren<Button>().onClick.AddListener(WinButton);

        //Initial generation of the level
        TrubaGenerator.Instance.GenerateLevel(TrubaAmountAtStart);

        //Finding all "Coins"
        //inGameCrystals = FindObjectsOfType<Coin>();

        maxCoins = CrystalList.Count;

        allCoins = PlayerPrefs.GetInt("allCoins");
        //Starting pause
        RefreshText();
        Pause();
    }
    void Update()
    {
        //Debug
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Also need to reset local variable
            PlayerPrefs.SetInt("allCoins", 0);
            allCoins = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(paused)
            {
                Resume();
            }else
            {
                Pause();
            }
        }
        //Debug

        if (death)
        {
            //UpdateAllCoins();
            endPanel.gameObject.SetActive(true);
            textPanel.gameObject.SetActive(true);

            levelTxt = endPanel.Find("Level").GetComponent<TMP_Text>();
            scoreTxt = endPanel.Find("Panel [Image]").GetComponentInChildren<Text>();

            //Stop player movement
            PlayerController.Instance.StopPlayer();
            //Reset particle effecs
            PlayerController.Instance.StopEffects();
            Pause();
            RefreshText();

            if(localScore > 0) StartCoroutine(TransferGold());
            death = false;
        }

        if (win)
        {
            Pause();

            var panel1 = winPanel.Find("Title1");
            var panel2 = winPanel.Find("Title2");

            //Different text
            if (currentCoins == 0)
            {
                //All coins collected
                panel2.gameObject.SetActive(true);
                panel1.gameObject.SetActive(false);
            }

            if(currentCoins > 0)
            {
                //Not all coins collected
                panel1.gameObject.SetActive(true);
                panel2.gameObject.SetActive(false);
            }

            if (currentCoins == maxCoins)
            {
                //None of the coins  collected
            }

            //Reset particle effecs
            PlayerController.Instance.StopEffects();

            winPanel.gameObject.SetActive(true);
            textPanel.gameObject.SetActive(true);

            levelTxt = winPanel.Find("Level").GetComponent<TMP_Text>();
            scoreTxt = winPanel.Find("Panel [Image]").GetComponentInChildren<Text>();

            RefreshText();

            PlayerController.Instance.WalkTowardsCrystal();
            StartCoroutine(CameraFollow.cam.PlayerShowcase());
            if (localScore > 0) StartCoroutine(TransferGold());
            win = false;
            endGame = true;
        }
    }

    #region Buttons
    void StartButton()
    {
        setSpeed(defaultLevelSpeed);
        startPanel.gameObject.SetActive(false);
        textPanel.gameObject.SetActive(false);
        //Add them to the list for easier manipulation
        CrystalList.AddRange(FindObjectsOfType<Coin>());
        Resume();
    }

    public void EndButton()
    {
        endPanel.gameObject.SetActive(false);
        textPanel.gameObject.SetActive(false);
        death = false;

        //Resetting the position of the player
        PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();
        PlayerController.Instance.ResetPlayer();
        //Reset the level
        TrubaGenerator.Instance.ResetLevel();

        CrystalList.Clear();
        CrystalList.AddRange(FindObjectsOfType<Coin>(true));
        
        //Cycle through all the coins and enable them
        for (int i = 0; i < CrystalList.Count; i++)
        {
            CrystalList[i].gameObject.SetActive(true);
            //Current amount of coins on the level
            currentCoins = maxCoins;
        }

        Resume();
    }

    void WinButton()
    {
        endGame = false;
        //Increase the speed every 2 levels
        if (gameLevel % 2 == 0) changeSpeed(speedIncrease);
        //Increase pipe amount by 1 every 3 levels
        if (gameLevel % 3 == 0) TrubaAmountAtStart++;

        //Clearing the level and generating a new one
        TrubaGenerator.Instance.ClearLevel(inGameTubes);
        TrubaGenerator.Instance.GenerateLevel(TrubaAmountAtStart);

        winPanel.gameObject.SetActive(false);
        textPanel.gameObject.SetActive(false);
        win = false;
        //Increase level and speed of the level otherwise
        gameLevel++;

        PlayerController.Instance.IncreaseStartingPosition(new Vector3(0, speedIncrease, 0));

        //Resetting the position of the player
        PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();
        PlayerController.Instance.ResetPlayer();

        //Reset camera position and rotation
        CameraFollow.cam.ResetCamera();

        Resume();
    }
    #endregion

    #region Setters and Getters
    //Set the speed of current levels
    public void setSpeed(float _speed)
    {
        defaultLevelSpeed = _speed;

        for(int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().levelSpeed = defaultLevelSpeed;
        }
    }

    public void changeSpeed(float _speed)
    {
        defaultLevelSpeed += _speed;

        for (int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().levelSpeed = defaultLevelSpeed;
        }
    }

    //Get the speed of current levels
    public float getSpeed()
    {
        return defaultLevelSpeed;
    }
    #endregion
    

    void RefreshText()
    {
        textPanel.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("allCoins").ToString();
        if (levelTxt) levelTxt.text = "Level: " + gameLevel;
        if(scoreTxt) scoreTxt.text = localScore.ToString();
    }

    //A funciton to reset the speed and position of the level and player
    void Pause()
    {
        for (int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().moving = false;
        }

        //Pausing
        paused = true;
    }

    void Resume()
    {
        paused = false;

        for (int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().moving = true;
        }
    }

    //When player reached final tube of the level
    public void NextLevel()
    {
        //UI display
        win = true;
        localScore += levelGold;
    }

    IEnumerator TransferGold()
    {
        yield return new WaitForSeconds(secondsBeforeTransfer);

        for (int i = 0; i < localScore;)
        {
            allCoins++;
            localScore--;
            PlayerPrefs.SetInt("allCoins", allCoins);
            PlayerPrefs.Save();
            RefreshText();

            /*Debug.Log("L " + localScore);
            Debug.Log("A " + allCoins);*/
            yield return new WaitForSeconds(secondsAfterTransfer);
        }
    }

    public void CollectGold()
    {
        localScore += goldPerCrystal;
    }
}
