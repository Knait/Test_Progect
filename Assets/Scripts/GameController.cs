using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance => instance;

    //Parameters
    [Header("Globals")]
    private int gameLevel;
    [HideInInspector] public int localScore;

    [Tooltip("Количество очков за подбор кристала.")]
    [SerializeField] private int goldPerCrystal;

    [Tooltip("Количество очков за прохождение уровня.")]
    [SerializeField] private int levelGold;

    [Tooltip("Количество труб на старте.")]
    [SerializeField] private int TrubaAmountAtStart;

    [Tooltip("Скорость с которой исчезают препятствия.")]
    public float fadeSpeed;

    [Header("UI Parameters")]
    [Tooltip("Сколько секунд подождать перед начислением очков.")]
    [SerializeField] private float secondsBeforeTransfer;

    [Tooltip("Сколько секунд между начислением одного очка.")]
    [SerializeField] private float secondsAfterTransfer;

    [Header("Speed")]
    [Tooltip("Начальная скорость уровня.")]
    [SerializeField] private float defaultLevelSpeed;
    [Tooltip("На сколько увеличивать скорость уровня.")]
    [SerializeField] private float speedIncrease;

    [Header("UI")]
    [Tooltip("Задержка времени перед появлением панели Победы.")]
    [SerializeField] private float timeBeforeWinPanel;
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private RectTransform startPanel;
    [SerializeField] private RectTransform endPanel;
    [SerializeField] private RectTransform winPanel;

    [Header("GEM")]
    public List<Color> gemColors = new List<Color>();

    private TMP_Text levelTxt;
    private Text scoreTxt;

    //List of tubes
    [HideInInspector] public List<GameObject> inGameTubes = new List<GameObject>();

    //List of current coins
    private List<Coin> CrystalList = new List<Coin>();
    //Currently not collected coins
    [HideInInspector] public int currentCoins;
    //Maximum amount of coins
    private int maxCoins;
    private int allCoins;

    //Flags
    [HideInInspector] public bool death = false;
    [HideInInspector] public bool win = false;
    [HideInInspector] public bool paused = false;
    [HideInInspector] public bool endGame = false;

    [HideInInspector] public bool vibrationLocked = false;

    void Awake()
    {
        instance = this;

        //Prefs
        //All coins
        if(!PlayerPrefs.HasKey("allCoins"))
        {
            PlayerPrefs.SetInt("allCoins", 0);
        }
        //Current level
        if(!PlayerPrefs.HasKey("currentLevel"))
        {
            PlayerPrefs.SetInt("currentLevel", 1);
        }
        //Current playerSpeed;
        if(!PlayerPrefs.HasKey("currentSpeed"))
        {
            PlayerPrefs.SetFloat("currentSpeed", defaultLevelSpeed);
        }
        //FlagForShop
        if(!PlayerPrefs.HasKey("goneShopping"))
        {
            PlayerPrefs.SetInt("goneShopping", 0);
        }
        //Current player skin
        if (!PlayerPrefs.HasKey("BodySkin_ID"))
        {
            PlayerPrefs.SetInt("BodySkin_ID", 0);
        }
        //Current sword skin
        if (!PlayerPrefs.HasKey("SwordSkin_ID"))
        {
            PlayerPrefs.SetInt("SwordSkin_ID", 0);
        }
        //The state of vibration (1 - On, 0 - Off)
        if (!PlayerPrefs.HasKey("Vibration"))
        {
            PlayerPrefs.SetInt("Vibration", 1);

        }
        else 
            if (PlayerPrefs.HasKey("Vibration")) //If player set the settings before
            {
                if(PlayerPrefs.GetInt("Vibration") == 0)
                {
                    vibrationLocked = true;
                }
                else
                    {
                        vibrationLocked = false;
                    }
            }

        PlayerPrefs.Save();
    }

    [SerializeField] private Button VibrateButton;
    [SerializeField] private Image buttonVibrateVisual;
    [SerializeField] private Sprite onVibrateVisual;
    [SerializeField] private Sprite offVibrateVisual;

    [SerializeField] private Button ShopButton;

    void Start()
    {
        if (speedIncrease == 0) Debug.LogError("SpeedIncrease need to be above 0, to work");

        //RESET LEVEL AT START
        if(PlayerPrefs.GetInt("goneShopping") == 1)
        {
            //Increased level stats (as if next level button was pressed)
            ChangeSpeedAndLevel();

            gameLevel = PlayerPrefs.GetInt("currentLevel");
            PlayerPrefs.SetInt("goneShopping", 0);
            PlayerPrefs.Save();

        } else
        {
            gameLevel = 1;
            PlayerPrefs.SetInt("currentLevel", 1);
        }

        //Start panel setup
        startPanel.gameObject.SetActive(true);
        startPanel.GetComponentInChildren<Button>().onClick.AddListener(StartButton);
        levelTxt = startPanel.GetChild(0).GetComponent<TMP_Text>();

        textPanel.gameObject.SetActive(false);
        
        //Creating a list of buttons on textpanel and filling it out
      //  List<Button> textPanelButtons = new List<Button>();
     //   textPanelButtons.AddRange(textPanel.GetComponentsInChildren<Button>());

        //If there is more than one button

        ShopButton.onClick.AddListener(ShopSelection);

        VibrateButton.onClick.AddListener(() => SwitchVibration());
        if (vibrationLocked)
            buttonVibrateVisual.sprite = offVibrateVisual;
        else
            buttonVibrateVisual.sprite = onVibrateVisual;

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
        //Debug (Resetting data)
        if (Input.GetKeyDown(KeyCode.R))
        {
            
            PlayerPrefs.DeleteAll();
            allCoins = 0;
            //Also need to reset local variable
            PlayerPrefs.Save();
            Debug.Log("All data was reset!");
        }
        //Debug (Pause the level)
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

        //Debug (Add 200 crystals)
        if (Input.GetKeyDown(KeyCode.F))
        {
            localScore += 1000;
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
            PlayerController.Instance.WalkTowardsCrystal();
            StartCoroutine(CameraFollow.cam.PlayerShowcase());
            StartCoroutine(WaitWinPanel(timeBeforeWinPanel));
            win = false;
            endGame = true;
        }
    }

    IEnumerator WaitWinPanel(float _time)
    {

        yield return new WaitForSeconds(_time);

        var panel1 = winPanel.Find("Title1");
        var panel2 = winPanel.Find("Title2");

        //Different text
        if (currentCoins == 0)
        {
            //All coins collected
            panel2.gameObject.SetActive(true);
            panel1.gameObject.SetActive(false);
        }

        if (currentCoins > 0)
        {
            //Not all coins collected
            panel1.gameObject.SetActive(true);
            panel2.gameObject.SetActive(false);
            //StartCoroutine(goodJobPanel(timeBeforePanel));
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

        if (localScore > 0) StartCoroutine(TransferGold());
    }

    #region Buttons
    void StartButton()
    {
        SetSpeed(defaultLevelSpeed);

        startPanel.gameObject.SetActive(false);
        textPanel.gameObject.SetActive(false);

        //Add them to the list for easier manipulation
        CrystalList.AddRange(FindObjectsOfType<Coin>());
        PlayerController.Instance.StartEffect();
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
        allCoins += localScore;
        PlayerPrefs.SetInt("allCoins", allCoins);
        PlayerPrefs.Save();

        endGame = false;

        PlayerController.Instance.ResetPlayer();

        //Clearing the level and generating a new one
        TrubaGenerator.Instance.ClearLevel(inGameTubes);
        TrubaGenerator.Instance.GenerateLevel(TrubaAmountAtStart);

        winPanel.gameObject.SetActive(false);
        textPanel.gameObject.SetActive(false);

        win = false;

        ChangeSpeedAndLevel();

        //STARTING POS MAKING BUG
        //PlayerController.Instance.IncreaseStartingPosition(new Vector3(0, speedIncrease, 0));

        //Resetting the position of the player
        //PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();

        //Destroying previously attached gem
        PlayerController.Instance.DestroyGem();

        //Reset camera position and rotation
        CameraFollow.cam.ResetCamera();

        Resume();
    }
    #endregion

    #region Setters and Getters
    //Set the speed of current levels
    public void SetSpeed(float _speed)
    {
        defaultLevelSpeed = _speed;

        for(int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().levelSpeed = defaultLevelSpeed;
        }
    }

    public void IncreaseSpeed(float _speed)
    {
        defaultLevelSpeed += _speed;

        for (int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().levelSpeed = defaultLevelSpeed;
        }
    }

    //Get the speed of current levels
    public float GetSpeed()
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

    void ChangeSpeedAndLevel()
    {
        //Increase the speed every 2 levels
        if (defaultLevelSpeed < 70)
        {
            if (gameLevel % 2 == 0) IncreaseSpeed(speedIncrease);
        }

        //Increase pipe amount by 1 every 3 levels
        if (gameLevel % 3 == 0) TrubaAmountAtStart++;

        //Increase level and speed of the level otherwise
        gameLevel++;
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

    //A function that executes when player pressed "Shop" button
    void ShopSelection()
    {
        allCoins += localScore;
        PlayerPrefs.SetInt("allCoins", allCoins);
        PlayerPrefs.SetFloat("currentSpeed", defaultLevelSpeed);
        PlayerPrefs.SetInt("currentLevel", gameLevel);
        PlayerPrefs.SetInt("goneShopping", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }

    //Switching vibration on and off
    void SwitchVibration()
    {
        Debug.Log("Vibration swiitching");

        if(!vibrationLocked) 
        {
            vibrationLocked = true;
            buttonVibrateVisual.sprite = offVibrateVisual;
            Vibration.Cancel(); 

            //Save the changes
            PlayerPrefs.SetInt("Vibration", 0);

        }
        else
        {
            vibrationLocked = false;
            buttonVibrateVisual.sprite = onVibrateVisual;

            //Save the changes
            PlayerPrefs.SetInt("Vibration", 1);
        }

        PlayerPrefs.Save();
    }
}
