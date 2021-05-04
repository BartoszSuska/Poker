using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Com.BoarShroom.RPGtest
{
    public class Money : NetworkBehaviour
    {
        [SyncVar] public int amount;
        [SerializeField] TextMesh amountText;

        void Update()
        {
            if (amountText)
            {
                amountText.text = amount.ToString();
                amountText.transform.LookAt(Camera.main.transform);
            }
        }
    }
}
