using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Klondike
{
    public class CardPile : MonoBehaviour
    {
        [SerializeField] protected PileType _type = PileType.NotSet;
        [SerializeField] protected Card[] _pile;
        [SerializeField, Range(0,52)] protected int _numberOfCards;

        public int NumberOfCards { get { return _numberOfCards; } }
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
                moveCardGroup: moveCardGroup
            );

            _numberOfCards++;
        }
        public virtual void DealTopCard(CardPile pile)
        {
            pile.ReceiveCard( _pile[ _numberOfCards-1 ]);
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
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
    }
}
