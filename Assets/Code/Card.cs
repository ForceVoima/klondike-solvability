using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{

    public class Card : MonoBehaviour
    {
        [SerializeField] private Suit _suit = Suit.NotSet;
        public Suit Suit
        {
            get { return _suit; }
            set { _suit = value; }
        }

        [SerializeField, Range(0,13)] int _rank = 0;
        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        [SerializeField] private bool _red;
        public bool Red { get { return _red; } }

        [SerializeField] private Card[] _blockedCards;

        [SerializeField] private bool _suitBlocked = false;
        [SerializeField] private int _solversBlocked = 0;

        public void MoveTo(Vector3 position, Quaternion rotation, bool instant)
        {
            if (instant)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }

        public void ClosedCards(Card[] _cards, int numberOfCards)
        {
            _blockedCards = new Card[numberOfCards];

            for (int i = 0; i < numberOfCards; i++)
            {
                _blockedCards[i] = _cards[i];

                if ( _cards[i].Rank == (_rank + 1) &&
                     _cards[i].Red == !_red )
                {
                    _solversBlocked++;
                }

                if ( _cards[i].Rank < _rank && 
                     _cards[i].Suit == _suit )
                {
                    _suitBlocked = true;
                }
            }
            
            if (numberOfCards > 0)
                Statistics.Instance.Report(this, _suitBlocked, _solversBlocked);
        }

        public void Reset()
        {
            _suitBlocked = false;
            _solversBlocked = 0;
        }
    }
}
