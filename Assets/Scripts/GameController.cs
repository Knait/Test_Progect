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
    [SerializeField] private float gameLevel;
    public int score;

    //UI
    [Header("UI")]
    [SerializeField] private RectTransform textPanel;
    [SerializeField] private RectTransform startPanel;
    [SerializeField] private RectTransform endPanel;
    private Text levelTxt;
    private Text scoreTxt;
    //REMOVE OR ADD UPON RELEASE
    private Text debugText;

    private bool pause = true;
    [HideInInspector] public bool death = false;

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
    }

    void Update()
    {
        DisplayText();

        if(pause) PlayerController.Instance.gameObject.transform.position = new Vector3(0, 0, 0);

        if(death) endPanel.gameObject.SetActive(true);
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

    void StartButton()
    {
        startPanel.gameObject.SetActive(false);
        pause = false;
    }

    void EndButton()
    {
        endPanel.gameObject.SetActive(false);
        death = false;
        pause = false;
    }
}
