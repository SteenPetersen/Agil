using UnityEngine;
using System.Collections;
using Kinect = Windows.Kinect;

public class BodySourceManager : MonoBehaviour
{
    [SerializeField] GameObject jointObject;

    public static BodySourceManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    private Kinect.KinectSensor _Sensor;
    private Kinect.BodyFrameReader _Reader;
    private Kinect.Body[] _Data = null;


    /// <summary>
    /// Returns all the body parts
    /// </summary>
    /// <returns></returns>
    public Kinect.Body[] GetData()
    {
        return _Data;
    }

    void Start()
    {
        _Sensor = Kinect.KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Kinect.Body[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);

                frame.Dispose();
                frame = null;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }

    /// <summary>
    /// Returns a single body to avoid 
    /// multitracking
    /// </summary>
    /// <param name="data">Data in which to search for the body</param>
    /// <param name="currTrackingId">The current tracking ID we are looking for</param>
    /// <returns></returns>
    public Kinect.Body GetActiveBody(Kinect.Body[] data, ulong currTrackingId)
    {
        if (currTrackingId <= 0)
        {
            Debug.Log("current tracking ID is equal to or under 0");
            foreach (var body in data)
            {
                if (body.IsTracked)
                {
                    currTrackingId = body.TrackingId;
                    return body;
                }
            }

            return null;
        }

        else
        {
            foreach (Kinect.Body body in data)
            {
                if (body.IsTracked && body.TrackingId == currTrackingId)
                {
                    Debug.Log("returning same body");
                    return body;
                }
            }
        }

        Debug.Log("reaching end!");

        return null;
    }

    /// <summary>
    /// Creates a new body and provides it 
    /// with a name or identification
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public GameObject CreateBodyObject(ulong id, Transform spawnPosObj)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = Instantiate(jointObject);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }

        body.transform.position = spawnPosObj.position;
        body.transform.SetParent(spawnPosObj);

        return body;
    }

    public static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
