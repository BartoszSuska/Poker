using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace Com.BoarShroom.RPGtest
{
    public class PlayerInteractions : NetworkBehaviour
    {
        public Transform[] cardsPositionsInHand;
        [SerializeField] Camera cam;
        float zoomSpeed = 5f;
        float normalFOV = 70f;
        float zoomFOV = 25f;
        Manager manager;
        bool host;
        public SyncList<GameObject> cardsInHand = new SyncList<GameObject>();
        [SyncVar] public bool turn;
        [SyncVar] public bool gameEnded;
        [SyncVar] public bool allIn;
        [SyncVar] public bool fold;
        [SyncVar] public bool lastPlayer;
        [SyncVar] public bool ready;
        [SyncVar] public int movesThisRound;
        [SyncVar] public int actualBid;
        [SyncVar] public int highestBid;
        [SyncVar] public int actualMoney;
        [SyncVar] public int beforeBid;
        public float actualBidFloat;
        [SerializeField] TMP_Text actualBidText;
        GameObject canvasKeyBindings;
        public GameObject canvasPause;
        public bool allInThisRound;
        [SerializeField] GameObject[] chipsTypes;
        GameObject actualMoneyObject;
        GameObject actualBidObject;
        [SerializeField] Transform actualMoneyTransform;
        [SerializeField] Transform actualBidTransform;
        [SyncVar] public bool cardsDealed;
        [SyncVar] public string winnerAnnouncment;
        public int pokerHandNumber; //higher number beeter poker hand
        public int biggestCardInPokerHand; //example ace in flush
        public int biggestCard;
        public List<int> pokerHandsCards = new List<int>();
        public List<int> biggestCards = new List<int>();
        [SyncVar] public int spawnPointIndex;
        [SyncVar] public Transform spawnPoint;

        void Start()
        {
            if (this.isLocalPlayer)
            {
                canvasKeyBindings = GameObject.FindGameObjectWithTag("KeyBindings");
                actualBidText = canvasKeyBindings.transform.Find("RiseAction/Mask/RiseText").GetComponent<TMP_Text>();
                CmdStarting();
                canvasPause = GameObject.FindGameObjectWithTag("Menu");
            }

            if (this.isServer)
            {
                manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
                manager.players.Add(this.gameObject);
                spawnPointIndex = NetworkManager.startPositionIndex;
                spawnPoint = NetworkManager.startPositions[spawnPointIndex];
                NetworkManager.UnRegisterStartPosition(NetworkManager.startPositions[spawnPointIndex]);
            }
        }

        void Update()
        {            
            if (this.isServer)
            {
                if (manager.ready && Input.GetKeyDown(KeyCode.Space) && !manager.started)
                {
                    manager.started = true;
                    manager.countPlayers = true;
                    manager.deal = true;
                }
            }

            if (this.isLocalPlayer)
            {
                if (cardsDealed)
                {
                    canvasPause.SetActive(false);
                }
                else
                {
                    canvasPause.SetActive(true);
                    canvasPause.GetComponentInChildren<TMP_Text>().text = winnerAnnouncment;
                }

                if(turn && ready)
                {
                    canvasKeyBindings.SetActive(true);
                }
                else
                {
                    canvasKeyBindings.SetActive(false);
                }

                if (Input.GetKey(KeyCode.Q) && cam.fieldOfView >= zoomFOV)
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomFOV, zoomSpeed * Time.deltaTime);
                }
                else if (cam.fieldOfView < normalFOV)
                {
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, normalFOV, zoomSpeed * Time.deltaTime);
                }

                if(turn && ready)
                {
                    actualBidFloat += Input.GetAxis("Mouse ScrollWheel");
                    int moneyDiff = highestBid - actualBid;
                    if(actualMoney > moneyDiff)
                    {
                        actualBidFloat = Mathf.Clamp(actualBidFloat, 25, actualMoney - actualBid);
                    }
                    else
                    {
                        actualBidFloat = Mathf.Clamp(actualBidFloat, 0, 0);
                    }
                    actualBidText.text = "3 to Rise: " + Mathf.RoundToInt(actualBidFloat).ToString();

                    if (Input.GetKeyDown(KeyCode.Alpha1)) // Check Action
                    {
                        CmdCheckTurn();
                    }
                    else if(Input.GetKeyDown(KeyCode.Alpha2)) // Call Action
                    {
                        CmdCallTurn();
                    }
                    else if(Input.GetKeyDown(KeyCode.Alpha3)) // Raise Action
                    {
                        int howMuch = Mathf.RoundToInt(actualBidFloat);
                        CmdRaiseTurn(howMuch);
                    }
                    else if(Input.GetKeyDown(KeyCode.Alpha4)) // All In Action
                    {
                        CmdAllIn();
                    }
                    else if(Input.GetKeyDown(KeyCode.Alpha5)) // Fold Turn
                    {
                        CmdFoldTurn();
                    }
                }
            }
        }

        public override void OnStopClient()
        {
            NetworkManager.RegisterStartPosition(spawnPoint);
            manager.players.Remove(this.gameObject);
        }

        [Command] public void CmdCheckTurn()
        {
            if(actualBid == highestBid)
            {
                beforeBid = actualBid;
                AllTurns();
            }
        }

        [Command] public void CmdCallTurn()
        {
            beforeBid = actualBid;

            if (actualMoney >= highestBid - beforeBid)
            {
                actualBid = highestBid;
                AllTurns();
            }
        }

        [Command] public void CmdRaiseTurn(int howMuch)
        {
            if(actualMoney >= howMuch)
            {
                beforeBid = actualBid;
                actualBid = howMuch + highestBid;
                AllTurns();
            }
        }

        [Command] public void CmdAllIn()
        {
            if(actualMoney > 0)
            {
                beforeBid = actualBid;
                actualBid += actualMoney;
                AllTurns();
            }
        }

        [Command] public void CmdFoldTurn()
        {
            fold = true;
            turn = false;
        }

        [Command] public void CmdStarting()
        {
            actualMoney = 1000;
        }

        void AllTurns()
        {
            int minusMoney = actualBid - beforeBid;
            actualMoney -= minusMoney;
            movesThisRound++;
            if(actualMoney <= 0)
            {
                allIn = true;
            }

            turn = false;
        }

        public void SpawnChipsObjects()
        {
            Destroy(actualMoneyObject);
            Destroy(actualBidObject);

            NetworkServer.Destroy(actualMoneyObject);
            NetworkServer.Destroy(actualBidObject);

            actualMoneyObject = Instantiate(chipsTypes[CheckAmount(actualMoney)], actualMoneyTransform.position, actualMoneyTransform.rotation);
            actualBidObject = Instantiate(chipsTypes[CheckAmount(actualBid)], actualBidTransform.position, actualBidTransform.rotation);

            NetworkServer.Spawn(actualMoneyObject);
            NetworkServer.Spawn(actualBidObject);

            if(actualMoneyObject.GetComponent<Money>()) actualMoneyObject.GetComponent<Money>().amount = actualMoney;
            if(actualBidObject.GetComponent<Money>()) actualBidObject.GetComponent<Money>().amount = actualBid;
        }

        int CheckAmount(int amount) // returns prefix for spawn chip object
        {
            if(amount <= 0)
            {
                return 0;
            }
            else if(amount <= 100)
            {
                return 1;
            }
            else if(amount <= 200)
            {
                return 2;
            }
            else if(amount <= 300)
            {
                return 3;
            }
            else if(amount <= 400)
            {
                return 4;
            }
            else if(amount <= 500)
            {
                return 5;
            }
            else if(amount <= 600)
            {
                return 6;
            }
            else if(amount <= 700)
            {
                return 7;
            }
            else if(amount <= 800)
            {
                return 8;
            }
            else if(amount <= 900)
            {
                return 9;
            }
            else
            {
                return 10;
            }

        }
    }
}
