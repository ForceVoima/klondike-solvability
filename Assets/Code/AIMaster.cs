using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class AIMaster : Master
    {
        [SerializeField] private int[] _foundationStatus;

        [SerializeField] private Card[] _foundationCards;

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

            _foundationStatus = new int[4];
            _foundationCards = new Card[4];

            for (int i = 0; i < 4; i++)
            {
                _foundationCards[i] = _stock.RequestCard( (Suit)i, 1 );
            }
        }

        public void Reset()
        {
            for (int i = 0; i < 4; i++)
            {
                _foundationStatus[i] = 0;
                _foundationCards[i] = _stock.RequestCard( (Suit)i, 1 );
            }
        }

        public void UpdateSolvable(Suit suit, int rank)
        {
            HighlightSolvableInSuit( (int)suit );
        }

        public void CardFounded(Suit suit, int rank)
        {
            _foundationStatus[(int)suit] = rank;

            if ( rank < 13 )
                _foundationCards[ (int)suit ] = _stock.RequestCard( suit, rank+1 );

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
            _foundationCards[ suit ].RecursiveSolvable( true );
        }

        public void SolveAll()
        {
            int suit = 0;
            int solved = 1;
            
            while (solved > 0)
            {
                suit = 0;
                solved = 0;

                while (suit < 4)
                {
                    solved += SolveSuit( suit );
                    suit++;
                }
            }
        }

        private int SolveSuit(int suit)
        {
            if ( !_foundationCards[ suit ].SuitSolvable )
                return 0;

            Card card = _foundationCards[ suit ];

            return SolveCard( card );
        }

        private int SolveCard(Card card)
        {
            CardPile pile = card.Parent;
            PileType type = pile.Type;

            int i = 0;

            while (i < 4)
            {
                if ( _foundations[ i ].AcceptsCard( card ))
                    break;
                else
                    i++;
            }

            if ( type == PileType.Stock || type == PileType.WasteHeap )
                pile.DealCardTo( _foundations[i], card.Suit, card.Rank );
            else
                pile.DealTopCard( _foundations[i] );

            return 1;
        }

        public void SolveUntil(Card card)
        {
            int solved = 1;
            int rank = card.Rank;
            
            while ( solved > 0 )
            {
                solved = SolveSuit( (int)card.Suit );

                if ( _foundationStatus[ (int)card.Suit ] == rank )
                    break;
            }
        }
    }
}
