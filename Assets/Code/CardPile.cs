using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class CardPile : MonoBehaviour
    {
        [SerializeField] protected PileType _type = PileType.NotSet;

        public PileType Type { get { return _type; } }
        [SerializeField] protected Card[] _pile;
        [SerializeField, Range(0,52)] protected int _numberOfCards;

        public int NumberOfCards { get { return _numberOfCards; } }
        protected Vector3[] _positions;
        protected Quaternion _rotation;

        public virtual void ReceiveCard(Card card)
        {
            card.transform.SetParent(transform);

            _pile[_numberOfCards] = card;
            card.MoveTo(
                position: _positions[_numberOfCards],
                rotation: _rotation,
                instant: true
            );

            _numberOfCards++;
        }
        public void DealTopCard(CardPile pile)
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
    }
}
