using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    [SerializeField] AudioSource audioSource;

    AudioClip wallBreak, success, bounce, heartBeat, menu_hover;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        wallBreak = Resources.Load<AudioClip>("SoundFX/" + "wall_break");
        success = Resources.Load<AudioClip>("SoundFX/" + "Success");
        bounce = Resources.Load<AudioClip>("SoundFX/" + "bounce");
        heartBeat = Resources.Load<AudioClip>("SoundFX/" + "danger");
        menu_hover = Resources.Load<AudioClip>("SoundFX/" + "menu_hover");
    }

    public void PlaySound(string sound)
    {
        switch (sound)
        {
            case "wall_break":
                audioSource.PlayOneShot(wallBreak);
                    break;

            case "success":
                audioSource.PlayOneShot(success);
                break;

            case "bounce":
                audioSource.PlayOneShot(bounce);
                break;

            case "heartBeat":
                audioSource.PlayOneShot(heartBeat);
                break;

            case "menu_hover":
                audioSource.PlayOneShot(menu_hover);
                break;

            default:
                break;
        }
    }
}
