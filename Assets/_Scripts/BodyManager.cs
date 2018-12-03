using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class BodyManager : MonoBehaviour
{
    public static BodyManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    [SerializeField] Material boneMaterial;
    [SerializeField] GameObject jointObject;
    [SerializeField] Color failColor;
    [SerializeField] Color winColor;
    [SerializeField] bool colliding;
    [SerializeField] Transform playerSpawnPosition;

    [SerializeField] ulong currTrackingId = 0;

    [Tooltip("Width of the lines drawn by the line renderer")]
    [SerializeField] float lineWidth;

    private Dictionary<ulong, GameObject> bodies = new Dictionary<ulong, GameObject>();
    BodySourceManager bsm;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };


    private GameObject playerBody;
    /// <summary>
    /// needed by the ObjectManager to determine
    /// if it is crossing the player
    /// </summary>
    public Transform MyPlayerSpawnPosition
    {
        get
        {
            return playerSpawnPosition;
        }
    }

    public GameObject MyPlayerBody
    {
        get
        {
            return playerBody;
        }
    }
    GameManager gm;

    void Start()
    {
        bsm = BodySourceManager.instance;
        gm = GameManager.instance;
    }

    void Update()
    {
        if (gm.readyToPlay)
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

            /// If body doesn't exist create it
            if (player != null)
            {
                if (!bodies.ContainsKey(player.TrackingId))
                {
                    bodies[player.TrackingId] = CreateBodyObject(player.TrackingId);
                }

                RefreshBodyObject(player, bodies[player.TrackingId]);
                DrawBodyLines(player, bodies[player.TrackingId]);
            }
        }
    }

    /// <summary>
    /// Creates a new body and provides it 
    /// with a name or identification
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = Instantiate(jointObject);

            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = boneMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        body.transform.position = MyPlayerSpawnPosition.position;
        body.transform.SetParent(MyPlayerSpawnPosition);

        playerBody = body;

        return body;
    }

    /// <summary>
    /// Refreshes the current body updating 
    /// the position of each of the limbs
    /// </summary>
    /// <param name="body">The Body to Update</param>
    /// <param name="bodyObject">The Body object holding all the pieces</param>
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            ///Where should the line be drawn from?
            Kinect.Joint currentJoint = body.Joints[jt];

            ///Find the child object that matches 
            ///the current joints name 
            Transform currentJointObj = bodyObject.transform.Find(jt.ToString());

            /// Find the Vector3 Position of this joint
            Vector3 currentJointPosition = GetVector3FromJoint(currentJoint);

            currentJointObj.localPosition = new Vector3(currentJointPosition.x*-1, currentJointPosition.y, currentJointPosition.z);

        }
    }

    /// <summary>
    /// Draws the lines between the 
    /// joints the create a visible player
    /// </summary>
    /// <param name="body"></param>
    /// <param name="bodyObject"></param>
    private void DrawBodyLines(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Transform currentJoint = bodyObject.transform.Find(jt.ToString());
            Transform nextJoint = null;

            if (_BoneMap.ContainsKey(jt))
            {
                nextJoint = bodyObject.transform.Find(_BoneMap[jt].ToString());
            }

            LineRenderer lr = currentJoint.GetComponent<LineRenderer>();

            if (nextJoint != null)
            {
                lr.SetPosition(0, currentJoint.position);
                colliding = currentJoint.GetComponent<JointManager>().isHitting;

                lr.SetPosition(1, nextJoint.position);

                if (colliding)
                {
                    lr.startColor = failColor;
                    lr.endColor = failColor;
                }
                else if (!colliding)
                {
                    lr.startColor = winColor;
                    lr.endColor = winColor;
                }
            }

            else
            {
                lr.enabled = false;
            }
        }

       
    }

    public void PrepareForNextGame()
    {
        Destroy(playerBody);
        bodies.Clear();
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}



