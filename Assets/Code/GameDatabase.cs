using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    [System.Serializable]
    public class GameDatabase
    {
        public List<KlondikeGame> allGames;

        public GameDatabase()
        {
            allGames = new List<KlondikeGame>();
        }
    }
}
