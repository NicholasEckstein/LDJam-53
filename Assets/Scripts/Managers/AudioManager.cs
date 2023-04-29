using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    private AudioSource m_musicSource = null;
    private AudioSource m_sfxSource = null;

    public override void InitManager() 
    {
        if(m_musicSource == null)
            m_musicSource = gameObject.AddComponent<AudioSource>();
        if(m_sfxSource == null)
            m_sfxSource = gameObject.AddComponent<AudioSource>();

        m_musicSource.volume = GameManager.Instance.MusicVolume;
        m_sfxSource.volume = GameManager.Instance.SFXVolume;
    }

    public void PlayMusic(AudioClip a_audioClip, bool a_restartTrackOnPlay = true, bool a_loop = true)
    {
        if (a_audioClip == null)
            return;

        if (!a_restartTrackOnPlay)
        {
            if (m_musicSource.clip == a_audioClip)
                return;
        }

        m_musicSource.clip = a_audioClip;
        m_musicSource.loop = a_loop;
        m_musicSource.Play();
    }

    public void PlaySFX(AudioClip a_audioClip, bool a_loop = false)
    {
        if (a_audioClip == null)
            return;
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
