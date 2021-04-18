using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    [System.Serializable]
    public class Restrictions
    {
        public string name;
        public int pile;
        public int position;
        public int[] suits;

        public Restrictions(string name)
        {
            this.name = name;
            suits = new int[4];
            pile = 0;
            position = 0;
        }
    }
}
