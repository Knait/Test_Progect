using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    //Singleton //TODO: TRY CATCH ANOTHER SIGLETON
    private static GameController instance;
    public static GameController Instance => instance;

    //Parameters
    [Header("Globals")]
    [SerializeField] private int gameLevel;
    public int score;

    [SerializeField] private float defaultLevelSpeed;
    [SerializeField] private float speedIncrease;

    //UI
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
    public int currentCoins;
    [SerializeField] private int maxCoins;

    [HideInInspector] public bool death = false;
    [HideInInspector] public bool win = false;
    [HideInInspector] public bool paused = false;

    private int allCoins;

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

        //Death panel setup
        endPanel.gameObject.SetActive(false);
        endPanel.GetComponentInChildren<Button>().onClick.AddListener(EndButton);

        //Win panel setup
        winPanel.gameObject.SetActive(false);
        winPanel.GetComponentInChildren<Button>().onClick.AddListener(WinButton);

        //Initial generation of the level
        TrubaGenerator.Instance.GenerateLevel(1);

        maxCoins = FindObjectsOfType<Coin>().Length;
        currentCoins = maxCoins;

        //Starting pause
        Pause();
    }
    void Update()
    {
        //Debug
        if (Input.GetKeyDown(KeyCode.R)) PlayerPrefs.SetInt("allCoins", 0);
        //Debug

        if (death)
        {
            UpdateAllCoins();
            endPanel.gameObject.SetActive(true);
            levelTxt = endPanel.Find("Level").GetComponent<TMP_Text>();
            scoreTxt = endPanel.Find("Panel [Image]").GetComponentInChildren<Text>();
            DisplayText();
            Pause();
            death = false;
        }

        if (win)
        {
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

            //if(currentCoins)
            winPanel.gameObject.SetActive(true);
            levelTxt = winPanel.Find("Level").GetComponent<TMP_Text>();
            scoreTxt = winPanel.Find("Panel [Image]").GetComponentInChildren<Text>();
            DisplayText();
            Pause();
            win = false;
        }
    }

    #region Buttons
    void StartButton()
    {
        setSpeed(defaultLevelSpeed);
        startPanel.gameObject.SetActive(false);
        paused = false;
        Resume();
    }

    void EndButton()
    {
        endPanel.gameObject.SetActive(false);
        death = false;

        //Reset position of the player
        PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();
        //Reset the level
        TrubaGenerator.Instance.ResetLevel();
        paused = false;
        Resume();
    }

    void WinButton()
    {
        winPanel.gameObject.SetActive(false);
        win = false;
        paused = false;
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
    

    void DisplayText()
    {
        textPanel.GetComponentInChildren<Text>().text = PlayerPrefs.GetInt("allCoins").ToString();
        if (levelTxt) levelTxt.text = "Level: " + gameLevel;
        if(scoreTxt) scoreTxt.text = score.ToString();
    }

    void UpdateAllCoins()
    {
        allCoins = PlayerPrefs.GetInt("allCoins");
        PlayerPrefs.SetInt("allCoins", allCoins + score);
        PlayerPrefs.Save();
        score = 0;
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
        //Resetting the velocity of player
        PlayerController.Instance.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        //DO NOT ALLOW DASHING (block controls)
        PlayerController.Instance.dashing = false;
    }

    void Resume()
    {
        //Time.timeScale = 1;
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

        //Increase level and speed of the levele otherwise
        gameLevel++;

        PlayerController.Instance.IncreaseStartingPosition(new Vector3(0, speedIncrease, 0));
        //Resetting the position of the player
        PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();

        //Clearing the level and generating a new one
        TrubaGenerator.Instance.ClearLevel(inGameTubes);
        TrubaGenerator.Instance.GenerateLevel(gameLevel);

        //Increase the speed every 2 levels
        if (gameLevel % 2 == 0) changeSpeed(speedIncrease);
    }
}
