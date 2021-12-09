using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class FSGameController : MonoBehaviour
{
    private static FSGameController instance;
    public static FSGameController Instance => instance;

    [HideInInspector] public bool gameIsPlayed = false;

    [Header("UI Menu Elements")]
    [SerializeField] private GameObject startLevelPanel;
    [SerializeField] private GameObject[] GamePlayedPanels = new GameObject[1];
    [SerializeField] private GameObject winGamePanel;
    [SerializeField] private GameObject loseGamePanel;

    [Header("Point")]
    [SerializeField] private bool usePointToWin = false;// хотим ли мы, что бы при наборе определенного количества очков игрок выигрывал
    [SerializeField] private int needPointsToWin = 50;
    private int pointValue = 0;
    [SerializeField] private TextMeshProUGUI PointTextInGame;

    [SerializeField] private TextMeshProUGUI PointText_WinPanel;
    [SerializeField] private TextMeshProUGUI PointText_LosePanel;

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private AudioClip pointUpdateAudio;

    [Header("ID следующего уровня")]
    [SerializeField] private int nextLevelID = 0;

    #region timer
    [Header("Timer")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private TextMeshProUGUI TimeText;
    private int minuts = 0;
    private float second = 0;

    [Header("TimerGoDown (Если useTimer = true)")]
    [SerializeField] private bool useTimerDown = false;
    [SerializeField] private int startMinutCount = 5;

    [Header("TimerGoUp (Если useTimer = true)")]
    [SerializeField] private bool useTimerUp = false;
    [SerializeField] private int maximumMinutCount = 5;
    #endregion

    #region startTimer
    [Header("Использовать таймер для старта")]
    [SerializeField] private float timerInStartValue = 3;
    [SerializeField] private Animator startTimerAnim;
    #endregion

    [Header("Запускаем при старте")]
    [SerializeField] private Animator[] gameStartAnim = new Animator[0];
    [SerializeField] private ParticleSystem[] gameStartParticle = new ParticleSystem[0];

    [Header("Запускаем при завершении")]
    [SerializeField] private Animator[] gameEndAnim = new Animator[0];
    [SerializeField] private ParticleSystem[] gameEndParticle = new ParticleSystem[0];

    [SerializeField] private UnityEvent gameStarted;
    [SerializeField] private UnityEvent gameEnded;
    [SerializeField] private UnityEvent gameReset;

    private void Awake()
    {
    /*    instance = this;

        minuts = startMinutCount;
        second = 0;
        TimeText.text = $"{minuts}:00";

        if (PointTextInGame != null) PointTextInGame.text = "0";
        pointValue = 0;

        //отключаем лишние панели и включаем только стартовую
        if (startLevelPanel != null) startLevelPanel.SetActive(true);
        if (GamePlayedPanels.Length > 0)
            for (int i = 0; i < GamePlayedPanels.Length; i++)
                GamePlayedPanels[i].SetActive(false);

        if (winGamePanel != null) winGamePanel.SetActive(false);
        if (loseGamePanel != null) loseGamePanel.SetActive(false);

        //Если есть класс SoundManagerAllControll
        //if (SoundManagerAllControll.Instance && backgroundClip) SoundManagerAllControll.Instance.BackgroundClipPlay(backgroundClip);

        //Проверки
        if (useTimer)
        {
            if (TimeText == null) Debug.LogError("Забыл прикрепить поле куда будет записываться текст-тайм");
            if (!useTimerDown && !useTimerUp) Debug.LogError("Если используешь тайме, нужно указать куда он будет двигаться (useTimerUp/useTimerDown), иначе по дефолту будет двигаться в +, без ограничения по времени");
        }
    */
        StartCoroutine(startCoroutine());
    }

    private IEnumerator startCoroutine()
    {
        Debug.LogError("We Wait Start");
        yield return new WaitForSeconds(5);

        GameStarted();
    }

    private void Update()
    {
        if (!gameIsPlayed) return;

        if (useTimer) TimeGO();
    }

    #region levelControllers
    public void NextLevel()//обращаемся из вне (кнопка UI на панели победы)
    {
        SceneManager.LoadScene(nextLevelID);
        //при необходимости запускаем из этой функции корутину (если например надо, что бы перед запуском нового уровня доигрывалась анимация)
    }

    public void ResetLevel()//я чаще использую полную перезагрузку сцены в случае если игрок проиграл и хочет начать заново.
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region startGame
    public void GameStarted()
    {
        /* if (startLevelPanel != null) startLevelPanel.SetActive(false);
         if (GamePlayedPanels.Length > 0)
             for (int i = 0; i < GamePlayedPanels.Length; i++)
                 GamePlayedPanels[i].SetActive(true);

         if (useTimer)
         {
             if (useTimerDown) minuts = startMinutCount;
             if (useTimerUp) minuts = 0;
             second = 0;
             TimeText.text = $"{minuts}:00";
         }

         if (PointTextInGame != null) PointTextInGame.text = "0";
         pointValue = 0;
        */
        StartCoroutine(StartTimerActive());
    }

    private IEnumerator StartTimerActive()
    {
        gameIsPlayed = true;
        gameStarted.Invoke();
        yield return new WaitForSeconds(timerInStartValue);
        /*        startTimerAnim.SetTrigger("Start");

                gameStarted.Invoke();

                if (gameStartAnim.Length >= 1)
                    for (int i = 0; i < gameStartAnim.Length; i++)
                        gameStartAnim[i].SetTrigger("Start");

                if (gameStartParticle.Length >= 1)
                    for (int i = 0; i < gameStartParticle.Length; i++)
                        gameStartParticle[i].Play();*/
    }
    #endregion

    #region endGame
    public void GameEnded(bool win = false)
    {
        gameIsPlayed = false;

        gameEnded.Invoke();

        if (gameEndAnim.Length >= 1)
            for (int i = 0; i < gameEndAnim.Length; i++)
                gameEndAnim[i].SetTrigger("Start");

        if (gameEndParticle.Length >= 1)
            for (int i = 0; i < gameEndParticle.Length; i++)
                gameEndParticle[i].Play();

        if (win)
        {
            for (int i = 0; i < GamePlayedPanels.Length; i++)
                GamePlayedPanels[i].SetActive(false);
            if (winGamePanel != null) winGamePanel.SetActive(true);
        }
        else
        {
            for (int i = 0; i < GamePlayedPanels.Length; i++)
                GamePlayedPanels[i].SetActive(false);
            if (loseGamePanel != null) loseGamePanel.SetActive(true);
        }
    }
    #endregion

    public int GetPointValue()
    {
        return pointValue;
    }

    public void UpdatePoint(int point)
    {
        if (!gameIsPlayed) return;

        pointValue += point;
        if (PointTextInGame != null) PointTextInGame.text = pointValue.ToString();

        //Если есть класс SoundManagerAllControll
        //if (SoundManagerAllControll.Instance && pointUpdateAudio != null) SoundManagerAllControll.Instance.ClipPlay(pointUpdateAudio);

        if (usePointToWin && pointValue >= needPointsToWin)
            GameEnded(true);
    }

    private void TimeGO()
    {
        if (!gameIsPlayed) return;

        if (useTimerDown)
        {
            if (second <= 0)
            {
                minuts -= 1;
                second = 59;
            }
            else
                second = Mathf.Clamp(second - Time.deltaTime, 0, 60);

            if (second >= 10)
                TimeText.text = $"{minuts}:{Mathf.CeilToInt(second)}";
            else
                TimeText.text = $"{minuts}:0{Mathf.CeilToInt(second)}";

            if (minuts <= 0 && second <= 0)
                GameEnded();
        }
        else
        {
            if (useTimerUp)
            {

                if (second >= 60)
                {
                    minuts += 1;
                    second = 0;
                }
                else
                    second = Mathf.Clamp(second + Time.deltaTime, 0, 60);

                if (second >= 10)
                    TimeText.text = $"{minuts}:{Mathf.CeilToInt(second)}";
                else
                    TimeText.text = $"{minuts}:0{Mathf.CeilToInt(second)}";

                if (minuts >= maximumMinutCount)
                    GameEnded();
            }
            else
            {
                if (second >= 60)
                {
                    minuts += 1;
                    second = 0;
                }
                else
                    second = Mathf.Clamp(second + Time.deltaTime, 0, 60);

                if (second >= 10)
                    TimeText.text = $"{minuts}:{Mathf.CeilToInt(second)}";
                else
                    TimeText.text = $"{minuts}:0{Mathf.CeilToInt(second)}";
            }
        }
    }
}