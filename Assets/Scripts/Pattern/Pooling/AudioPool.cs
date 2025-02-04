using UnityEngine;
/// <summary>
/// Class to store and call audio pool item
/// </summary>
public class AudioPool : Pool
{
    public void PlaySound(AudioClip clip)
    {
        AudioItem audioItem = GetAPoolObject() as AudioItem;
        audioItem.PlaySound(clip);
    }
}
