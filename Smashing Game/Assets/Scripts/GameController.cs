using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

public class GameController : MonoBehaviour {

    public GameObject mainPanel;
    public GameObject namePanel;
    public GameObject gamePanel;
    public GameObject scorePanel;
    public GameObject statsPanel;

    public InputField nameInputField;

    public Button backToNameButton;
    public Button exitButton;
    public Button nameSubmitButton;
    public Button backFromScoresButton;
    public Button myBestScoresButton;
    public Button myBestTodayButton;
    public Button myBestWeekButton;
    public Button myBestMonthButton;
    public Button highScoresButton;
    public Button highScoresTodayButton;
    public Button highScoresWeekButton;
    public Button highScoresMonthButton;
    public Button statsMenuButton;

    public Text madeItOrNotText;
    public Text[] rankText;
    public Text[] nameText;
    public Text[] scoreText;
    public Text timerText;
    public Text gamePanelTittle;
    public Text gameScoreText;
    public Text pressEnterText;
    public Text commonScoreText;
    public Text averageScoreText;

    public AudioSource gameAudio;

    public string playersName;
    public bool gameIsOn;
    public bool gameIsPlayed;
    public int playersScore;
    public float gameTimer;
    public Menu currentMenu;
    public int commonScore;
    public float averageScore;

    public Color normalYellow;
    public Color highlightOrange;
    public Color highLightPurple;
    public EntryResult _entryResult;

    //*******************************************
    string www = "http://localhost:5000/api/scores";
    byte[] results;
    //*******************************************


    // Use this for initialization
    void Start () {
        backToNameButton.onClick.AddListener(BackToNameButtonPressed);
        exitButton.onClick.AddListener(ExitButtonPressed);
        nameSubmitButton.onClick.AddListener(NameSubmitButtonPressed);
        backFromScoresButton.onClick.AddListener(BackFromScoresButtonPressed);
        myBestScoresButton.onClick.AddListener(MyBestButtonPressed);
        myBestTodayButton.onClick.AddListener(MyBestTodayButtonPressed);
        myBestWeekButton.onClick.AddListener(MyBestWeekButtonPressed);
        myBestMonthButton.onClick.AddListener(MyBestMonthButtonPressed);
        highScoresButton.onClick.AddListener(HighScoresButtonPressed);
        highScoresTodayButton.onClick.AddListener(HighScoresTodayButtonPressed);
        highScoresWeekButton.onClick.AddListener(HighScoresWeekButtonPressed);
        highScoresMonthButton.onClick.AddListener(HighScoresMonthButtonPressed);
        statsMenuButton.onClick.AddListener(StatsButtonPressed);

        playersName = "Anonymous";
        playersScore = 0;
        gameIsOn = false;
        gameIsPlayed = false;
        gameTimer = 0f;
        commonScore = 0;
        averageScore = 0f;
        commonScoreText.text = "";
        averageScoreText.text = "";
        _entryResult = null;
        
        mainPanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(false);
        scorePanel.gameObject.SetActive(false);
        statsPanel.gameObject.SetActive(false);
        namePanel.gameObject.SetActive(true);
        currentMenu = Menu.InNameMenu;
        
	}
	
