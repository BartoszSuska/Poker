using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Com.BoarShroom.RPGtest
{
    public class PlayerInfo : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameChanged))] public string playerName;
        [SyncVar(hook = nameof(OnModelChanged))] public int playerModel;
        [SerializeField] TextMesh nicknameText;
        [SerializeField] GameObject[] allModels;
        [SerializeField] Camera thisCamera;

        void Update()
        {
            if (!this.isLocalPlayer)
            {
                if (nicknameText)
                {
                    //nicknameText.transform.LookAt(thisCamera.transform);
                }
            }

        }

        public override void OnStartLocalPlayer()
        {
            string nickname = PlayerPrefs.GetString("Nickname");
            int model = PlayerPrefs.GetInt("Model");

            CmdSetupPlayer(nickname, model);
        }

        [Command]
        public void CmdSetupPlayer(string _name, int _model)
        {
            playerModel = _model;
            playerName = _name;
        }

        void OnNameChanged(string _Old, string _New)
        {
            nicknameText.text = playerName;
        }

        void OnModelChanged(int _Old, int _New)
        {
            //playerModel.SetActive(true);
            Debug.Log("onmodelchanged");
            allModels[playerModel].SetActive(true);
        }
    }
}
