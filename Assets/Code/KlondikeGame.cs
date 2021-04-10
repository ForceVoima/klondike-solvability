using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    [System.Serializable]
    public class KlondikeGame
    {
        public int gameID;
        public string[] piles = new string[8];
        public string stats;
        public int bestOpens = 0;
        public int bestFoundations = 0;

        public KlondikeGame(int gameID)
        {
            this.gameID = gameID;
        }
    }
}
