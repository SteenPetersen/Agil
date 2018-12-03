using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class MenuController : MonoBehaviour {

    public static MenuController instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    /// Public
    public bool leftMenu;

    /// Private
    [SerializeField] Transform playerSpawnPosition;
    [SerializeField] Transform perspectiveObj;
    [SerializeField] ulong currTrackingId = 0;
    [SerializeField] GameObject handCursorObj;
    bool initialized;
    BodySourceManager bsm;
    Animator camAnim;
    private Kinect.HandState handState;
    GameObject handCursor;
    bool selectionMade;
    bool playerInteracting;

    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();

    void Start ()
    {
        bsm = BodySourceManager.instance;
        camAnim = Camera.main.GetComponent<Animator>();
	}

    void Update()
    {
        if (!leftMenu)
        {
            ///Gather the data from the sourceManager
            Kinect.Body[] data = bsm.GetData();

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

            /// If you have a player
            if (player != null)
            {
                if (!playerInteracting)
                {
                    Debug.Log("Found player setting interaction to true");
                    playerInteracting = true;
                }

                if (!bodies.ContainsKey(player.TrackingId))
                {
                    bodies[player.TrackingId] = bsm.CreateBodyObject(player.TrackingId, playerSpawnPosition);
                    initialized = false;
                }

                UpdateHandInput(player, bodies[player.TrackingId]);
            }

            /// If you do not have a player
            if (player == null && playerInteracting)
            {
                Debug.Log("Can no longer find player!");
                ClearMenuElements(false);
                playerInteracting = false;
            }
        }
    }

    GameObject playButton;
    GameObject enterNameButton;
    GameObject highScoresButton;
    bool playSelected, highscoreSelected, enterNameSelected;

    /// <summary>
    /// Refreshes the current body updating 
    /// the position of each of the limbs
    /// </summary>
    /// <param name="body">The Body to Update</param>
    /// <param name="bodyObject">The Body object holding all the pieces</param>
    private void UpdateHandInput(Kinect.Body body, GameObject bodyObject)
    {
        if (!initialized)
        {
            handCursor = Instantiate(handCursorObj);
            playerSpawnPosition.localEulerAngles = new Vector3(playerSpawnPosition.localEulerAngles.x - 90, 0, 0);
            initialized = true;
        }

        Kinect.Joint rh = body.Joints[Kinect.JointType.HandRight];

        Transform currentJointObj = bodyObject.transform.Find(Kinect.JointType.HandRight.ToString());

        /// Find the Vector3 Position of this joint
        Vector3 currentJointPosition = BodySourceManager.GetVector3FromJoint(rh);

        currentJointObj.localPosition = new Vector3(currentJointPosition.x * -1, currentJointPosition.y, currentJointPosition.z);

        handState = body.HandRightState;

        Vector3 handPos = (currentJointObj.position - perspectiveObj.position) * 0.5f + perspectiveObj.position;
        handCursor.transform.position = new Vector3(handPos.x, 2, handPos.z);

        Vector3 dir = new Vector3(perspectiveObj.position.x - currentJointObj.position.x,
                                  perspectiveObj.position.y - currentJointObj.position.y,
                                  perspectiveObj.position.z - currentJointObj.position.z);

        Debug.DrawRay(currentJointObj.position, dir, Color.cyan, 2);

        int menuMask = 1 << 13;

        RaycastHit hit;
        if (Physics.Raycast(currentJointObj.position, dir, out hit, Mathf.Infinity, menuMask))
        {
            Vector3 selected = new Vector3(1.1f, 1.1f, 1);

            if (hit.collider.tag == "PlayButton")
            {
                if (!playSelected)
                {
                    hit.collider.transform.localScale = selected;
                    playButton = hit.collider.transform.gameObject;

                    if (highScoresButton != null)
                    {
                        highScoresButton.transform.localScale = Vector3.one;
                    }
                    if (enterNameButton != null)
                    {
                        enterNameButton.transform.localScale = Vector3.one;
                    }

                    SoundManager.instance.PlaySound("menu_hover");

                    playSelected = true;

                    highscoreSelected = false;
                    enterNameSelected = false;
                }


            }

            else if (hit.collider.tag == "HighButton")
            {
                if (!highscoreSelected)
                {
                    hit.collider.transform.localScale = selected;
                    highScoresButton = hit.collider.transform.gameObject;

                    if (playButton != null)
                    {
                        playButton.transform.localScale = Vector3.one;
                    }
                    if (enterNameButton != null)
                    {
                        enterNameButton.transform.localScale = Vector3.one;
                    }

                    SoundManager.instance.PlaySound("menu_hover");

                    highscoreSelected = true;

                    playSelected = false;
                    enterNameSelected = false;
                }

            }

            else if (hit.collider.tag == "NameButton")
            {
                if (!enterNameSelected)
                {
                    hit.collider.transform.localScale = selected;
                    enterNameButton = hit.collider.transform.gameObject;

                    if (highScoresButton != null)
                    {
                        highScoresButton.transform.localScale = Vector3.one;
                    }
                    if (playButton != null)
                    {
                        playButton.transform.localScale = Vector3.one;
                    }

                    SoundManager.instance.PlaySound("menu_hover");

                    enterNameSelected = true;

                    playSelected = false;
                    highscoreSelected = false;
                }

            }

            if (handState == Kinect.HandState.Closed && body.HandRightConfidence == Kinect.TrackingConfidence.High && !selectionMade)
            {
                if (hit.collider.tag == "PlayButton")
                {
                    camAnim.SetTrigger("menutogame");
                    selectionMade = true;
                }

                else if (hit.collider.tag == "HighButton")
                {
                    camAnim.SetTrigger("menutohigh");
                    selectionMade = true;
                }

                else if (hit.collider.tag == "NameButton")
                {
                    camAnim.SetTrigger("menutoname");
                    selectionMade = true;
                }
            }
        }
    }

    /// <summary>
    /// Resets the Menu when the
    /// user navigates away from it
    /// </summary>
    public void ClearMenuElements(bool playerLeftMenu = true)
    {
        currTrackingId = 0;
        initialized = false;
        bodies.Clear();
        Destroy(handCursor);
        selectionMade = false;
        playerSpawnPosition.localEulerAngles = new Vector3(0, 0, 0);

        if (playerLeftMenu)
        {
            leftMenu = true;
        }
    }
}
