using UnityEngine;

public class MainLandEffectPlayer : MonoBehaviour, ILandAnimations
{
    public bool enable;
    [SerializeField] private ParticleSystem particleSystem_;

    public void playBounceAnimation()
    {
        if (enable)
            particleSystem_.Play();
    }

    public void playHitAnimation()
    {
        return;
    }
}
