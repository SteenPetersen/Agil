using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class EnterNameHandCursor : MonoBehaviour {

    private Kinect.HandState handState;

    public GameObject DetectKey()
    {
        Debug.DrawRay(transform.position, Vector3.down, Color.cyan, 2);

        int menuMask = 1 << 13;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, menuMask))
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

}
