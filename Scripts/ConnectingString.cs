using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Com.BoarShroom.RPGtest
{
    public class ConnectingString : MonoBehaviour
    {
        string connecting;
        float numberOfDots;
        string dots;

        void Start()
        {
            connecting = GetComponent<TMP_Text>().text;
            numberOfDots = 1;
        }

        void Update()
        {
            numberOfDots += Time.deltaTime;
            
            if(numberOfDots >= 1 && numberOfDots < 2)
            {
                dots = ".";
            }
            else if(numberOfDots >= 2 && numberOfDots < 3)
            {
                dots = "..";
            }
            else if(numberOfDots >= 3 && numberOfDots < 4)
            {
                dots = "...";
            }
            else if(numberOfDots >= 4)
            {
                numberOfDots = 1;
            }

            GetComponent<TMP_Text>().text = "Connecting" + dots;
        }
    }
}
