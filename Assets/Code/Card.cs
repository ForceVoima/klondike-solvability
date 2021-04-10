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

        [SerializeField] private CardStatus _status = CardStatus.Open;
        private Effect _currentEffect = Effect.Normal;

        private Track _sequence;
        public Track Track { get { return _sequence; } }

        private Card _parallel, _solver1, _solver2;

        public Card Parallel { get { return _parallel; } }

        public Card below, above;

        [SerializeField] private Card[] _blockedCards;

        [SerializeField] private bool _suitBlocked = false;
        [SerializeField] private int _solversBlocked = 0;

        [Header("Unity specific")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private Rigidbody _rigidBody;

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

            if ( ( _red && _rank % 2 == 0 ) || ( !_red && _rank % 2 == 1 ) )
                _sequence = Track.BlackOddRedEven;
            else
                _sequence = Track.RedOddBlackEven;

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

        public void MoveTo(Vector3 position, Quaternion rotation, bool instant, PileType pile, bool moveCardGroup = false)
        {
            if (instant)
            {
                transform.position = position;
                transform.rotation = rotation;
            }

            if ( !moveCardGroup )
                StatusCheck( pile );
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
                    _parallel.Highlight(Effect.LowPriority);
                    _solversBlocked++;
                    Highlight( Effect.SolverBlock );
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
            _blockedCards = null;
            Highlight( Effect.Normal );
            _boxCollider.enabled = false;
            _rigidBody.isKinematic = true;
        }

        public void Highlight(Effect code)
        {
            _renderer.material = Settings.Instance.GetHighlight( code );
            _currentEffect = code;
        }

        public void EnablePhysics()
        {
            _boxCollider.enabled = true;
            _rigidBody.isKinematic = false;
        }

        public void ThrowCard(Vector3 impulse, ForceMode mode, Vector3 torque)
        {
            _rigidBody.AddForce(impulse, mode);
            _rigidBody.AddTorque(torque, mode);
        }

        public override string ToString()
        {
            string suit = "";

            if ( _suit == Suit.Club )
                suit = "C";
            if ( _suit == Suit.Spade )
                suit = "S";
            if ( _suit == Suit.Heart )
                suit = "H";
            if ( _suit == Suit.Diamond )
                suit = "D";

            return string.Concat( suit, Rank.ToString() );
        }

        private void StatusCheck(PileType pile)
        {
            if ( ( _currentEffect == Effect.LowPriority ||
                   _currentEffect == Effect.SolverBlock ) &&
                   _status != CardStatus.Closed &&
                   pile != PileType.ClosedPile )
            {
                _solversBlocked = 0;
                _suitBlocked = false;
                _blockedCards = null;
                Highlight( Effect.Normal );
            }

            if ( pile == PileType.FoundationPile )
                Highlight( Effect.Normal );

            if ( pile == PileType.ClosedPile )
            {
                _status = CardStatus.Closed;
                AIMaster.Instance.ClosedCard( _suit, _rank );
            }

            else if ( _status != CardStatus.Open )
            {
                _status = CardStatus.Open;
                AIMaster.Instance.OpenedCard( _suit, _rank );
            }
        }

        public void Blocked(Card byCard)
        {
            _status = CardStatus.Blocked;
            above = byCard;
            AIMaster.Instance.ClosedCard(_suit, _rank);

            if ( _currentEffect == Effect.Solvable )
            {
                _currentEffect = Effect.Normal;
                Highlight( Effect.Normal );
            }
        }

        public void UnBlocked()
        {
            _status = CardStatus.Open;
            above = null;
            AIMaster.Instance.OpenedCard(_suit, _rank);
        }
    }
}
