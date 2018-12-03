using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Kinect = Windows.Kinect;

public class HighScoreController : MonoBehaviour {

    public static HighScoreController instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    /// Public
    public bool viewingHighScores;

    /// Private
    [SerializeField] TextMeshPro[] scores;
    [SerializeField] TextMeshPro[] names;
    [SerializeField] TextMeshPro bestPlayer;
    [SerializeField] TextMeshPro bestPlayerScore;
    [SerializeField] ulong currTrackingId = 0;
    BodySourceManager bsm;
    private Kinect.HandState rightHandState;
    private Kinect.HandState leftHandState;
    bool handCheckCooldown;

    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();

    private void Start()
    {
        StaticScores.LoadScores();
        bsm = BodySourceManager.instance;
        UpdateHighScores();
    }



    void Update()
    {
        if (viewingHighScores)
        {
            ///Gather the data from the sourceManager
            Kinect.Body[] data = BodySourceManager.instance.GetData();

            /// if there is no data don't do anything
            if (data == null)
            {
                return;
            }

            /// instantiate an empty list of tracked IDs
            /// This list is necessary in the event that we make multiplayer
            List<ulong> trackedIds = new List<ulong>();

            /// Make sure you only have 1 player
            Kinect.Body player = bsm.GetActiveBody(data, currTrackingId);

            ///if you have a player then add -
            ///it to the list of trackedIds
            if (player != null)
            {
                trackedIds.Add(player.TrackingId);
            }

            List<ulong> knownIds = new List<ulong>(bodies.Keys);

            /// First delete untracked bodies
            foreach (ulong trackingId in knownIds)
            {
                if (!trackedIds.Contains(trackingId))
                {
                    Destroy(bodies[trackingId]);
                    bodies.Remove(trackingId);
                }
            }

            /// If body doesn't exist create it
            if (player != null)
            {
                if (!bodies.ContainsKey(player.TrackingId))
                {
                    Debug.Log("Didnt have the key in bodies disctionary");
                    bodies[player.TrackingId] = bsm.CreateBodyObject(player.TrackingId, transform);
                }

                Debug.Log(bodies[player.TrackingId].gameObject.name);

                RefreshBodyObject(player, bodies[player.TrackingId]);
                //DrawBodyLines(player, bodies[player.TrackingId]);
            }
        }
    }

    public void UpdateHighScores()
    {
        int count = 0;

        foreach (ScoreInfo info in StaticScores.scoreList)
        {
            names[count].text = info.Name;
            scores[count].text = info.Score.ToString();
            count++;
        }

        bestPlayer.text = StaticScores.scoreList[0].Name;
        bestPlayerScore.text = StaticScores.scoreList[0].Score.ToString();
    }

    /// <summary>
    /// Refreshes the current body updating 
    /// the position of each of the limbs
    /// </summary>
    /// <param name="body">The Body to Update</param>
    /// <param name="bodyObject">The Body object holding all the pieces</param>
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        rightHandState = body.HandRightState;
        leftHandState = body.HandLeftState;

        if (rightHandState == Kinect.HandState.Closed && body.HandRightConfidence == Kinect.TrackingConfidence.High)
        {
            if (leftHandState == Kinect.HandState.Closed && body.HandLeftConfidence == Kinect.TrackingConfidence.High)
            {
                if (!handCheckCooldown)
                {
                    StartCoroutine(ReturnToMenu());
                }
            }
        }
    }

    IEnumerator ReturnToMenu()
    {
        handCheckCooldown = true;
        Debug.Log("hands closed");
        Camera.main.GetComponent<Animator>().SetTrigger("hightomenu");
        yield return new WaitForSeconds(4);
        handCheckCooldown = false;
    }

    public void ResetHighScoreArea()
    {
        bodies.Clear();
        currTrackingId = 0;
    }
}
