using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.BoarShroom.RPGtest
{
    public class Musician : NPC
    {
        [SerializeField] int index;
        [SerializeField] AudioClip[] songs;
        AudioListener audioListener;
        float lagSong;

        void Update()
        {
            audioListener = (AudioListener)FindObjectOfType(typeof(AudioListener));

            anim.speed = GetAveragedVolume() * Vector3.Distance(transform.position, audioListener.transform.position) * 10;

            if(GetAveragedVolume() <= 0)
            {
                lagSong += Time.deltaTime;
            }
            else
            {
                lagSong = 0;
            }

            if(audio.time >= audio.clip.length || lagSong >= 11)
            {
                lagSong = 0;
                StartCoroutine(WaitForNextSong());
            }
        }

        float GetAveragedVolume()
        {
            float[] data = new float[256];
            float a = 0;
            audio.GetOutputData(data, 0);
            foreach (float s in data)
            {
                a += Mathf.Abs(s);
            }

            return a / 256;
        }

        IEnumerator WaitForNextSong()
        {
            anim.SetBool("animation", false);
            yield return new WaitForSeconds(10);
            index++;
            if(index >= songs.Length)
            {
                index = 0;
            }
            audio.clip = songs[index];
            audio.Play();
            anim.SetBool("animation", true);
        }
    }
}
