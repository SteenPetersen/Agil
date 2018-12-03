using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Kinect = Windows.Kinect;

public class EnterNameController : MonoBehaviour {

    public static EnterNameController instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    /// Public
    public bool enteringName;

    /// Private
    [SerializeField] GameObject A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z;
    [SerializeField] GameObject[] letters;
    [SerializeField] Material defaultLetterMaterial;
    [SerializeField] Material hoverLetterMaterial;
    [SerializeField] Material selectedLetterMaterial;
    [SerializeField] string tmpName;
    [SerializeField] TextMeshPro playerName;
    [SerializeField] Transform playerSpawnPosition;
    [SerializeField] Transform perspectiveObj;
    [SerializeField] ulong currTrackingId = 0;
    [SerializeField] GameObject handCursorObj;
    GameObject handCursor;
    GameObject currentLetter;
    EnterNameHandCursor handCursorScript;
    bool letterEnterCooldown;
    bool initialized;
    bool playerInteracting;
    Animator camAnim;
    BodySourceManager bsm;
    GameManager gm;
    private Kinect.HandState rightHandState;
    private Kinect.HandState leftHandState;

    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();


    void Start()
    {
        bsm = BodySourceManager.instance;
        gm = GameManager.instance;
        camAnim = Camera.main.GetComponent<Animator>();
        gm._PlayerName.text = LoadLastPlayerName();
    }

