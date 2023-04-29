using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    private AudioSource m_musicSource;
    private AudioSource m_sfxSource;

    public override void InitManager() 
    {
        m_musicSource = gameObject.AddComponent<AudioSource>();
        m_sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip a_audioClip, bool a_loop = true)
    {
        m_musicSource.clip = a_audioClip;
        m_musicSource.loop = a_loop;
        m_musicSource.Play();
    }

    public void PlaySFX(AudioClip a_audioClip, bool a_loop = false)
    {
        m_sfxSource.clip = a_audioClip;
        m_sfxSource.loop = a_loop;
        m_sfxSource.Play();
    }

    public void PauseMusic(bool a_pause)
    {
        if (a_pause)
            m_musicSource.Pause();
        else
            m_musicSource.UnPause();
    }

    public void StopMusic()
    {
        m_musicSource.Stop();
    }

    public void StopSFX()
    {
        m_sfxSource.Stop();
    }
}
