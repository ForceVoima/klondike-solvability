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
        public Card[] sourceCards;
        public CardPile sourcePile;
        public int sourceIndex;
        public CardPile targetPile;
        public int targetIndex;
        public bool confirmed;

        public Move()
        {
            type = MoveType.Null;
        }

        public Move(int moveNumber, Card card, CardPile source, CardPile target)
        {
            this.moveNumber = moveNumber;
            this.sourceCard = card;
            this.sourceCards = null;
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

            else if ( source.Type == PileType.BuildPile )
            {
                if ( target.Type == PileType.BuildPile )
                    type = MoveType.TableToTable;
                else if ( target.Type == PileType.FoundationPile )
                    type = MoveType.TableToFoundation;
            }

            else if ( source.Type == PileType.FoundationPile && target.Type == PileType.BuildPile )
            {
                type = MoveType.WorryBack;
            }
        }

        public Move(int moveNumber, Card[] cards, CardPile source, CardPile target)
        {
            this.moveNumber = moveNumber;
            this.sourceCard = null;
            this.sourceCards = cards;
            this.sourcePile = source;
            this.targetPile = target;
            this.type = MoveType.TableToTable;
        }
    }
}
