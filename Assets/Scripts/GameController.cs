using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //Singleton //TODO: TRY CATCH ANOTHER SIGLETON
    private static GameController instance;
    public static GameController Instance => instance;

    //Globals
    [Header("Globals")]
    [SerializeField] private float gameLevel;
    public int score;
    [SerializeField] private float levelSpeed;

    //UI
    [Header("UI")]
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private RectTransform startPanel;
    [SerializeField] private RectTransform endPanel;
    [SerializeField] private RectTransform winPanel;

    //List of tubes
    public List<GameObject> inGameTubes = new List<GameObject>();

    private Text levelTxt;
    private Text scoreTxt;
    //REMOVE OR ADD UPON RELEASE
    private Text debugText;

    [HideInInspector] public bool death = false;
    [HideInInspector] public bool win = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameLevel = 1;
        //Go to child and grab component
        scoreTxt = textPanel.GetChild(0).GetComponent<Text>();
        levelTxt = textPanel.GetChild(1).GetComponent<Text>();
        debugText = textPanel.GetChild(2).GetComponent<Text>();

        //Start panel setup
        startPanel.gameObject.SetActive(true);
        startPanel.GetComponentInChildren<Button>().onClick.AddListener(StartButton);

        //Death panel setup
        endPanel.gameObject.SetActive(false);
        endPanel.GetComponentInChildren<Button>().onClick.AddListener(EndButton);

        //Win panel setup
        winPanel.gameObject.SetActive(false);
        winPanel.GetComponentInChildren<Button>().onClick.AddListener(WinButton);

        Pause();
    }

    void Update()
    {
        DisplayText();

        if (death)
        {
            endPanel.gameObject.SetActive(true);
            Pause();
            death = false; //Temporary
        }

        if (win)
        {
            winPanel.gameObject.SetActive(true);
            Pause();
            win = false; //Temporary
        }
    }

    void DisplayText()
    {
        if(!levelTxt || !scoreTxt)
        {
            Debug.LogError("Can't display text. No text object found.");

        } else
        {
            levelTxt.text = "Level: " + gameLevel;
            scoreTxt.text = "Score: " + score;
            debugText.text = "" + PlayerController.Instance.restBetweenDash;
        }
    }

    #region Buttons
    void StartButton()
    {
        startPanel.gameObject.SetActive(false);
        setSpeed(10);
    }

    void EndButton()
    {
        endPanel.gameObject.SetActive(false);
        death = false;
        setSpeed(10);
    }

    void WinButton()
    {
        winPanel.gameObject.SetActive(false);
        win = false;
        setSpeed(10);
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

    //A funciton to reset the speed and position of the level and player
    void Pause()
    {
        GameObject previousObj = null;
        setSpeed(0);

        //Resetting the position of the tubes
        for (int i = 0; i < inGameTubes.Count; i++)
        {
            Debug.Log(i);
            if (i == 0)
            {
                previousObj = inGameTubes[0];
                inGameTubes[i].transform.position = new Vector3(0, -45f, 0);

            }
            else
            {
                inGameTubes[i].transform.position = new Vector3(0, previousObj.transform.position.y - 90f, 0);
                previousObj = inGameTubes[i];
            }
        }

        //Resetting the position of the player
        PlayerController.Instance.gameObject.transform.position = new Vector3(0, 0, 0);
    }
}
