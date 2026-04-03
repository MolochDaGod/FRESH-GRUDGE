using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class audiomaster : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource audiosource;
    public AudioClip defaultClip;  //the default clip that will play at start up

    void Start()
    {
        audiosource.clip = defaultClip;
        audiosource.Play();
        
    }
    

    public void play(AudioClip clip1){
        if (clip1.name == audiosource.clip.name && audiosource.isPlaying) { return; } //if were already playign the default clip
audiosource.clip = clip1;
        audiosource.Play();
}

public void stop(){
        if (defaultClip == audiosource.clip) {  return; }
        //audiosource.clip = defaultClip;//when exiting trigger can revert to default audioclip
        audiosource.Play();
    }

  
}