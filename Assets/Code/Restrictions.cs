using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    [System.Serializable]
    public class Restrictions
    {
        public int pile;
        public int position;
        public int[] suits;

        public Restrictions()
        {
            suits = new int[4];
        }
    }
}
