using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Com.BoarShroom.RPGtest
{
    public class Manager : NetworkBehaviour
    {
        public List<GameObject> players = new List<GameObject>();
        public bool countPlayers;

        [SyncVar] public bool ready;
        [SyncVar] public bool started;

        public List<GameObject> cards;
        [SyncVar] public int actualCard;
        List<GameObject> cardsOnTable = new List<GameObject>();
        List<GameObject> cardsOnTableObjects = new List<GameObject>();
        List<GameObject> cardsInHandsObjects = new List<GameObject>();
        [SerializeField] GameObject[] cardsSpawners;
        [SyncVar] public bool deal;

        int actualPlayerTurn;
        int actualPlayerStarter;
        int actualSmallBlind;
        int actualBigBlind;
        int whichPlayerStart;

        public bool playerTurn;
        bool nextPlayerTrigger;
        int numberOfPlayersDidTurn;

        public int highestBid;
        int moneyToWin;
        int round;        

        void Start()
        {
            actualCard = 0;
            
            whichPlayerStart = 0;
        }

        void Update()
        {
            ready = false;

            if (players.Count > 1)
            {
                ready = true;
            }

            if (started)
            {

                NetworkManager.singleton.maxConnections = NetworkServer.connections.Count;

                if (deal)
                {
                    ClearTable();
                    DealCards();
                }

                if (playerTurn)
                {
                    NextPlayerTurn();
                }
            }
        }

        void ClearTable()
        {
            for (int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();
                if (PI.cardsInHand.Count == 2)
                {
                    PI.cardsInHand.Clear();
                }
            }

            for (int i = 0; i < cardsInHandsObjects.Count; i++)
            {
                Destroy(cardsInHandsObjects[i]);
                NetworkServer.Destroy(cardsInHandsObjects[i]);
            }
            cardsInHandsObjects.Clear();

            for (int i = 0; i < cardsOnTable.Count; i++)
            {
                Destroy(cardsOnTableObjects[i]);
                NetworkServer.Destroy(cardsOnTableObjects[i]);
            }
            cardsOnTableObjects.Clear();
            cardsOnTable.Clear();
        }

        void DealCards()
        {
            Shuffle();
            GiveCardsToPlayers();
            SetStartingPlayer();
            SetBlinds();
            players[actualPlayerStarter].GetComponent<PlayerInteractions>().turn = true;
            actualPlayerTurn = actualPlayerStarter;
            playerTurn = true;
            round = 0;
            deal = false;
        }

        public void Shuffle()
        {
            actualCard = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                GameObject obj = cards[i];
                int random = Random.Range(0, i);
                cards[i] = cards[random];
                cards[random] = obj;
            }
        }

        public void GiveCardsToPlayers()
        {
            int playersInGame = 0;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();

                if(PI.actualMoney <= 0)
                {
                    PI.gameEnded = true;
                }
                else
                {
                    PI.cardsDealed = true;
                    PI.gameEnded = false;
                    PI.fold = false;
                    PI.allIn = false;
                    PI.lastPlayer = false;
                    PI.turn = false;
                    PI.ready = ready;
                    playersInGame++;
                }

                for (int j = 0; j < 2; j++)
                {
                    PI.cardsInHand.Add(cards[actualCard]);
                    actualCard++;
                    GameObject card = Instantiate(PI.cardsInHand[j], PI.cardsPositionsInHand[j].position, PI.cardsPositionsInHand[j].rotation);
                    NetworkServer.Spawn(card, players[i]);
                    card.GetComponent<Card>().toFollow = PI.cardsPositionsInHand[j];
                    cardsInHandsObjects.Add(card);
                }
            }

            if(playersInGame == 1)
            {
                NetworkManager.singleton.StopServer();
            }
        }

        void SetStartingPlayer()
        {
            actualSmallBlind = whichPlayerStart;
            actualBigBlind = actualSmallBlind + 1;
            if (actualBigBlind >= players.Count) { actualBigBlind = 0; }
            actualPlayerStarter = actualBigBlind + 1;
            if (actualPlayerStarter >= players.Count) { actualPlayerStarter = 0; }
            whichPlayerStart++;
            if (whichPlayerStart >= players.Count) { whichPlayerStart = 0; }
        }

        void SetBlinds()
        {
            PlayerInteractions smallBlind = players[actualSmallBlind].GetComponent<PlayerInteractions>();
            smallBlind.actualBid = 25;
            smallBlind.actualMoney -= 25;

            PlayerInteractions bigBlind = players[actualBigBlind].GetComponent<PlayerInteractions>();
            bigBlind.actualBid = 50;
            bigBlind.actualMoney -= 50;

            highestBid = 50;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();
                PI.SpawnChipsObjects();
                PI.highestBid = highestBid;
            }
        }

        void NextPlayerTurn()
        {
            PlayerInteractions PI = players[actualPlayerTurn].GetComponent<PlayerInteractions>();

            if (PI == null)
            {
                NextPlayerTrigger();
            }
            else
            {
                CheckTurnVar(PI);
            }

            if (nextPlayerTrigger)
            {
                PI.turn = false;
                PI.SpawnChipsObjects();

                if (PI.actualBid > highestBid)
                {
                    highestBid = PI.actualBid;
                }

                if (PlayersReadyThisRound())
                {
                    NextRound();
                }

                NextPlayerTrigger();
            }
        }

        void CheckTurnVar(PlayerInteractions _PI)
        {
            if (_PI.fold || _PI.gameEnded || _PI.allIn || !_PI.turn)
            {
                nextPlayerTrigger = true;
            }

            int all = 0;
            int readyPlayers = 0;

            for(int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();
                if(PI != _PI)
                {
                    all++;
                    if(PI.fold || PI.gameEnded || PI.allIn)
                    {
                        readyPlayers++;
                    }
                }
            }

            if(readyPlayers == all && _PI.actualBid == highestBid)
            {
                _PI.lastPlayer = true;
                nextPlayerTrigger = true;
            }
        }

        void NextPlayerTrigger()
        {
            actualPlayerTurn++;

            if (actualPlayerTurn == players.Count)
            {
                actualPlayerTurn = 0;
            }

            PlayerInteractions PInext = players[actualPlayerTurn].GetComponent<PlayerInteractions>();

            PInext.highestBid = highestBid;

            if(!PInext.allIn && !PInext.fold && !PInext.gameEnded)
            {
                PInext.turn = true;
            }

            nextPlayerTrigger = false;
        }

        bool PlayersReadyThisRound()
        {
            int roundPlayers = 0;
            int readyPlayers = 0;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null)
                {
                    PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();
                    if (!PI.gameEnded && !PI.fold)
                    {
                        roundPlayers++;
                        if (PI.actualBid == highestBid && PI.movesThisRound > 0 || PI.allIn || PI.lastPlayer)
                        {
                            readyPlayers++;
                        }
                    }
                }
            }

            if (roundPlayers == readyPlayers)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void NextRound()
        {
            round++;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null)
                {
                    PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();

                    if (!PI.gameEnded)
                    {
                        moneyToWin += PI.actualBid;
                        PI.actualBidFloat = 1;
                        PI.actualBid = 0;
                        PI.highestBid = 0;
                        PI.movesThisRound = 0;
                        PI.SpawnChipsObjects();
                        PI.turn = false;
                    }
                }
            }            

            if (round == 1)
            {
                AddCardsOnTable(3);
                players[actualSmallBlind].GetComponent<PlayerInteractions>().turn = true;
            }
            else if(round == 2 || round == 3)
            {
                AddCardsOnTable(1);
                players[actualSmallBlind].GetComponent<PlayerInteractions>().turn = true;
            }
            else if(round == 4)
            {
                playerTurn = false;
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].GetComponent<PlayerInteractions>().cardsDealed = false;
                }
                EndDeal();
            }

            highestBid = 0;
        }

        void AddCardsOnTable(int howManyCards)
        {
            for(int i = 0; i < howManyCards; i++)
            {
                cardsOnTable.Add(cards[actualCard]);
                int j = cardsOnTable.Count - 1;
                GameObject cardOnTable = Instantiate(cardsOnTable[j], cardsSpawners[j].transform.position, cardsSpawners[j].transform.rotation);
                NetworkServer.Spawn(cardOnTable);
                cardOnTable.GetComponent<Card>().toFollow = cardsSpawners[j].transform;
                cardsOnTableObjects.Add(cardOnTable);
                actualCard++;
            }
        }

        void EndDeal()
        {
            List<int> winningPlayers = new List<int>();
        
            for (int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();

                PI.turn = false;
                PI.ready = false;

                if (PI.fold || PI.gameEnded)
                {                    
                    continue;
                }

                if(winningPlayers.Count == 0)
                {
                    winningPlayers.Add(i);
                }
                
                PlayerInteractions PIwinning = players[winningPlayers[0]].GetComponent<PlayerInteractions>();
                CheckPokerHand(PI);

                if (winningPlayers[0] != i)
                {
                    CheckBetterHand(winningPlayers[0], i, PI, PIwinning, winningPlayers);
                }
            }

            moneyToWin /= winningPlayers.Count;

            for(int i = 0; i < players.Count; i++)
            {
                for(int j = 0; j < winningPlayers.Count; j++)
                {
                    if(winningPlayers[j] == i)
                    {
                        players[i].GetComponent<PlayerInteractions>().actualMoney += moneyToWin;
                    }
                }
            }

            moneyToWin = 0;


            if (winningPlayers.Count > 1)
            {
                StartCoroutine(WaitForNextDeal("Draw"));
            }
            else
            {
                PlayerInfo Pinfo = players[winningPlayers[0]].GetComponent<PlayerInfo>();
                
                StartCoroutine(WaitForNextDeal(Pinfo.playerName + " wins!"));
            }
        }

        void CheckPokerHand(PlayerInteractions _PI)
        {
            Card[] cardsToCheck = new Card[7];
            int cardsSum = 0;
            for (int i = 0; i < cardsOnTableObjects.Count; i++)
            {
                cardsToCheck[i] = cardsOnTableObjects[i].GetComponent<Card>();
                cardsSum++;
            }
            for (int i = 0; i < _PI.cardsInHand.Count; cardsSum++, i++)
            {
                cardsToCheck[cardsSum] = _PI.cardsInHand[i].GetComponent<Card>();
            }

            List<int> usedIndex = new List<int>();
            usedIndex.Clear();

            switch (1)
            {
                case 1:
                    if (RoyalFlush(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 10;
                        Debug.Log("Royal Flush");
                        break;
                    }
                    else
                    {
                        goto case 2;
                    }
                case 2:
                    if (StraightFlush(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 9;
                        Debug.Log("Straight Flush");
                        break;
                    }
                    else
                    {
                        goto case 3;
                    }
                case 3:
                    if (FourOfKind(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 8;
                        Debug.Log("Four of a Kind");
                        break;
                    }
                    else
                    {
                        goto case 4;
                    }
                case 4:
                    if (FullHouse(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 7;
                        Debug.Log("Full House");
                        break;
                    }
                    else
                    {
                        goto case 5;
                    }
                case 5:
                    if (Flush(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 6;
                        Debug.Log("Flush");
                        break;
                    }
                    else
                    {
                        goto case 6;
                    }
                case 6:
                    if (Straight(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 5;
                        Debug.Log("Straight");
                        break;
                    }
                    else
                    {
                        goto case 7;
                    }
                case 7:
                    if (ThreeOfKind(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 4;
                        Debug.Log("Three of a Kind");
                        break;
                    }
                    else
                    {
                        goto case 8;
                    }
                case 8:
                    if (TwoPairs(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 3;
                        Debug.Log("Two Pairs");
                        break;
                    }
                    else
                    {
                        goto case 9;
                    }
                case 9:
                    if (OnePair(cardsToCheck, usedIndex))
                    {
                        _PI.pokerHandNumber = 2;
                        Debug.Log("One Pair");
                        break;
                    }
                    else
                    {
                        goto case 10;
                    }
                case 10:
                    _PI.pokerHandNumber = 1;
                    _PI.biggestCard = BiggestCard(cardsToCheck, usedIndex);
                    
                    Debug.Log("Biggest Card");
                    break;
            }

            List<int> notUsedIndex = new List<int>();
            for (int i = 0; i < cardsToCheck.Length; i++)
            {
                for (int j = 0; j < usedIndex.Count; j++)
                {
                    if (i == usedIndex[j]) // check used index
                    {
                        goto EndLoop;
                    }
                }

                notUsedIndex.Add(i);
            EndLoop: continue;
            }

            int restCards = 5 - usedIndex.Count;
            List<int> biggestPokerHandCards = new List<int>();
            List<int> biggestCards = new List<int>();

            foreach(int index in usedIndex)
            {
                biggestPokerHandCards.Add(cardsToCheck[index].number);
            }

            biggestPokerHandCards.Sort();
            biggestPokerHandCards.Reverse();

            while (restCards <= 5)
            {
                biggestCards.Add(BiggestCard(cardsToCheck, usedIndex));
                restCards++;
            }

            _PI.pokerHandsCards = biggestPokerHandCards;
            _PI.biggestCards = biggestCards;
        }

        void CheckBetterHand(int lastWinner, int thisPlayer, PlayerInteractions PI, PlayerInteractions PIwinning, List<int> listOfWinners)
        {
            if (PI.pokerHandNumber > PIwinning.pokerHandNumber)
            {
                listOfWinners.Clear();
                listOfWinners.Add(thisPlayer);
                return;
            }
            else if (PI.pokerHandNumber == PIwinning.pokerHandNumber)
            {
                for(int i = 0; i < PI.pokerHandsCards.Count; i++)
                {
                    if(PI.pokerHandsCards[i] > PIwinning.pokerHandsCards[i])
                    {
                        listOfWinners.Clear();
                        listOfWinners.Add(thisPlayer);
                        return;
                    }
                    else if(PI.pokerHandsCards[i] < PIwinning.pokerHandsCards[i])
                    {
                        return;
                    }
                }

                for (int i = 0; i < PI.biggestCards.Count; i++)
                {
                    if (PI.biggestCards[i] > PIwinning.biggestCards[i])
                    {
                        listOfWinners.Clear();
                        listOfWinners.Add(thisPlayer);
                        return;
                    }
                    else if (PI.biggestCards[i] < PIwinning.biggestCards[i])
                    {
                        return;
                    }
                }
            }
            else if(PI.pokerHandNumber < PIwinning.pokerHandNumber)
            {
                return;
            }

            listOfWinners.Add(thisPlayer);
            return;
        }

        bool RoyalFlush(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i].number == 10)
                {
                    usedIndex.Add(i);

                    if (CheckNextCard(_cards, 11, 1, _cards[i].color, 4, usedIndex))
                    {
                        return true;
                    }
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool StraightFlush(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number + 1, 1, _cards[i].color, 4, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool FourOfKind(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number, 0, 0, 3, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool FullHouse(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number, 0, 0, 2, usedIndex))
                {
                    for (int j = 0; j < _cards.Length; j++)
                    {
                        List<int> secondIndex = new List<int>(usedIndex);

                        secondIndex.Add(j);
                        if (CheckNextCard(_cards, _cards[j].number, 0, 0, 1, secondIndex))
                        {
                            usedIndex.AddRange(secondIndex);
                            return true;
                        }
                        secondIndex.Clear();
                    }
                }

                usedIndex.Clear();
            }
            return false;
        }

        bool Flush(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, 0, 0, _cards[i].color, 4, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool Straight(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number + 1, 1, 0, 4, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool ThreeOfKind(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number, 0, 0, 2, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        bool TwoPairs(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number, 0, 0, 1, usedIndex))
                {
                    for (int j = 0; j < _cards.Length; j++)
                    {
                        List<int> secondIndex = new List<int>(usedIndex);

                        secondIndex.Add(j);
                        if (CheckNextCard(_cards, _cards[j].number, 0, 0, 1, secondIndex))
                        {
                            usedIndex.AddRange(secondIndex);
                            return true;
                        }
                        secondIndex.Clear();
                    }
                }

                usedIndex.Clear();
            }
            return false;
        }

        bool OnePair(Card[] _cards, List<int> usedIndex)
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                usedIndex.Add(i);

                if (CheckNextCard(_cards, _cards[i].number, 0, 0, 1, usedIndex))
                {
                    return true;
                }

                usedIndex.Clear();
            }

            return false;
        }

        int BiggestCard(Card[] _cards, List<int> usedIndex)
        {
            int biggestCard = 0;
            int index = 0;

            for (int i = 0; i < _cards.Length; i++)
            {
                for (int j = 0; j < usedIndex.Count; j++)
                {
                    if (i == usedIndex[j]) // check used index
                    {
                        goto EndLoop;
                    }
                }

                if (biggestCard < _cards[i].number)
                {
                    biggestCard = _cards[i].number;
                    index = i;
                }

            EndLoop: continue;
            }

            usedIndex.Add(index);
            return biggestCard;
        }

        bool CheckNextCard(Card[] _cards, int nextCardNumber, int nextCardNumberModificator, int nextCardColor, int cardsLeft, List<int> usedIndex)
        {
            if (cardsLeft == 0)
            {
                return true;
            }

            for (int i = 0; i < _cards.Length; i++)
            {
                for (int j = 0; j < usedIndex.Count; j++)
                {
                    if (i == usedIndex[j]) // check used index
                    {
                        goto EndLoop;
                    }
                }

                if (_cards[i].number == nextCardNumber || nextCardNumber == 0) // check number
                {
                    if (_cards[i].color == nextCardColor || nextCardColor == 0) // check color
                    {
                        usedIndex.Add(i);

                        if (CheckNextCard(_cards, nextCardNumber + nextCardNumberModificator, nextCardNumberModificator, nextCardColor, cardsLeft - 1, usedIndex))
                        {
                            return true;
                        }
                    }
                }

            EndLoop: continue;
            }

            return false;
        }

        IEnumerator WaitForNextDeal(string winner)
        {


            for (int i = 0; i < players.Count; i++)
            {
                PlayerInteractions PI = players[i].GetComponent<PlayerInteractions>();
                PI.winnerAnnouncment = winner;
            }
            yield return new WaitForSeconds(6);
            deal = true;
        }

    }
}