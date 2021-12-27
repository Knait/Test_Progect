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

    //Globals
    [Header("Globals")]
    [SerializeField] private int gameLevel;
    public int score;
    [SerializeField] private float levelSpeed;
    [SerializeField] private float speedIncrease;

    //UI
    [Header("UI")]
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private RectTransform startPanel;
    [SerializeField] private RectTransform endPanel;
    [SerializeField] private RectTransform winPanel;

    //List of tubes
    public List<GameObject> inGameTubes = new List<GameObject>();
    //List of current coins
    //TODO add all generated coins there
    //disable them upon impact
    [SerializeField] private List<GameObject> coins = new List<GameObject>();

    private TMP_Text levelTxt;
    private Text scoreTxt;

    [HideInInspector] public bool death = false;
    [HideInInspector] public bool win = false;
    [HideInInspector] public bool paused = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (speedIncrease == 0) Debug.LogError("SpeedIncrease need to be above 0, to work");

        gameLevel = 1;
        //Go to child and grab component
        //scoreTxt = textPanel.GetChild(0).GetComponent<TextMeshPro>();

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

        //Starting pause
        Pause();
    }

    void Update()
    {
        DisplayText();

        if (death)
        {
            endPanel.gameObject.SetActive(true);
            levelTxt = endPanel.Find("Level").GetComponent<TMP_Text>();
            scoreTxt = endPanel.Find("Panel [Image]").GetComponentInChildren<Text>();
            DisplayText();
            Pause();
            death = false;
        }

        if (win)
        {
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
        setSpeed(levelSpeed);
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
        levelSpeed = _speed;

        for(int i = 0; i < inGameTubes.Count; i++)
        {
            inGameTubes[i].GetComponent<LevelController>().levelSpeed = levelSpeed;
        }
    }

    //Get the speed of current levels
    public float getSpeed()
    {
        return levelSpeed;
    }
    #endregion
    

    void DisplayText()
    {
        if(levelTxt) levelTxt.text = "Level: " + gameLevel;
        if(scoreTxt) scoreTxt.text = score.ToString();
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

        //Increase the speed every 2 levels
        if (gameLevel % 2 == 0) setSpeed(speedIncrease += 5);

        setSpeed(speedIncrease);

        PlayerController.Instance.ChangeStartingPosition(new Vector3(0, speedIncrease, 0));
        //Resetting the position of the player
        PlayerController.Instance.gameObject.transform.position = PlayerController.Instance.GetStartingPosition();

        //Clearing the level and generating a new one
        TrubaGenerator.Instance.ClearLevel(inGameTubes);
        TrubaGenerator.Instance.GenerateLevel(gameLevel);
    }
}
