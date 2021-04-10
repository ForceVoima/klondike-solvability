using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class AIMaster : Master
    {
        public bool[] _openCards;

        [SerializeField] private int[] _foundationStatus;

        private static AIMaster _instance;
        public static AIMaster Instance
        {
            get { return _instance; }
        }

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate AI!");
                return;
            }

            _openCards = new bool[52];
            _foundationStatus = new int[4];
        }

        public void Reset()
        {
            for (int i = 0; i < _openCards.Length; i++)
            {
                _openCards[i] = true;
            }

            for (int i = 0; i < 4; i++)
            {
                _foundationStatus[i] = 0;
            }
        }

        public void ClosedCard(Suit suit, int rank)
        {
            _openCards[ (int)suit*13 + rank - 1 ] = false;
        }

        public void OpenedCard(Suit suit, int rank)
        {
            _openCards[ (int)suit*13 + rank - 1 ] = true;
                HighlightSolvableInSuit( (int)suit );
        }

        public void CardFounded(Suit suit, int rank)
        {
            _foundationStatus[(int)suit] = rank;
            HighlightSolvableInSuit( (int)suit );
        }

        public void HighlightSolvable()
        {
            int suit = 0;
            
            while (suit < 4)
            {
                HighlightSolvableInSuit( suit );
                suit++;
            }
        }

        private void HighlightSolvableInSuit(int suit)
        {
            int rank = 0;

            rank = _foundationStatus[suit];

            if ( rank == 0 && !_openCards[ suit*13 ] || rank == 13 )
            {
                suit++;
                return;
            }
            
            rank++;
            _stock.RequestCard( (Suit)suit, rank ).RecursiveSolvable( true );
        }
    }
}
