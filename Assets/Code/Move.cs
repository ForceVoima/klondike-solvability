using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    [System.Serializable]
    public class Move
    {
        public int moveNumber;
        public MoveType type;
        public Card sourceCard;
        public CardPile sourcePile;
        public int sourceIndex;
        public CardPile targetPile;
        public int targetIndex;
        public bool confirmed;

        public Move(int moveNumber, Card card, CardPile source, CardPile target)
        {
            this.moveNumber = moveNumber;
            this.sourceCard = card;
            this.sourcePile = source;
            this.targetPile = target;

            if ( source.Type == PileType.Stock || source.Type == PileType.WasteHeap )
                type = MoveType.Stock;

            else if ( source.Type == PileType.BuildPile )
            {
                if ( target.Type == PileType.FoundationPile )
                    this.type = MoveType.TableToFoundation;
                else if ( target.Type == PileType.BuildPile )
                    this.type = MoveType.TableToTable;
            }

            else if ( source.Type == PileType.ClosedPile )
                type = MoveType.ClosedToOpen;
        }
    }
}