    void Update()
    {
        if (enteringName)
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

            /// If there is a player
            if (player != null)
            {
                if (!playerInteracting)
                {
                    Debug.Log("Found player setting interaction to true");
                    playerInteracting = true;
                }

                /// if the list of bodies doesnt contain this player then create them
                if (!bodies.ContainsKey(player.TrackingId))
                {
                    bodies[player.TrackingId] = bsm.CreateBodyObject(player.TrackingId, playerSpawnPosition);
                    initialized = false;
                }

                /// Update the position of the players hands every third frame
                if (Time.frameCount % 3 == 0)
                {
                    RefreshHandPosition(player, bodies[player.TrackingId]);
                }

                /// Update the state of the players hand in every frame
                RefreshHandState(player);
            }

            if (handCursor != null)
            {
                handCursor.transform.position = Vector3.MoveTowards(handCursor.transform.position, des, speed * Time.deltaTime);
            }

            if (Time.frameCount % 15 == 0)
            {
                RefreshLetters();
            }

            /// If you do not have a player
            if (player == null && playerInteracting)
            {
                Debug.Log("Can no longer find player!");
                ResetEnterNameArea(false);
                playerInteracting = false;
            }
        }
    }

    public float speed = .5f;

    private Vector3 start;
    private Vector3 des;
    private float fraction = 0;

    /// <summary>
    /// Refreshes the current body updating 
    /// the position of each of the limbs
    /// </summary>
    /// <param name="body">The Body to Update</param>
    /// <param name="bodyObject">The Body object holding all the pieces</param>
    private void RefreshHandPosition(Kinect.Body body, GameObject bodyObject)
    {
        if (!initialized)
        {
            handCursor = Instantiate(handCursorObj, bodyObject.transform.position, Quaternion.identity);
            handCursorScript = handCursor.GetComponent<EnterNameHandCursor>();
            playerSpawnPosition.localEulerAngles = new Vector3(0, playerSpawnPosition.localEulerAngles.y - 90, playerSpawnPosition.localEulerAngles.z + 90);
            initialized = true;
        }

        Kinect.Joint rh = body.Joints[Kinect.JointType.HandRight];

        Transform currentJointObj = bodyObject.transform.Find(Kinect.JointType.HandRight.ToString());

        /// Find the Vector3 Position of this joint
        Vector3 currentJointPosition = BodySourceManager.GetVector3FromJoint(rh);

        currentJointObj.localPosition = new Vector3(currentJointPosition.x * -1, currentJointPosition.y, currentJointPosition.z);

        Vector3 handPos = (currentJointObj.position - perspectiveObj.position) * 0.5f + perspectiveObj.position;
        des = new Vector3(handPos.x, 2, handPos.z);


    }

    private void RefreshHandState(Kinect.Body body)
    {
        rightHandState = body.HandRightState;
        leftHandState = body.HandLeftState;

        if (handCursor != null)
        {
            currentLetter = FindCurrentLetter();
        }

        if (currentLetter != null)
        {
            if (currentLetter.transform.Find("Cube").GetComponent<MeshRenderer>().material != hoverLetterMaterial)
            {
                currentLetter.transform.Find("Cube").GetComponent<MeshRenderer>().material = hoverLetterMaterial;
            }
        }

        if (rightHandState == Kinect.HandState.Closed && body.HandRightConfidence == Kinect.TrackingConfidence.High)
        {
            if (currentLetter != null && !letterEnterCooldown)
            {
                if (currentLetter.name == "Submit")
                {
                    gm._PlayerName.text = playerName.text;
                    SaveLastUserName(playerName.text);
                    camAnim.SetTrigger("nametomenu");
                    return;
                }

                StartCoroutine(EnterLetter(currentLetter.transform.Find("Cube").GetComponent<MeshRenderer>()));
                playerName.text += currentLetter.name.ToString();
            }
        }

        else if (leftHandState == Kinect.HandState.Closed && body.HandLeftConfidence == Kinect.TrackingConfidence.High)
        {
            if (!letterEnterCooldown)
            {
                StartCoroutine(DeleteLetter());
            }
        }
    }

    private GameObject FindCurrentLetter()
    {
        int menuMask = 1 << 13;

        RaycastHit hit;
        if (Physics.Raycast(handCursor.transform.position, handCursor.transform.position - Camera.main.transform.position, out hit, Mathf.Infinity, menuMask))
        {
            if (hit.collider.tag == "Letter")
            {

                return hit.collider.transform.parent.gameObject;
            }

            else if (hit.collider.tag == "Submit")
            {

                return hit.collider.transform.parent.gameObject;
            }
        }

        return null;
    }

    private void RefreshLetters()
    {
        foreach (var letter in letters)
        {
            if (letter != currentLetter && letter.transform.Find("Cube").GetComponent<MeshRenderer>().material != defaultLetterMaterial)
            {
                letter.transform.Find("Cube").GetComponent<MeshRenderer>().material = defaultLetterMaterial;
            }
        }
    }

    /// <summary>
    /// Deletes the previous letter and ensures
    /// a cooldown before being able to repeat this action
    /// </summary>
    /// <returns></returns>
    IEnumerator DeleteLetter()
    {
        letterEnterCooldown = true;

        if (playerName.text.Length > 0)
        {
            playerName.text = playerName.text.Remove(playerName.text.Length - 1);
        }

        yield return new WaitForSeconds(0.7f);
        letterEnterCooldown = false;
    }

    /// <summary>
    /// Enters a letter into the temporary name
    /// and certifies that there is a cooldown 
    /// before being able to enter another one
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    IEnumerator EnterLetter(MeshRenderer mesh)
    {
        letterEnterCooldown = true;
        mesh.material = selectedLetterMaterial;
        yield return new WaitForSeconds(0.3f);
        mesh.material = defaultLetterMaterial;

        yield return new WaitForSeconds(0.7f);
        letterEnterCooldown = false;
    }

    /// <summary>
    /// Resets the enteringName menu
    /// area when the user navigates away from it
    /// </summary>
    public void ResetEnterNameArea(bool playerLeftMenu = true)
    {
        Destroy(handCursor);
        bodies.Clear();
        initialized = false;
        currTrackingId = 0;
        playerSpawnPosition.localEulerAngles = new Vector3(0, 0, 0);

        if (playerLeftMenu)
        {
            playerName.text = "";
        }
    }

    /// <summary>
    /// Saves the last submitted name
    /// to the playerprefs (file on users computer)
    /// </summary>
    /// <param name="name"></param>
    void SaveLastUserName(string name)
    {
        PlayerPrefs.SetString("LastPlayerName", name);
    }

    /// <summary>
    /// Loads the last submitted playername
    /// if there is one
    /// </summary>
    /// <returns></returns>
    string LoadLastPlayerName()
    {
        if (PlayerPrefs.HasKey("LastPlayerName"))
        {
            return PlayerPrefs.GetString("LastPlayerName");
        }

        return null;
    }
}
