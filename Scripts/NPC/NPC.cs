using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.BoarShroom.RPGtest
{

    public class NPC : MonoBehaviour
    {
        public AudioSource audio;
        public Animator anim;

        void Start()
        {
            if (GetComponent<Animator>()) { anim = GetComponent<Animator>(); }

            if (GetComponent<AudioSource>()) { audio = GetComponent<AudioSource>();  anim.SetBool("animation", true); }
        }

    }
}
