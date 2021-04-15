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
        public bool KingOrAce { get { return ( _rank == 1 || _rank == 13); } }

        [SerializeField] private bool _red;
        public bool Red { get { return _red; } }

        [SerializeField] private CardStatus _status = CardStatus.Stock;
        public bool SuitSolvable { get { return (_status == CardStatus.Open || _status == CardStatus.Stock); } }
        private Effect _currentEffect = Effect.Normal;
        private CardPile _parent;
        public CardPile Parent { get { return _parent; } }

        private Track _sequence;
        public Track Track { get { return _sequence; } }

        private Card _parallel, _solverA, _solverB, _suitBlocker;
        private Card _suitUp, _suitDown;

        public Card cardBelow, cardAbove;

        [SerializeField] private Card[] _blockedCards;

        [SerializeField] private bool _suitBlocked = false;
        [SerializeField] private bool _solverALocked = false;
        [SerializeField] private bool _solverBLocked = false;
        private int _solversBlocked = 0;
        [SerializeField] private Restrictions _restrictions;

        [Header("Unity specific")]
        [SerializeField] private Renderer _cardRenderer;
        [SerializeField] private Renderer _outlineRenderer;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private Rigidbody _rigidBody;

        public void Init()
        {
            if (_rank > 1 && _rank < 13)
            {
                if (_red)
                {
                    _solverA = Stock.Instance.RequestCard(Suit.Club, _rank + 1);
                    _solverB = Stock.Instance.RequestCard(Suit.Spade, _rank + 1);
                }
                else
                {
                    _solverA = Stock.Instance.RequestCard(Suit.Heart, _rank + 1);
                    _solverB = Stock.Instance.RequestCard(Suit.Diamond, _rank + 1);
                }
            }

            if ( ( _red && _rank % 2 == 0 ) || ( !_red && _rank % 2 == 1 ) )
                _sequence = Track.BlackOddRedEven;
            else
                _sequence = Track.RedOddBlackEven;

            if ( _rank < 13 )
                _suitUp = Stock.Instance.RequestCard( _suit, _rank+1 );
            if ( _rank > 1 )
                _suitDown = Stock.Instance.RequestCard( _suit, _rank-1 );

            _status = CardStatus.Stock;
            _restrictions = new Restrictions();

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

        public void MoveTo(Vector3 position,
                           Quaternion rotation,
                           bool instant,
                           PileType pile,
                           CardPile parent,
                           bool moveCardGroup = false)
        {
            if (instant)
            {
                transform.position = position;
                transform.rotation = rotation;
            }

            _parent = parent;

            if ( !moveCardGroup )
                StatusCheck( pile );
        }

        public void ClosedCards(Card[] cards, int pileNumber, int numberOfCards)
        {
            _blockedCards = new Card[numberOfCards];

            _restrictions.pile = pileNumber;
            _restrictions.position = pileNumber - numberOfCards;

            for (int i = 0; i < numberOfCards; i++)
            {
                _blockedCards[i] = cards[i];

                if ( !KingOrAce &&
                     cards[i].SameAs( _solverA ) )
                {
                    _solverALocked = true;
                    _solversBlocked++;
                }

                if ( !KingOrAce && 
                     cards[i].SameAs( _solverB ) )
                {
                    _solverBLocked = true;
                    _solversBlocked++;
                }

                if ( cards[i].Rank < _rank && 
                     cards[i].Suit == _suit )
                {
                    _suitBlocker = cards[i];
                    _suitBlocked = true;
                }
            }
            
            if (numberOfCards > 0)
            {
                if ( _solversBlocked == 1 )
                {
                    _parallel.Highlight(Effect.LowPriority);
                    Highlight( Effect.SolverBlock );
                }
                else if ( _solverALocked && _solverBLocked && !_suitBlocked )
                {
                    Highlight( Effect.MustSuitSolve );
                    _solverA.Highlight( Effect.MustSuitSolve );
                    _solverB.Highlight( Effect.MustSuitSolve );
                }

                Statistics.Instance.Report(this, _suitBlocked, _solversBlocked);
                cardBelow = _blockedCards[ _blockedCards.Length - 1 ];
                cardBelow.cardAbove = this;

                if ( _solverALocked && _solverBLocked && _suitBlocked )
                {
                    SetColor( Settings.Instance.block );
                    _suitBlocker.SetColor( Settings.Instance.block );
                    _solverA.SetColor( Settings.Instance.block );
                    _solverB.SetColor( Settings.Instance.block );
                }
            }
        }

        public void Reset()
        {
            _suitBlocked = false;
            _solverALocked = false;
            _solverBLocked = false;
            cardAbove = null;
            cardBelow = null;
            _solversBlocked = 0;
            _blockedCards = null;
            Highlight( Effect.Normal );
            SetColor( Settings.Instance.open );
            _boxCollider.enabled = false;
            _rigidBody.isKinematic = true;
            _status = CardStatus.Stock;
            _restrictions = new Restrictions();
        }

        public void Highlight(Effect code)
        {
            _outlineRenderer.material = Settings.Instance.GetHighlight( code );
            _currentEffect = code;
        }

        public void SetColor( Color color )
        {
            _cardRenderer.material.SetColor( "_Color", color );
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
            {
                Highlight( Effect.Normal );
                _status = CardStatus.Foundation;
                return;
            }

            else if ( pile == PileType.ClosedPile )
            {
                _status = CardStatus.Closed;
                SetColor( Settings.Instance.closed );
                return;
            }

            else if ( pile == PileType.BuildPile)
            {
                // Open card from closed state:
                if ( _status != CardStatus.Open && _status != CardStatus.Stock )
                {
                    if ( _status == CardStatus.Closed )
                        GameMaster.Instance.CardOpened();

                    _status = CardStatus.Open;
                    AIMaster.Instance.UpdateSolvable( _suit, _rank );
                    SetColor( Settings.Instance.open );
                    return;
                }
            }

            else if ( pile == PileType.Stock || pile == PileType.WasteHeap )
            {
                _status = CardStatus.Stock;
                SetColor( Settings.Instance.open );
            }
        }

        public void Blocked(Card byCard)
        {
            _status = CardStatus.Blocked;
            cardAbove = byCard;
            cardAbove.cardBelow = this;
            RecursiveSolvable( false );
        }

        public void UnBlocked()
        {
            _status = CardStatus.Open;
            cardAbove = null;
            AIMaster.Instance.UpdateSolvable(_suit, _rank);
        }

        public void RecursiveSolvable(bool solvable)
        {
            if ( solvable && ( _status == CardStatus.Open || _status == CardStatus.Stock ) )
            {
                if ( _currentEffect != Effect.Solvable )
                    Highlight( Effect.Solvable );

                if ( _rank != 13 )
                    _suitUp.RecursiveSolvable( true );
            }

            else
            {
                if ( _currentEffect == Effect.Solvable )
                    Highlight( Effect.Normal );

                if ( _rank != 13 )
                    _suitUp.RecursiveSolvable( false );
            }
        }

        public bool SameAs(Card card)
        {
            return ( _rank == card.Rank && _suit == card.Suit );
        }

        public void SolvabilityHeuristicsOrigin( bool allCards = false )
        {
            if ( KingOrAce )
            {
                if ( _blockedCards.Length > 0 || allCards )
                    Statistics.Instance.AdvancedReport( _suitBlocked, false, false );

                return;
            }

            bool routeSuit = !_suitBlocked;
            bool routeA = !_solverALocked;
            bool routeB = !_solverBLocked;

            if ( !_solverALocked )
                routeA = _solverA.Available( _restrictions );

            if ( !_solverBLocked )
                routeB = _solverB.Available( _restrictions );

            if ( !routeSuit && !routeA && !routeB )
            {
                SetColor(Settings.Instance.block);
                Highlight( Effect.Normal );
            }
            else if ( !routeA && !routeB )
                Highlight( Effect.MustSuitSolve );

            if ( _blockedCards.Length > 0 || allCards )
                Statistics.Instance.AdvancedReport( !routeSuit, !routeA, !routeB );
        }

        public bool Available(Restrictions restrictions)
        {
            if ( _status == CardStatus.Closed || _status == CardStatus.Open )
            {
                if ( this._restrictions.pile == restrictions.pile &&
                     this._restrictions.position > restrictions.position )
                {
                    return false;
                }

                return true;
            }
            else if ( _status == CardStatus.Stock )
            {
                if ( _rank == 13)
                    return true;

                bool a = _solverA.Available( restrictions );
                bool b = _solverB.Available( restrictions );

                return (a || b);
            }
            return false;
        }

        public bool AboveCardPossible(Restrictions limitA, Restrictions limitB)
        {
            if ( KingOrAce )
            {
                if ( _status == CardStatus.Closed )
                    return cardAbove.AboveCardPossible( limitA, limitB );
                else
                    return true;
            }

            bool routeSuit = !_suitBlocked;
            bool routeA = !_solverALocked;
            bool routeB = !_solverBLocked;

            if ( !_solverALocked )
                routeA = _solverA.AvailableAbove( limitA, limitB );

            if ( !_solverBLocked )
                routeB = _solverB.AvailableAbove( limitA, limitB );

            if ( !routeSuit && !routeA && !routeB )
                return false;
            else
            {
                if ( _status == CardStatus.Open )
                    return true;
                else
                    return cardAbove.AboveCardPossible( limitA, limitB );
            }
        }

        public bool AvailableAbove(Restrictions limitA, Restrictions limitB)
        {
            if ( _status == CardStatus.Closed || _status == CardStatus.Open )
            {
                if ( this._restrictions.pile == limitA.pile &&
                     this._restrictions.position > limitA.position )
                {
                    return false;
                }
                if ( this._restrictions.pile == limitB.pile &&
                     this._restrictions.position > limitB.position )
                {
                    return false;
                }

                return true;
            }
            else if ( _status == CardStatus.Stock )
            {
                if ( _rank == 13)
                    return true;

                bool a = _solverA.AvailableAbove( limitA, limitB );
                bool b = _solverB.AvailableAbove( limitA, limitB );

                return (a || b);
            }
            return false;
        }
    }
}
