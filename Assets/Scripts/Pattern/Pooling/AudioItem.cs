using UnityEngine;
/// <summary>
/// Audio item used to play sound instead of instantiate a oneShot audio 
/// </summary>
public class AudioItem : PoolItem
{
    private AudioSource audioSource;
    
    //get its own audio source
    private void OnEnable()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    //play a sound and deactivate the pool item after the clip duration
    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        var duration = clip.length;
        audioSource.Play();
        Invoke(nameof(Remove), duration);
    }
    
    
}
