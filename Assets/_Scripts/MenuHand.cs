using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuHand : MonoBehaviour
{
    [SerializeField] private GameObject handCursor;
    [SerializeField] private Material hoverSelectedMaterial;
    [SerializeField] private Material notSelectedMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Transform cam;
    [SerializeField] private TextMeshPro letter;
    [SerializeField] private TextMeshPro nameField;
    private bool isHandTracked = true;
    private  BodySourceManager bodySrcManager;
    private JointType rightHand;
    private GameObject lastSelected;
    private Body[] bodies;
    private float startSceneTime;

    private HandState handState;

    void Awake()
    {
        StaticScores.LoadScores();
    }
    // Use this for initialization
    void Start ()
	{
	    rightHand = JointType.HandRight;

        bodySrcManager = BodySourceManager.instance;

	    if (nameField != null)
	    {
	        //nameField.text = StaticScores.LastName;
	    }

	    startSceneTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (bodySrcManager == null)
	    {
	        return;
	    }

	    bodies = bodySrcManager.GetData();

	    if (bodies == null)
	    {
	        return;
	    }

	    foreach (var body in bodies)
	    {
	        if (body == null)
	        {
                continue;
	        }

            if (body.IsTracked)
	        {
	            if (isHandTracked && handCursor != null)
	            {
	                isHandTracked = false;
	                handCursor = Instantiate(handCursor);
	            }
                if (handCursor != null)
                {
                    var handPos = body.Joints[rightHand].Position;
                    handCursor.transform.position = new Vector3(handPos.X * 20, handPos.Y * 20, -1);
                    handState = body.HandRightState;
                }

	        }
	        
	    }

        if (handCursor != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(handCursor.transform.position, handCursor.transform.position - cam.position, out hit, Mathf.Infinity))
            {

                if (lastSelected != null)
                {
                    lastSelected.GetComponent<Renderer>().material = notSelectedMaterial;
                }
                lastSelected = hit.collider.gameObject;
                lastSelected.gameObject.GetComponent<Renderer>().material = hoverSelectedMaterial;
                if (handState == HandState.Closed && Time.time > startSceneTime + 1)
                {
                    lastSelected.gameObject.GetComponent<Renderer>().material = selectedMaterial;
                    if (lastSelected.tag == "StartButton")
                    {
                        SceneManager.LoadScene("EnterName");
                    }
                    else if (lastSelected.tag == "PlayButton")
                    {
                        //StaticScores.SetLastName(nameField.text);
                        SceneManager.LoadScene("Test_Scene");
                    }
                    else if (lastSelected.tag == "HighButton")
                        SceneManager.LoadScene("Highscore");
                    else if (lastSelected.tag == "StartButton")
                        SceneManager.LoadScene("EnterName");
                    else if (lastSelected.tag == "MenuButton")
                        SceneManager.LoadScene("MenuScene");
                    else if (lastSelected.tag == "LeftButton" || lastSelected.tag == "RightButton")
                        LeftOrRight(lastSelected.tag);
                    else if (lastSelected.tag == "EnterButton" || lastSelected.tag == "BackButton")
                        EnterOrBack(lastSelected.tag);

                }
            }


        }
        else
        {
            if (lastSelected != null)
            {
                lastSelected.GetComponent<Renderer>().material = notSelectedMaterial;
            }
        }

    }

    private float lastTime = 0;
    void LeftOrRight(string button)
    {
        if (letter != null && Time.time > lastTime+0.5f)
        {
            if (button == "RightButton")
            {
                char character = letter.text.ToCharArray()[0];
                if (character == 'Z')
                    character = 'A';
                else
                    character++;
                letter.text = character.ToString();
            }
            else if (button == "LeftButton")
            {
                char character = letter.text.ToCharArray()[0];
                if (character == 'A')
                    character = 'Z';
                else
                    character--;
                letter.text = character.ToString();
            }

            lastTime = Time.time;
        }
    }

    void EnterOrBack(string button)
    {
        if (nameField != null && Time.time > lastTime + 0.5f)
        {
            if (button == "EnterButton")
            {
                nameField.text = nameField.text + letter.text;
            }
            else if (button == "BackButton")
            {
                if (nameField.text != "")
                {
                    nameField.text = nameField.text.Remove(nameField.text.Length - 1);
                }
            }

            lastTime = Time.time;
        }
    }
}
