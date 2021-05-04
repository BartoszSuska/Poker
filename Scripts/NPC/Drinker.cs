using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.BoarShroom.RPGtest
{
    public class Drinker : NPC
    {
        public float waitBeforeAnimationStartMin;
        public float waitBeforeAnimationStartMax;
        public float waitBeforeAnimation;

        void Update()
        {
            waitBeforeAnimation -= Time.deltaTime;

            if (waitBeforeAnimation <= 0)
            {
                waitBeforeAnimation = Random.Range(waitBeforeAnimationStartMin, waitBeforeAnimationStartMax);
                anim.SetTrigger("animation");
            }

        }
    }
}
