using UnityEngine;

public class MonolithSound : MonoBehaviour
{
    //This script created because of animation events can only call functions from its own gameobject
    private AudioSource monolithSound;

    private void Start()
    {
        monolithSound = this.gameObject.GetComponent<AudioSource>();
    }
    public void playMonolithSound()
    {
        monolithSound.Play();
    }
}
