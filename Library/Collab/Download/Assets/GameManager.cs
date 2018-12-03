using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Kinect = Windows.Kinect;

public class GameManager : MonoBehaviour {

    /// Singleton
    public static GameManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    /// Player
    Transform currentPlayer;
    [SerializeField] bool playerLost;


    /// Text
    [SerializeField] TextMeshProUGUI jointsCounter;
    [SerializeField] TextMeshProUGUI currentValueText;
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI finalScore;
    [SerializeField] TextMeshProUGUI totalWalls;
    int playerScore;
    public static int jointsColliding;
    [SerializeField] Animator gameOverAnim;
    private bool playerScoreSaved = true;


    ///Walls (obstacles)
    [SerializeField] GameObject[] levelWalls;
    [SerializeField] GameObject gameOverWall;
    [SerializeField] int setWallSpeed;
    [SerializeField] Transform wallSpawnPosition;
    private static float wallSpeed;
    private float wallSpawnTimer;
    public static float MyWallSpeed
    {
        get
        {
            return wallSpeed;
        }

        set
        {
            wallSpeed = value;
        }
    }
    int count = -1;


    ///Panic Bar   
    [SerializeField] Slider slider;
    [SerializeField] int maxValue;
    [SerializeField] int currentValue;
    [SerializeField] float lerpSpeed;
    [SerializeField] float regenTimer;
    [SerializeField] float regenTime;
    [SerializeField] Color dangerColor, safeColor, handleDefault, handleGone;
    [SerializeField] Image fill, handleSprite;
    [SerializeField] RectTransform handle;
    Vector3 normalSizedHandle;
    Vector3 largeSizedHandle;
    float handlePumpDuration;
    [SerializeField] PlayerCondition playerCondition;


    void Start ()
    {
        wallSpawnPosition = GameObject.FindGameObjectWithTag("WallSpawner").transform;
        SpawnNextWall();
        MyWallSpeed = setWallSpeed;
        fill = slider.transform.Find("Fill Area/Fill").GetComponent<Image>();
        handle = slider.transform.Find("Handle Slide Area/Handle").GetComponent<RectTransform>();
        handleSprite = slider.transform.Find("Handle Slide Area/Handle").GetComponent<Image>();

        normalSizedHandle = handle.localScale;
        largeSizedHandle = new Vector3(1.5f, 1.5f, 1.5f);
	}
	
	void Update ()
    {
        if (playerLost)
        {
            /// TODO DO HIGHSCORE LOGIC HERE
            
            MyWallSpeed = 0;

            LoseLogic();

            return;
        }

        if (Time.frameCount % 10 == 0)
        {
            jointsCounter.text = jointsColliding.ToString();
        }


        ManagePanicBar();

        regenTimer += Time.deltaTime;

        if (regenTimer > regenTime)
        {
            if (currentValue > 0)
            {
                currentValue -= 1;
            }
            regenTimer = 0;
        }
	}

    private void LoseLogic()
    {
        gameOverAnim.SetTrigger("gameover");
        totalWalls.text = "Total Walls : " + count;
        finalScore.text = "Final Score : " + playerScore;
        if (playerScoreSaved)
        {
            StaticScores.SaveScore(StaticScores.LastName, playerScore);
            playerScoreSaved = false;
        }

        //SceneManager.LoadScene("MenuScene");
    }

    void ManagePanicBar()
    {
        handlePumpDuration = 1;
        fill.color = Color.Lerp(safeColor, dangerColor, slider.value);
        float lerp = Mathf.PingPong(Time.time, handlePumpDuration) / handlePumpDuration;
        handleSprite.color = Color.Lerp(handleDefault, handleGone, lerp);

        if (slider.value > 0.8f)
        {
            playerCondition = PlayerCondition.Losing;

            handle.localScale = Vector3.Lerp(normalSizedHandle, largeSizedHandle, lerp);

            if (Time.frameCount % 40 == 0)
            {
                SoundManager.instance.PlaySound("heartBeat");
            }

            return;
        }

        else if (slider.value > 0.6f)
        {
            playerCondition = PlayerCondition.Poor;
            handle.localScale = Vector3.Lerp(normalSizedHandle, largeSizedHandle, lerp);

            if (Time.frameCount % 240 == 0)
            {
                SoundManager.instance.PlaySound("heartBeat");
            }

            return;
        }

        else if (slider.value > 0.4f)
        {
            playerCondition = PlayerCondition.Good;
            return;
        }

        else if (slider.value > 0.2f)
        {
            playerCondition = PlayerCondition.Excellent;
            return;
        }

        playerCondition = PlayerCondition.Perfect;

    }

