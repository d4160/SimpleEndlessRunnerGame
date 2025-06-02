using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource _aSource;

    public void PlayOneShot(AudioClip clip)
    {
        _aSource.PlayOneShot(clip);
    }
}