	// Update is called once per frame
	void Update () {
        if (currentMenu.Equals(Menu.InMainMenu))
        {
            if (gameIsOn)
            {
                float redChannel = (1f + Mathf.Sin(Time.time*10)) / 2f;
                Color changingColor = new Color(redChannel, normalYellow.g, normalYellow.b);
                gamePanelTittle.color = changingColor;

                gameTimer -= Time.deltaTime;
                if (gameTimer <= 0f)
                {
                    gameIsOn = false;
                    timerText.text = "Times Up!";
                    gameIsPlayed = true;
                    gamePanelTittle.color = normalYellow;
                    pressEnterText.gameObject.SetActive(true);
                }
                else
                {
                    timerText.text = "" + Math.Round(gameTimer, 1);
                }
                if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) && gameIsOn)
                {
                    playersScore++;
                    gameScoreText.text = "" + playersScore;

                }
            }
            else if (gameIsPlayed && Input.GetKeyDown(KeyCode.Return))
            {
                gameIsPlayed = false;
                SendScoreToServer();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
        }
        else if (currentMenu.Equals(Menu.InScoreMenu))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                statsPanel.gameObject.SetActive(false);
            }
        }
        /*else if (currentMenu.Equals(Menu.InNameMenu))
            if (Input.GetKeyDown(KeyCode.Return))
            {
                NameSubmitButtonPressed();
            }*/
	}

    IEnumerator IGetHighScores (int days)
    {
        string s;
        if (days== 0)
        {
            s = www;
        }
        else
        {
            s = www + "?days=" + days;
           
        }
        UnityWebRequest webRequest = UnityWebRequest.Get(s);
        webRequest.SetRequestHeader("unityKey", "unity1234");
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            string a = webRequest.downloadHandler.text;
            ScoreEntryArray sArray = JsonUtility.FromJson<ScoreEntryArray>("{\"entries\":" + a.ToString() + "}");
            //Unityn ankea FromJson: 1) lista pitää olla wrapper luokassa
            // 2) ko. luokassa EI voi olla get/set tai ei osaa toimia
            FillInScoreList(sArray.entries.ToArray(), 1);
        }
        
    }

    IEnumerator IGetPlayersHighScores(int days)
    {
        string s = www + "/" + playersName;
        if (days != 0)
        {
            s += "?days=" + days;
        }
 
        UnityWebRequest webRequest = UnityWebRequest.Get(s);
        webRequest.SetRequestHeader("unityKey", "unity1234");
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            string a = webRequest.downloadHandler.text;
            //returned array have to be wrapped to variable in order to use JsonUtility convert
            ScoreEntryArray sArray = JsonUtility.FromJson<ScoreEntryArray>("{\"entries\":" + a.ToString() + "}");
            //Unityn ankea FromJson: 1) lista pitää olla wrapper luokassa
            // 2) ko. luokassa EI voi olla get/set tai ei osaa toimia
            FillInScoreList(sArray.entries.ToArray(), 1);
        }
    }

    IEnumerator IGetAverage()
    {
        string s = www + "/average";
        UnityWebRequest webRequest = UnityWebRequest.Get(s);
        webRequest.SetRequestHeader("unityKey", "unity1234");
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            string a = webRequest.downloadHandler.text;
            float.TryParse(a, out averageScore);
        }
    }

    IEnumerator IGetCommon()
    {
        string s = www + "/common";
        UnityWebRequest webRequest = UnityWebRequest.Get(s);
        webRequest.SetRequestHeader("unityKey", "unity1234");
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            string a = webRequest.downloadHandler.text;
            int.TryParse(a, out commonScore);
        }
    }

    public IEnumerator ISendResult()
    {
        //tämä ei toimi
        NewEntry n = new NewEntry
        {
            name = playersName,
            score = playersScore
        };
        string entryJson = JsonUtility.ToJson(n);
        byte[] bytes = Encoding.UTF8.GetBytes(entryJson);
        WWWForm form = new WWWForm();
        form.AddBinaryData("body", bytes);
        UnityWebRequest req = UnityWebRequest.Post(www, form);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("UnityKey", "unity1234");
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.uploadHandler.contentType = "application/json";

        yield return req.SendWebRequest();
        if (req.isNetworkError || req.isHttpError)
        {
            Debug.Log(req.error);
        }
        else
        {
            string a = req.downloadHandler.text;
            EntryResult result = JsonUtility.FromJson<EntryResult>(a);
            if (result.ranking > 0)
            {
                madeItOrNotText.text = "YOU MADE IT TO THE LIST";
                int startFrom;
                if (result.ranking < 10)
                {
                    startFrom = 1;
                }
                else
                {
                    startFrom = result.ranking - 4;
                }

                string searchUrl = www + "?slice=" + startFrom;
                UnityWebRequest webRequest = UnityWebRequest.Get(searchUrl);
                webRequest.SetRequestHeader("unityKey", "unity1234");

                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    string entryList = webRequest.downloadHandler.text;
                    ScoreEntryArray sArray = JsonUtility.FromJson<ScoreEntryArray>("{\"entries\":" + entryList.ToString() + "}");
                    FillInScoreListWithEntry(sArray.entries.ToArray(), startFrom, result);
                }
            }
            else
            {
                madeItOrNotText.text = "Bummer... Not good enough";
                HighScoresButtonPressed();
            }

        }
    }



    void StartGame()
    {
        gamePanel.gameObject.SetActive(true);
        pressEnterText.gameObject.SetActive(false);
        playersScore = 0;
        gameTimer = 10f;
        timerText.text = "10.00";
        gameScoreText.text = "0";
        gameIsOn = true;
        gameIsPlayed = false;
        gameAudio.PlayOneShot(gameAudio.clip);
    }


    void SendScoreToServer()
    {
        StartCoroutine(ISendResult());
        StartCoroutine(IGetAverage());
        StartCoroutine(IGetCommon());
        scorePanel.gameObject.SetActive(true);
        mainPanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(false);
        currentMenu = Menu.InScoreMenu;
    }


    void FillInScoreListWithEntry(ScoreEntry[] entries, int fromRank, EntryResult entry)
    {
        int scoresSet = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            string rank;
            int r = fromRank + scoresSet;
            if (r < 10)
            {
                rank = "0" + r;
            }
            else
            {
                rank = "" + r;
            }
            rankText[i].text = rank;
            nameText[i].text = entries[i].name;
            scoreText[i].text = "" + entries[i].score;

            if (entries[i].name.Equals(entry.name))
            {
                if (r==entry.ranking)
                {
                    rankText[i].color = highlightOrange;
                    nameText[i].color = highlightOrange;
                    scoreText[i].color = highlightOrange;
                }
                else
                {
                    rankText[i].color = highLightPurple;
                    nameText[i].color = highLightPurple;
                    scoreText[i].color = highLightPurple;
                }
            }
            else
            {
                rankText[i].color = normalYellow;
                nameText[i].color = normalYellow;
                scoreText[i].color = normalYellow;
            }
            scoresSet++;
            if (scoresSet == 10)
            {
                break;
            }
        }
        while (scoresSet < 10)
        {
            string rank;
            int r = fromRank + scoresSet;
            if (r < 10)
            {
                rank = "0" + r;
            }
            else
            {
                rank = "" + r;
            }
            rankText[scoresSet].text = rank;
            nameText[scoresSet].text = "";
            scoreText[scoresSet].text = "";
            rankText[scoresSet].color = normalYellow;
            nameText[scoresSet].color = normalYellow;
            scoreText[scoresSet].color = normalYellow;
            scoresSet++;
        }
    }



    void FillInScoreList(ScoreEntry[] entries, int fromRank)
    {
        int scoresSet = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            string rank;
            int r = fromRank + scoresSet;
            if (r < 10)
            {
                rank = "0" + r;
            }
            else
            {
                rank = "" + r;
            }
            rankText[i].text = rank;
            nameText[i].text = entries[i].name;
            scoreText[i].text = "" + entries[i].score;
            if (entries[i].name.Equals(playersName))
            {
                rankText[i].color = highLightPurple;
                nameText[i].color = highLightPurple;
                scoreText[i].color = highLightPurple;

            }
            else
            {
                rankText[i].color = normalYellow;
                nameText[i].color = normalYellow;
                scoreText[i].color = normalYellow;
            }

            scoresSet++;
            if (scoresSet==10)
            {
                break;
            }
        }
        while (scoresSet < 10)
        {
            string rank;
            int r = fromRank + scoresSet;
            if (r < 10)
            {
                rank = "0" + r;
            }
            else
            {
                rank = "" + r;
            }
            rankText[scoresSet].text = rank;
            nameText[scoresSet].text = "";
            scoreText[scoresSet].text = "";
            rankText[scoresSet].color = normalYellow;
            nameText[scoresSet].color = normalYellow;
            scoreText[scoresSet].color = normalYellow;
            scoresSet++;
        }
    }

    void BackToNameButtonPressed()
    {
        mainPanel.gameObject.SetActive(false);
        namePanel.gameObject.SetActive(true);
        currentMenu = Menu.InNameMenu;
    }

    void ExitButtonPressed()
    {
        Application.Quit();
    }

    void NameSubmitButtonPressed()
    {
        if (nameInputField.text == null || nameInputField.text.Equals(""))
        {
            playersName = "Anonymous";
        }
        else
        {
            playersName = nameInputField.text;
        }
        namePanel.gameObject.SetActive(false);
        mainPanel.gameObject.SetActive(true);
        currentMenu = Menu.InMainMenu;

    }

    void BackFromScoresButtonPressed()
    {
        scorePanel.gameObject.SetActive(false);
        statsPanel.gameObject.SetActive(false);
        namePanel.gameObject.SetActive(true);
        currentMenu = Menu.InNameMenu;
    }

    void MyBestButtonPressed()
    {

        StartCoroutine(IGetPlayersHighScores(0));
    }

    void MyBestTodayButtonPressed()
    {
        StartCoroutine(IGetPlayersHighScores(1));
    }

    void MyBestWeekButtonPressed()
    {
        StartCoroutine(IGetPlayersHighScores(7));
    }

    void MyBestMonthButtonPressed()
    {
        StartCoroutine(IGetPlayersHighScores(30));
    }

    void HighScoresButtonPressed()
    {
        StartCoroutine(IGetHighScores(0));
    }

    void HighScoresTodayButtonPressed()
    {
        StartCoroutine(IGetHighScores(1));
    }

    void HighScoresWeekButtonPressed()
    {
        StartCoroutine(IGetHighScores(7));
    }

    void HighScoresMonthButtonPressed()
    {
        StartCoroutine(IGetHighScores(30));
    }

    void StatsButtonPressed()
    {
        if (statsPanel.gameObject.activeInHierarchy)
        {
            statsPanel.gameObject.SetActive(false);
        }
        else
        {
            statsPanel.gameObject.SetActive(true);
            SetStatsTexts();
        }
    }

    private void SetStatsTexts()
    {
        commonScoreText.text = "" + commonScore;
        averageScoreText.text = "" + System.Math.Round(averageScore, 2);
    }

    void ExitStats()
    {
            statsPanel.gameObject.SetActive(false);
    }
}
