using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EchoParticleManager : MonoBehaviour
{

    [SerializeField] MeshRenderer mesh;

    [Header("Colors")]
    [SerializeField] Material normalColor;
    [SerializeField] Material igniteColor;

    [Header("Particles")]
    [SerializeField] protected TrailRenderer echoTrail;
    [SerializeField] protected Gradient regularGradient;
    [SerializeField] protected Gradient ignitionGradient;
    [SerializeField] protected ParticleSystem ignitionTravelParticles;
    [SerializeField] protected ParticleSystem ignitionDeflectParticles;
    [SerializeField] protected ParticleSystem invulnParticles;



    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public void PlayInvulnParticles()
    {        
        var newInvulnParticles = Instantiate(invulnParticles);
        newInvulnParticles.transform.position = transform.position;
        newInvulnParticles.Play();
    }



    public void PlayIgnitionParticles()
    {
        ignitionDeflectParticles.Play();
    }



    public virtual void OnSpeedUpdated(float newSpeed, bool isIgnited)
    {
        if (igniteColor == null || normalColor == null || ignitionGradient == null || regularGradient == null) return;
        mesh.material = isIgnited ? igniteColor : normalColor;
        echoTrail.colorGradient = isIgnited ? ignitionGradient : regularGradient;
        if (!isIgnited)
        {
            ignitionTravelParticles.Stop();
        }
        else if (!ignitionTravelParticles.isPlaying)
        {
            ignitionTravelParticles.Play();
        }
    }

}
