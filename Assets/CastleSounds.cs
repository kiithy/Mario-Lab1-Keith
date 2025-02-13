using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleSounds : MonoBehaviour
{
    public AudioSource bowserAudio;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayBowserDamagedSound()
    {
        bowserAudio.PlayOneShot(bowserAudio.clip);
        Debug.Log("Bowser Damaged Sound Played");
    }
}
