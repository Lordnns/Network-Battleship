using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioPool audioPool;
    
    
    //creating an audioManger singleton to play some audio from anywhere without having to ref an audio source
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioPool = gameObject.GetComponent<AudioPool>();
    }

    public void PlayOneShotSound(AudioClip clip)
    {
        audioPool.PlaySound(clip);
        //audioSource.PlayOneShot(clip);
    }
}
