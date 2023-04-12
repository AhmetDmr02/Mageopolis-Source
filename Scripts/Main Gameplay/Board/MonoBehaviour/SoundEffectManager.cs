using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager instance;
    [SerializeField] private AudioSource diceLandSound;
    [SerializeField] private AudioSource diceLaunchSound;
    [SerializeField] private AudioSource diceImpactSound;
    [SerializeField] private AudioSource flareUpSound;
    public AudioSource MainGameMusic;
    [SerializeField] private AudioClip _clickSound1;
    public AudioClip clickSound1 => _clickSound1;

    [SerializeField] private AudioClip _clickSound2;
    public AudioClip clickSound2 => _clickSound2;

    [SerializeField] private AudioClip bounceDefault;
    public AudioClip BounceDefault => bounceDefault;

    [SerializeField] private AudioClip fireballSound;
    public AudioClip FireballSound => fireballSound;

    [SerializeField] private AudioClip fireballImpact;
    public AudioClip FireballImpact => fireballImpact;

    [SerializeField] private AudioClip fireballExplosion;
    public AudioClip FireballExplosion => fireballExplosion;

    [SerializeField] private AudioClip earthQuakeShort;
    public AudioClip EarthQuakeShort => earthQuakeShort;

    [SerializeField] private AudioClip earthQuakeMiddleCharger;
    public AudioClip EarthQuakeMiddleCharger => earthQuakeMiddleCharger;
    [SerializeField] private AudioClip earthQuakeLong;
    public AudioClip EarthQuakeLong => earthQuakeLong;


    [SerializeField] private AudioClip blobBlossom;
    public AudioClip BlobBlossom => blobBlossom;

    [SerializeField] private GameObject dummySource;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    public void PlayDiceLandSound() => diceLandSound.Play();
    public void PlayDiceImpactSound() => diceImpactSound.Play();
    public void PlayDiceLaunchSound() => diceLaunchSound.Play();
    public void PlayFlareUpSound() => flareUpSound.Play();
    public void PlayClickSound()
    {
        CreateDummyAudioAt(clickSound1, Vector3.zero, 1, 1, 0.4f, 0);
    }
    public void CreateDummyAudioAt(AudioClip bounceSound, Vector3 playLocation, float minPitch, float maxPitch, float volume, float _2DTO3D)
    {
        GameObject go = Instantiate(dummySource);
        go.transform.position = playLocation;
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.clip = bounceSound;
        audioSource.volume = volume;
        float fixedFloat = _2DTO3D > 1 ? 1 : _2DTO3D;
        fixedFloat = fixedFloat < 0 ? 0 : fixedFloat;
        audioSource.spatialBlend = fixedFloat;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
        go.AddComponent<DestroyMe>().destroyMeAfterSeconds(bounceSound.length + 2);
    }
}
