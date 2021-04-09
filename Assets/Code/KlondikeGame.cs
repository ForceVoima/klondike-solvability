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

        public KlondikeGame(int gameID)
        {
            this.gameID = gameID;
        }
    }
}
