using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointManager : MonoBehaviour {
    [SerializeField] private Material hitMaterial;

    [SerializeField] private Material defaultMaterial;

    Renderer rend;
    public bool isHitting;

    private void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (Time.frameCount % 3 == 0)
        {
            if (col.tag == "Wall")
            {
                if (!isHitting)
                {
                    GameManager.jointsColliding += 1;
                }

                rend.material = hitMaterial;
                Debug.Log(col.name);
                isHitting = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (Time.frameCount % 1 == 0)
        {
            if (col.tag == "Wall")
            {
                if (isHitting)
                {
                    GameManager.jointsColliding -= 1;
                }

                Debug.Log("Exiting");
                rend.material = defaultMaterial;
                isHitting = false;
            }
        }

    }
}
