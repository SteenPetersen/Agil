using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour {

    public static ParticleManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    [SerializeField] ParticleSystem[] particles;

    public ParticleSystem GetParticle(string n)
    {
        foreach (var p in particles)
        {
            if (p.name == n)
            {
                return p;
            }
        }

        return null;

    }

}
