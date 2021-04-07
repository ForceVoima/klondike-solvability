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

        [SerializeField] private Card _parallel, _solver1, _solver2;

        public Card Parallel { get { return _parallel; } }

        [SerializeField] private Card[] _blockedCards;

        [SerializeField] private bool _suitBlocked = false;
        [SerializeField] private int _solversBlocked = 0;
        [SerializeField] private Renderer _renderer;

        public void Init()
        {
            if (_rank > 1 && _rank < 13)
            {
                if (_red)
                {
                    _solver1 = Stock.Instance.RequestCard(Suit.Club, _rank + 1);
                    _solver2 = Stock.Instance.RequestCard(Suit.Spade, _rank + 1);
                }
                else
                {
                    _solver1 = Stock.Instance.RequestCard(Suit.Heart, _rank + 1);
                    _solver2 = Stock.Instance.RequestCard(Suit.Diamond, _rank + 1);
                }
            }

            switch (_suit)
            {
                case Suit.Club:
                    _parallel = Stock.Instance.RequestCard(Suit.Spade, _rank);
                    return;
                case Suit.Spade:
                    _parallel = Stock.Instance.RequestCard(Suit.Club, _rank);
                    return;
                case Suit.Diamond:
                    _parallel = Stock.Instance.RequestCard(Suit.Heart, _rank);
                    return;
                case Suit.Heart:
                    _parallel = Stock.Instance.RequestCard(Suit.Diamond, _rank);
                    return;
            }
        }

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

                if ( _rank > 1 &&
                     _cards[i].Rank == (_rank + 1) &&
                     _cards[i].Red == !_red )
                {
                    _cards[i].Highlight(Effect.SolverBlock);
                    _cards[i].Parallel.Highlight(Effect.SolverBlock);
                    _parallel.Highlight(Effect.LowPriority);
                    _solversBlocked++;
                    Highlight( Effect.SolverBlock );
                }

                if ( _cards[i].Rank < _rank && 
                     _cards[i].Suit == _suit )
                {
                    _suitBlocked = true;
                    Highlight( Effect.SuitBlock );
                }
            }
            
            if (numberOfCards > 0)
                Statistics.Instance.Report(this, _suitBlocked, _solversBlocked);
        }

        public void Reset()
        {
            _suitBlocked = false;
            _solversBlocked = 0;
            _blockedCards = null;
            Highlight( Effect.Normal );
        }

        public void Highlight(Effect code)
        {
            if ( _solversBlocked > 0 )
                _renderer.material = Settings.Instance.GetHighlight( Effect.SolverBlock );
            else if ( _suitBlocked )
                _renderer.material = Settings.Instance.GetHighlight( Effect.SuitBlock );
            else
                _renderer.material = Settings.Instance.GetHighlight( code );
        }
    }
}
