using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Klondike
{
    public class CardPile : MonoBehaviour
    {
        [SerializeField] protected PileType _type = PileType.NotSet;
        public PileType Type { get { return _type; } }
        [SerializeField] protected Card[] _pile;
        [SerializeField, Range(0,52)] protected int _numberOfCards;
        public int NumberOfCards { get { return _numberOfCards; } }
        public bool hasCards { get { return (_numberOfCards > 0); } }
        protected Vector3[] _positions;
        protected Quaternion _rotation;
        public Card TopCard {
        get
        {
            if ( _numberOfCards > 0 )
                return _pile[ _numberOfCards-1 ];
            else
                return null;
            }
        }

        public virtual void ReceiveCard(Card card, bool moveCardGroup = false)
        {
            card.transform.SetParent(transform);
            _pile[_numberOfCards] = card;

            card.MoveTo(
                position: _positions[_numberOfCards],
                rotation: _rotation,
                instant: true,
                pile: _type,
                parent: this,
                moveCardGroup: moveCardGroup
            );

            _numberOfCards++;
        }

        public virtual void ReceiveToIndex(Card card, int index)
        {
        }
        
        public virtual void DealTopCard(CardPile pile)
        {
            TopCard.cardBelow = null;

            pile.ReceiveCard( _pile[ _numberOfCards-1 ]);
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
        }

        public virtual void TopCardTaken()
        {
        }

        public virtual void ReturnTopCard(CardPile pile)
        {
            pile.ReceiveCard( _pile[ _numberOfCards-1 ]);
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
        }

        public virtual void ReturnCard(Card card, CardPile pile, int sourceIndex)
        {
        }

        public virtual void ReturnCards(Card[] cards, CardPile pile)
        {
        }

        public virtual void ResetCards(Stock stock)
        {
            for (int i = 0; i < _numberOfCards; i++)
            {
                stock.ReceiveCard( _pile[i] );
                _pile[i] = null;
            }

            _numberOfCards = 0;
        }

        public override string ToString()
        {
            int i = 1;
            StringBuilder sb = new StringBuilder();

            if ( _numberOfCards > 0 )
            {
                sb.Append( _pile[0].ToString() );
            }

            while ( i < _numberOfCards )
            {
                sb.Append("-").Append( _pile[i].ToString() );
                i++;
            }

            return sb.ToString();
        }

        public bool Contains(Card card)
        {
            int i = IndexOf( card.Suit, card.Rank );
            return i != -1;
        }

        public int IndexOf(Card card)
        {
            return IndexOf( card.Suit, card.Rank );
        }

        protected int IndexOf(Suit suit, int rank)
        {
            int i = 0;

            while (i < _pile.Length)
            {
                if ( _pile[i] != null &&
                     _pile[i].Suit == suit && _pile[i].Rank == rank )
                     return i;
                
                i++;
            }

            return -1;
        }

        public virtual void DealCardTo(CardPile pile, Suit suit, int rank)
        {
        }

        public void PileAnalysis(bool allCards = false)
        {
            for (int i = 0; i < _numberOfCards; i++)
            {
                _pile[i].RecordAdvancedStatistics( allCards );
            }
        }

        public virtual void PopulateHoverList(List<Card> cardList, Card card = null)
        {
        }
    }
}