    public void SpawnNextWall()
    {
        if (playerLost)
        {
            Vector3 gameOverSpawnPosition = new Vector3(wallSpawnPosition.position.x, 
                wallSpawnPosition.position.y, 
                wallSpawnPosition.position.z + 500);
            Instantiate(gameOverWall, gameOverSpawnPosition, Quaternion.identity);
            return;
        }

        int rand = UnityEngine.Random.Range(0, levelWalls.Length);
        Instantiate(levelWalls[rand], wallSpawnPosition.position, Quaternion.identity);
        MyWallSpeed += (int)playerCondition;
        count++;
    }

    /// <summary>
    /// Returns the amount of the panic bar should 
    /// be filled at any given time
    /// </summary>
    /// <returns></returns>
    float CalculatePanicBarFillPercentage()
    {
        if (currentValue > 0)
        {
            Debug.Log(currentValue + "   " + maxValue + "  " + (float)currentValue / maxValue);
            return (float)currentValue / maxValue;
        }
        return 0;
    }

    public void UpdateScore(int v)
    {
        playerScore += v;
        score.text = playerScore.ToString();
    }

    public void CheckWinCondition(Wall wall)
    {
        int failedJoints = 0;

        if (BodyManager.instance.MyPlayerBody != null)
        {
            currentPlayer = BodyManager.instance.MyPlayerBody.transform;

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                ///Find the child object that matches 
                ///the current joints name 
                Transform currentJointObj = currentPlayer.Find(jt.ToString());

                if (currentJointObj.GetComponent<JointManager>().isHitting)
                {
                    failedJoints += 1;
                }
            }

            if (failedJoints == 0)
            {
                PerfectPassWall(wall);
            }
            else
            {
                BreakWall(wall);
            }
        }
        else
        {
            BreakWall(wall);
            failedJoints = 24;
        }


        StartCoroutine(UpdatePanicBar(failedJoints));
        UpdateScore(24 - failedJoints);

        SpawnNextWall();
    }

    private void PerfectPassWall(Wall wall)
    {
        ParticleSystem p = Instantiate(ParticleManager.instance.GetParticle("WinPartSystem"));
        p.transform.position = new Vector3(transform.position.x, 3, transform.position.z);
        p.Play();
        wall.GetComponentInChildren<MeshRenderer>().enabled = false;
        SoundManager.instance.PlaySound("success");
    }

    private void BreakWall(Wall wall)
    {
        SoundManager.instance.PlaySound("wall_break");
        ParticleSystem p = Instantiate(ParticleManager.instance.GetParticle("BreakPartSystem"));
        p.transform.position = new Vector3(transform.position.x, 3, transform.position.z);
        p.Play();
        wall.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Updates the slider on the side to display 
    /// how close the player is to losing
    /// </summary>
    /// <param name="v">How much to increase current value by</param>
    /// <returns></returns>
    public IEnumerator UpdatePanicBar(int v)
    {
        float animationTime = 0f;

        currentValue += v;
        currentValueText.text = "Current value: " + currentValue.ToString();

        if (currentValue >= maxValue)
        {
            playerLost = true;
        }

        while (animationTime < lerpSpeed)
        {
            animationTime += Time.deltaTime;
            float lerpValue = animationTime / lerpSpeed;
            slider.value = Mathf.Lerp(slider.value, CalculatePanicBarFillPercentage(), lerpValue);
            yield return null;
        }

        //Debug.Log(v + "  " + slider.value + "  " + CalculatePanicBarFillPercentage() + "  " + currentValue);
    }

}

public enum PlayerCondition { Perfect, Excellent, Good, Poor, Losing };
