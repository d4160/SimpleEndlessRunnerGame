using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource _aSource;

    public void PlayAudio()
    {
        _aSource.Play();
    }
}
