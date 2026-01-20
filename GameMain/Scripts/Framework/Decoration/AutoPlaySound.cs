using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPlaySound : MonoBehaviour
{
    public float playSoundInterval = 1.0f;
    public string soundName;
    [Range(0.0f, 1.0f)]
    public float volumn = 1.0f;

    private float accumulatedTime;
    private int audioIDCache = -1;
    private bool isMute = true;
    void Update()
    {
        accumulatedTime += Time.deltaTime;
        if (accumulatedTime > playSoundInterval)
        {
            accumulatedTime = 0;
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (GameManager.Sound != null)
        {
            if (isMute)
                return;
            if (audioIDCache >= 0)
                GameManager.Sound.StopSound(audioIDCache);

            PlaySoundParams soundParams = PlaySoundParams.Create();
            soundParams.VolumeInSoundGroup = volumn;
            audioIDCache = (int)GameManager.Sound.PlayAudio(soundName, soundParams);
        }
    }

    public void MuteSound()
    {
        if (GameManager.Sound != null)
        {
            if (audioIDCache >= 0)
                GameManager.Sound.StopSound(audioIDCache);
        }
        isMute = true;
    }

    public void ResumeSound()
    {
        if (GameManager.Sound != null)
        {
            if (audioIDCache >= 0)
                GameManager.Sound.StopSound(audioIDCache);

            PlaySoundParams soundParams = PlaySoundParams.Create();
            soundParams.VolumeInSoundGroup = volumn;
            audioIDCache = (int)GameManager.Sound.PlayAudio(soundName, soundParams);
        }
        isMute = false;
    }

    private void OnDestroy()
    {
        MuteSound();
    }
}
