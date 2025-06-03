using BNG;
using UnityEngine;
using UnityEngine.Events;

public class BubbleGun : MonoBehaviour
{
    Animator m_Animator;
    
    [SerializeField] ParticleSystem m_BubbleParticleSystem = null;

    const string k_AnimTriggerDown = "TriggerDown";
    const string k_AnimTriggerUp = "TriggerUp";
    const float k_HeldThreshold = 0.1f;

    float m_TriggerHeldTime;
    bool m_TriggerDown;

    public GrabbableUnityEvents GrabbableUnityEvents;

    protected void Start()
    {
        GrabbableUnityEvents = GetComponent<GrabbableUnityEvents>();
        m_Animator = GetComponent<Animator>();
        GrabbableUnityEvents.onRelease.AddListener(DroppedGun);
        GrabbableUnityEvents.onTriggerDown.AddListener(TriggerPulled);
        GrabbableUnityEvents.onTriggerUp.AddListener(TriggerReleased);
    }

    private void TriggerReleased()
    {
        m_Animator.SetTrigger(k_AnimTriggerUp);
        m_TriggerDown = false;
        m_TriggerHeldTime = 0f;
        m_BubbleParticleSystem.Stop();
    }

    private void TriggerPulled()
    {
        m_Animator.SetTrigger(k_AnimTriggerDown);
        m_TriggerDown = true;
    }

    private void DroppedGun()
    {
        // In case the gun is dropped while in use.
        m_Animator.SetTrigger(k_AnimTriggerUp);

        m_TriggerDown = false;
        m_TriggerHeldTime = 0f;
        m_BubbleParticleSystem.Stop();
    }

    protected void Update()
    {
        if (m_TriggerDown)
        {
            m_TriggerHeldTime += Time.deltaTime;

            if (m_TriggerHeldTime >= k_HeldThreshold)
            {
                if (!m_BubbleParticleSystem.isPlaying)
                {
                    m_BubbleParticleSystem.Play();    
                }
            }
        }
    }

    public void ShootEvent()
    {
        m_BubbleParticleSystem.Emit(1);
    }
}
