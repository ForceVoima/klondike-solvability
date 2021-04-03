using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public enum Suit
    {
        Club = 0,
        Spade = 1,
        Diamond = 2,
        Heart = 3,
        NotSet = 4
    }

    public enum PileType
    {
        NotSet,
        Stock,
        WasteHeap,
        ClosedPile,
        BuildPile,
        FoundationPile
    }
}
