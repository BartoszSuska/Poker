using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Com.BoarShroom.RPGtest
{
    public class Card : NetworkBehaviour
    {
        public int color; //1=trefl 2=diament 3=serce 4=pik
        public int number; //14=a 11=j 12=q 13=k
        [SyncVar] public Transform toFollow;

        void Update()
        {
            if(this.isServer)
            {
                transform.position = toFollow.position;
                transform.rotation = toFollow.rotation;
            }
        }
    }
}

