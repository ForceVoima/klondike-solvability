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
        public bool IsClosed { get { return _status == CardStatus.Closed; } }
        public bool IsBlocked { get { return _status == CardStatus.Blocked; } }

        public bool SuitSolvable { get { return (_status == CardStatus.Open || _status == CardStatus.Stock); } }
        public bool OnTable { get {
            return ( _status == CardStatus.Closed ||
                     _status == CardStatus.Open ||
                     _status == CardStatus.Blocked );
        } }
        public bool OffTable { get { return ( _status == CardStatus.Stock || _status == CardStatus.Foundation); } }
        [SerializeField] private Effect _currentEffect = Effect.Normal;
        private CardPile _parent;
        public CardPile Parent { get { return _parent; } }

        private Track _sequence;
        public Track Track { get { return _sequence; } }

        private Card _parallel, _solverA, _solverB, _suitBlocker;
        private Card _solveeA, _solveeB;
        private Card _suitUp, _suitDown;

        public Card cardBelow, cardAbove;

        [SerializeField] private Card[] _blockedCards;

        [SerializeField] private bool _suitBlocked = false;
        [SerializeField] private bool _aSolverBlocked = false;
        [SerializeField] private bool _bSolverBlocked = false;
        [SerializeField] private bool _aRouteBlocked = false;
        [SerializeField] private bool _bRouteBlocked = false;
        [SerializeField] private bool _suitRouteBlocked = false;
        [SerializeField] private bool _parallelSuitRouteBlocked = false;
        [SerializeField] private bool _solved = true;
        [SerializeField] private bool _impossible = false;
        private int _solversBlocked = 0;
        [SerializeField] private Restrictions _restrictions;

        [Header("Unity specific")]
        [SerializeField] private Renderer _cardRenderer;
        [SerializeField] private Renderer _outlineRenderer;
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private Rigidbody _rigidBody;

        public bool _debug = false;
        private bool _blockedNotify = false;

        private Vector3 _original = new Vector3();
        private Vector3 _offset = new Vector3();

        private List<Card> _blockList;
        private List<Card> _solverList;

        public void Init()
        {
            if (_rank > 1 && _rank <= 13)
            {
                if (_red)
                {
                    if ( _rank < 13)
                    {
                        _solverA = Stock.Instance.RequestCard(Suit.Club, _rank + 1);
                        _solverB = Stock.Instance.RequestCard(Suit.Spade, _rank + 1);
                    }
                    if ( _rank > 2 )
                    {
                        _solveeA = Stock.Instance.RequestCard(Suit.Club, _rank - 1);
                        _solveeB = Stock.Instance.RequestCard(Suit.Spade, _rank - 1);
                    }
                }
                else
                {
                    if ( _rank < 13 )
                    {
                        _solverA = Stock.Instance.RequestCard(Suit.Heart, _rank + 1);
                        _solverB = Stock.Instance.RequestCard(Suit.Diamond, _rank + 1);
                    }
                    if ( _rank > 2 )
                    {
                        _solveeA = Stock.Instance.RequestCard(Suit.Heart, _rank - 1);
                        _solveeB = Stock.Instance.RequestCard(Suit.Diamond, _rank - 1);
                    }
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
            _restrictions = new Restrictions(name);
            _blockList = new List<Card>();
            _solverList = new List<Card>();
            _solved = true;

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

            _original = transform.position;
        }

        public void ClosedCards(Card[] cards, int pileNumber, int numberOfCards)
        {
            _blockedCards = new Card[numberOfCards];

            _restrictions.pile = pileNumber;
            _restrictions.position = pileNumber - numberOfCards;

            _solversBlocked = 0;

            for (int i = 0; i < numberOfCards; i++)
            {
                _blockedCards[i] = cards[i];

                if ( !KingOrAce &&
                     cards[i].SameAs( _solverA ) )
                {
                    _aSolverBlocked = true;
                    _solversBlocked++;
                }

                if ( !KingOrAce && 
                     cards[i].SameAs( _solverB ) )
                {
                    _bSolverBlocked = true;
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
                else if ( _aSolverBlocked && _bSolverBlocked && !_suitBlocked )
                {
                    Highlight( Effect.MustSuitSolve );
                    _solverA.Highlight( Effect.MustSuitSolve );
                    _solverB.Highlight( Effect.MustSuitSolve );
                }

                Statistics.Instance.Report(this, _suitBlocked, _solversBlocked);
                cardBelow = _blockedCards[ _blockedCards.Length - 1 ];
                cardBelow.cardAbove = this;

                // Card is impossible to solve in trivial way
                if ( _aSolverBlocked && _bSolverBlocked && _suitBlocked )
                {
                    _impossible = true;
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
            _aSolverBlocked = false;
            _bSolverBlocked = false;
            _solved = true;
            cardAbove = null;
            cardBelow = null;
            _solversBlocked = 0;
            _blockedCards = null;
            Highlight( Effect.Normal );
            SetColor( Settings.Instance.open );
            _boxCollider.enabled = false;
            _rigidBody.isKinematic = true;
            _status = CardStatus.Stock;
            _restrictions = new Restrictions(name);
            _impossible = false;
            _blockedNotify = false;
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

        #region StatusChecks
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
                if ( !_solved )
                    Solved();

                Founded();
                return;
            }

            else if ( pile == PileType.ClosedPile )
            {
                Closed();
                return;
            }

            else if ( pile == PileType.BuildPile )
            {
                if ( _status == CardStatus.Open ||
                     _status == CardStatus.Blocked )
                    Solved();

                // Open card from closed state:
                if ( _status != CardStatus.Open && _status != CardStatus.Stock )
                {
                    Opened();
                    return;
                }
                else if ( _status == CardStatus.Stock )
                    _status = CardStatus.Open;
            }

            else if ( pile == PileType.Stock || pile == PileType.WasteHeap )
            {
                _status = CardStatus.Stock;
                SetColor( Settings.Instance.open );
            }
        }

        public void Solved()
        {
            // If solved despite being close to impoosible: change color
            if ( _suitRouteBlocked && _aRouteBlocked && _bRouteBlocked )
            {
                SetColor( Settings.Instance.open );
            }

            _solved = true;

            if ( _currentEffect != Effect.Solvable || _currentEffect != Effect.MustSuitSolve )
                Highlight( Effect.Normal );
        }

        public void UnSolved()
        {
            _solved = false;
        }

        private void Founded()
        {
            Highlight( Effect.Normal );
            _status = CardStatus.Foundation;
            //Debug.Log( name + " founded!");
        }

        private void Closed()
        {
            _solved = false;
            _status = CardStatus.Closed;
            SetColor( Settings.Instance.closed );
            //Debug.Log( name + " closed!");
        }

        public void Opened()
        {
            if ( _status == CardStatus.Closed )
                GameMaster.Instance.CardOpened();

            _status = CardStatus.Open;
            AIMaster.Instance.UpdateSolvable( _suit );
            SetColor( Settings.Instance.open );
            cardAbove = null;

            //Debug.Log( name + " opened!");
        }

        public void Blocked(Card byCard)
        {
            _status = CardStatus.Blocked;
            cardAbove = byCard;
            cardAbove.cardBelow = this;
            _blockedNotify = true;

            //SetColor( Settings.Instance.yellow );
            //Debug.Log( name + " blocked!");
        }

        public void BlockedNotify()
        {
            if ( !_blockedNotify )
                return;

            //Debug.Log( name + "(" + _status + ") notifies others!");

            RecursiveSolvable( false );

            if ( _rank > 2 )
            {
                _solveeA.UpdateRoutes();
                _solveeB.UpdateRoutes();
            }

            if ( _rank < 13 )
                _suitUp.SuitUpRecursiveUpdate();
        }
        #endregion StatusChecks

        #region RecursiveMessages

        public void SuitUpRecursiveUpdate()
        {
            UpdateRoutes();

            if ( _rank < 12 )
                _suitUp.SuitUpRecursiveUpdate();
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

        #endregion RecursiveMessages

        public bool SameAs(Card card)
        {
            return ( _rank == card.Rank && _suit == card.Suit );
        }

        public void RecordAdvancedStatistics( bool allCards = false )
        {
            if ( KingOrAce )
            {
                if ( _blockedCards.Length > 0 || allCards )
                    Statistics.Instance.AdvancedReport( _suitBlocked, false, false );

                return;
            }

            UpdateRoutes();

            if ( _blockedCards.Length > 0 || allCards )
                Statistics.Instance.AdvancedReport( _suitRouteBlocked, _aRouteBlocked, _bRouteBlocked );

            if ( _suitRouteBlocked && _aRouteBlocked && _bRouteBlocked )
            {
                SetColor(Settings.Instance.block);
                Highlight( Effect.Normal );
                _impossible = true;
            }
        }

        public void UpdateRoutes(bool debugVisuals = false)
        {
            if ( debugVisuals )
            {
                string below, card, above;

                if ( cardBelow == null)
                    below = "";
                else
                    below = cardBelow.name;
                    
                card = name + "(" + _status + ")";

                if ( cardAbove == null)
                    above = "";
                else
                    above = cardAbove.name;

                VisualHelper.Instance.AboveBelowCards( below, card, above );
            }

            if ( KingOrAce )
                return;
                
            _blockList.Clear();
            _solverList.Clear();

            if ( _solved || !OnTable )
                return;

            if (_debug)
                Debug.Log( name + " UpdateRoutes called");
            
            _aRouteBlocked = _aSolverBlocked;
            _bRouteBlocked = _bSolverBlocked;
            _suitRouteBlocked = _suitBlocked;

            if ( _aRouteBlocked && !_bRouteBlocked )
            {
                _blockList.Add( _solverA );
                _bRouteBlocked = !_solverB.Available( _restrictions, _blockList, _solverList, _debug );
            }
            else if ( !_aRouteBlocked && _bRouteBlocked )
            {
                _aRouteBlocked = !_solverA.Available( _restrictions, _blockList, _solverList, _debug );
                _blockList.Add( _solverB );
            }
            else if ( _aRouteBlocked && _bRouteBlocked )
            {
                _blockList.Add( _solverA );
                _blockList.Add( _solverB );
            }
            else
            {
                if ( _solverA.OffTable && _solverB.OffTable )
                {
                    if ( _debug )
                        Debug.Log( name + " | both " + _solverA.name + " & " + _solverB.name + " off table, single A()");

                    _aRouteBlocked = !_solverA.Available( _restrictions, _blockList, _solverList, _debug );
                    _bRouteBlocked = _aRouteBlocked;    
                    _solverList.Add( _solverB );
                }
                else
                {
                    _aRouteBlocked = !_solverA.Available( _restrictions, _blockList, _solverList, _debug );
                    _bRouteBlocked = !_solverB.Available( _restrictions, _blockList, _solverList, _debug );
                }
            }

            if ( _suitBlocked )
            {
                _blockList.Add( _suitBlocker );

                if ( _suitRouteBlocked && _aRouteBlocked && _bRouteBlocked )
                {
                    if ( _debug )
                        Debug.Log( name + " I'm not solvable but let's check parallel: " + _parallel.name );

                    if ( _solverA.IsBlocked || _solverB.IsBlocked )
                        _parallelSuitRouteBlocked = !_parallel.ParallelSuitRoute( _restrictions, _blockList, _solverList, _debug );

                    else
                        _parallelSuitRouteBlocked = true;


                    if ( !_parallelSuitRouteBlocked )
                    {
                        if ( _debug )
                            Debug.Log( name + " Parallel: " + _parallel.name + " can still be suit solved!");

                        _parallel.Highlight( Effect.MustSuitSolve );
                        _solverList.Add( _parallel );
                    }
                    else
                    {
                        if ( _debug )
                            Debug.Log( name + " Parallel: " + _parallel.name + " is also blocked!");

                        _blockList.Add( _parallel );
                    }
                }
            }
            else if ( _aRouteBlocked && _bRouteBlocked )
            {
                if (_debug)
                    Debug.Log( name + " | Both solvers blocked, checking suit down!" );

                Highlight( Effect.MustSuitSolve );
                _suitRouteBlocked = !_suitDown.NextSuitAvailable( _restrictions, _blockList, _solverList, _debug);

                if ( _debug && _suitRouteBlocked )
                    Debug.Log( name + " Oh no! Suit cards are not solvable without me!");
            }
            
            if ( ( _suitRouteBlocked && _parallelSuitRouteBlocked ) && _aRouteBlocked && _bRouteBlocked )
            {
                if (_debug)
                    Debug.Log( name + " I'm totally unsolvable in any way!");

                SetColor(Settings.Instance.block);
                Highlight( Effect.Normal );
                _impossible = true;
                return;
            }
            else
            {
                NormalColor();
            }
        }

        private bool Available(Restrictions restrictions, List<Card> blocklist, List<Card> solveList, bool debugMessages = false)
        {
            if (debugMessages)
                Debug.Log( name + "(" + _status + ") Available() called" );

            if ( _status == CardStatus.Closed || _status == CardStatus.Open )
            {
                if ( !_solved &&
                     this._restrictions.pile == restrictions.pile &&
                     this._restrictions.position > restrictions.position )
                {
                    blocklist.Add( this );
                    
                    if (debugMessages)
                        Debug.Log( name + " I'm under " + restrictions.name + " and unavailable for solving! False");

                    return false;
                }

                if ( cardAbove != null )
                {
                    if (debugMessages)
                        Debug.Log( name + " I'm available not under "+ restrictions.name +", let's check aboveCard: " + cardAbove.name );

                    bool pos = cardAbove.AboveCardSolvable( restrictions, this._restrictions, debugMessages );

                    if ( pos )
                        solveList.Add( this );
                    else
                        blocklist.Add( this );

                    if (debugMessages)
                        Debug.Log( name + " aboveCard: " + cardAbove.name + " ACS returned: " + pos);
                        
                    return pos;
                }
                else
                {
                    if (debugMessages)
                        Debug.Log( name + " | A | I'm top card and available!");

                    solveList.Add( this );
                    return true;
                }
            }
            else if ( _status == CardStatus.Stock || _status == CardStatus.Foundation )
            {
                if ( _rank == 13)
                    return true;

                bool a = _solverA.Available( restrictions, blocklist, solveList, debugMessages );
                bool b = _solverB.Available( restrictions, blocklist, solveList, debugMessages );

                if ( debugMessages )
                    Debug.Log( name + "(" + _status + ") | A | " +_solverA.name + ": " + a + " | "+ _solverB.name +": " + b);

                solveList.Add( this );

                return (a || b);
            }

            else if ( _status == CardStatus.Blocked )
            {
                if (debugMessages)
                    Debug.Log( name + " | A | I'm blocked and unavailable!");

                blocklist.Add( this );
                return false;
            }

            return false;
        }

        private bool AboveCardSolvable(Restrictions limitA, Restrictions limitB, bool debugMessages = false)
        {
            if (debugMessages)
                Debug.Log( name + " | ACS | called");

            if ( _impossible )
            {
                if ( debugMessages )
                    Debug.Log( name + "I'm impossible to solve: False");

                return false;
            }

            if ( KingOrAce )
            {
                if ( _status == CardStatus.Closed )
                {
                    if (debugMessages)
                        Debug.Log( name + " | ACS | I'm King/Ace, skip to card above me " + cardAbove.name );

                    return cardAbove.AboveCardSolvable( limitA, limitB );
                }
                else
                {
                    if (debugMessages)
                        Debug.Log( name + " | ACS | I'm top of the pile and solvable! True!");

                    return true;
                }
            }

            if ( _solved )
                Debug.LogError( name + "(" + _status + ") | ACS | I'm solved and not supposed to be above anything");

            bool routeSuit = !_suitBlocked;
            bool routeA = !_aSolverBlocked;
            bool routeB = !_bSolverBlocked;

            if ( routeA && !routeB )
            {
                routeA = _solverA.NextSolverAvailable( limitA, limitB, debugMessages );
            }
            else if ( !routeA && routeB )
            {
                routeB = _solverB.NextSolverAvailable( limitA, limitB, debugMessages );
            }
            else if ( routeA && routeB )
            {
                if ( _solverA.OffTable && _solverB.OffTable )
                {
                    if ( _debug )
                        Debug.Log( name + " | ACS | both " + _solverA.name + " & " + _solverB.name + " off table, single A()");

                    routeA = _solverA.NextSolverAvailable( limitA, limitB, debugMessages );
                    routeB = routeA;
                }
                else
                {
                    routeA = _solverA.NextSolverAvailable( limitA, limitB, debugMessages );
                    routeB = _solverB.NextSolverAvailable( limitA, limitB, debugMessages );
                }
            }

            if ( debugMessages )
                Debug.Log( name + " | ACS | "+_solverA.name + ": " + routeA + " | "+ _solverB.name +": " + routeB);

            if ( !routeSuit && !routeA && !routeB )
            {
                if (debugMessages)
                    Debug.Log( name + " ACS | I'm not solvable with these limits!" );

                return false;
            }
            else
            {
                if ( _status == CardStatus.Open || _status == CardStatus.Blocked )
                {
                    if (debugMessages)
                        Debug.Log(name + " | ACS | I'm top of pile and solvable!" );

                    return true;
                }
                else
                {
                    if (debugMessages)
                        Debug.Log( name + " | ACS | I'm solvable, but lets check above card " + cardAbove.name);

                    if ( cardAbove == null)
                    {
                        Debug.LogError( name + "(" + _status + ") ACS | my abovecard is null!");
                        return true;
                    }

                    bool a = cardAbove.AboveCardSolvable( limitA, limitB, debugMessages );

                    if (debugMessages)
                        Debug.Log( name + " | ACS | abovecard " + cardAbove.name + " returned " + a);

                    return a;
                }
            }
        }

        private bool NextSolverAvailable(Restrictions limitA, Restrictions limitB, bool debugMessages = false )
        {
            if (debugMessages)
                Debug.Log( name + "(" + _status + ") | AA | called " );

            if ( _status == CardStatus.Closed || _status == CardStatus.Open )
            {
                if ( !_solved && this._restrictions.Under( limitA ) )
                {
                    if (debugMessages)
                        Debug.Log( name + ": False | I'm under " + limitA.name + " and not available!");

                    return false;
                }

                if ( !_solved && this._restrictions.Under( limitB ) )
                {
                    if (debugMessages)
                        Debug.Log( name + ": False | I'm under " + limitB.name + " and not available!");

                    return false;
                }
                
                if (debugMessages && _solved)
                {
                    Debug.Log( name + ": True | AA | Not buried in piles and available!");
                }

                return true;
            }
            else if ( _status == CardStatus.Stock || _status == CardStatus.Foundation )
            {
                if ( _rank == 13)
                    return true;

                bool a, b;

                if ( _solverA.OffTable && _solverB.OffTable )
                {
                    if ( debugMessages )
                        Debug.Log( name + " | AA | Both solvers " + _solverA.name + " and " + _solverB.name + " are off table, only one needs to be checked!");

                    a = _solverA.NextSolverAvailable( limitA, limitB, debugMessages );
                    b = a;
                }
                else
                {
                    a = _solverA.NextSolverAvailable( limitA, limitB, debugMessages );
                    b = _solverB.NextSolverAvailable( limitA, limitB, debugMessages );
                }
                
                if ( debugMessages )
                    Debug.Log( name + " | AA | " +_solverA.name + ": " + a + " | "+ _solverB.name +": " + b);

                return (a || b);
            }
            else if ( _status == CardStatus.Blocked )
            {
                if ( debugMessages && cardAbove != null )
                    Debug.Log( name + ": False | I'm blocked under " + cardAbove.name );

                return false;
            }
            return false;
        }

        private bool NextSuitAvailable(Restrictions limit, List<Card> blockList, List<Card> solverList, bool debugMessages = false)
        {
            if (debugMessages)
                Debug.Log( name + "(" + _status + ") | NSA Called!" );

            if ( !_solved )
            {
                if ( _status == CardStatus.Closed )
                {
                    if ( this._restrictions.Under( limit ) )
                    {
                        if ( debugMessages )
                            Debug.Log( name + " | NSA I'm under " + limit.name + " and can't be used!");

                        return false;
                    }
                }

                if ( _status == CardStatus.Blocked )
                {
                    if (debugMessages)
                        Debug.Log(name + " | NSA I'm blocked top card but let's assume I can be freed!");

                    blockList.Add( this );
                }
                else if ( cardAbove != null )
                {
                    if (debugMessages)
                        Debug.Log( name + " | NSA | Let's check: Abovecard " + cardAbove.name );

                    bool acs = cardAbove.AboveCardSolvable( limit, limit, debugMessages);

                    if ( !acs )
                    {                        
                        if (debugMessages)
                            Debug.Log( name + " | NSA | Abovecard " + cardAbove.name + " returned false!" );

                        blockList.Add( cardAbove );

                        return false;
                    }
                    else if ( debugMessages )
                        Debug.Log(name + " | NSA | Above cards OK!");
                }
                else if (debugMessages)
                    Debug.Log(name + " | NSA I'm top card, no need to check above cards!");
            }
            else if ( _status == CardStatus.Blocked )
            {
                if (debugMessages && cardAbove != null)
                    Debug.Log(name + " | NSA I'm blocked under " + cardAbove.name + " but let's assume I can be freed!");

                blockList.Add( this );
            }

            if ( OnTable )
            {
                if ( _status == CardStatus.Blocked )
                    blockList.Add( this );
                else
                    solverList.Add( this );
            }

            if ( _currentEffect != Effect.Solvable && _status != CardStatus.Foundation )
                Highlight( Effect.MustSuitSolve );

            if ( _rank > 1 )
            {
                if ( _status == CardStatus.Foundation )
                {
                    if (debugMessages)
                        Debug.Log( name + " | NSA | I'm in Foundation, this is deep enough!");

                    return true;
                }

                if (debugMessages)
                    Debug.Log( name + "(" + _status + ") | NSA | Let's check next card: " + _suitDown.name );

                return _suitDown.NextSuitAvailable( limit, blockList, solverList, debugMessages );
            }
            else
            {
                if (debugMessages)
                    Debug.Log( name + " | NSA | I'm Ace, this is deep enough!");

                return true;
            }
        }

        private bool ParallelSuitRoute( Restrictions limit, List<Card> blockList, List<Card> solverList, bool debugMessages )
        {
            return _suitDown.NextSuitAvailable(limit, blockList, solverList, debugMessages);
        }

    #region CardHover
        public void SaveOffset(Vector3 reference)
        {
            _original = transform.position;
            _offset = transform.position - reference + Vector3.up * 1.0f;
        }

        public void Hover(Vector3 cursor)
        {
            transform.position = cursor + _offset;
        }

        public void EndHover()
        {
            transform.position = _original;
        }

        public void ShowSolver(bool show)
        {
            if ( show )
                SetColor( Settings.Instance.yellow );
            else
                NormalColor();

            foreach (Card card in _blockList)
            {
                if ( show )
                    card.HighlightColor( Settings.Instance.block );
                else
                    card.NormalColor();
            }

            foreach (Card card in _solverList)
            {
                if ( show )
                    card.HighlightColor( Settings.Instance.solver );
                else
                    card.NormalColor();
            }
        }

        public void HighlightColor( Color color )
        {
            SetColor( color );

            if ( _status == CardStatus.Foundation )
            {
                transform.position = _original + Vector3.back * 2f;
            }
        }

        private void NormalColor()
        {
            if ( (_suitRouteBlocked && _parallelSuitRouteBlocked) && _aRouteBlocked && _bRouteBlocked )
                SetColor( Settings.Instance.block );

            else if ( _status == CardStatus.Closed )
                SetColor( Settings.Instance.closed );

            //else if ( _status == CardStatus.Blocked )
            //    SetColor( Settings.Instance.yellow );

            else
                SetColor( Settings.Instance.open );

            if ( _status == CardStatus.Foundation )
                transform.position = _original;
        }
    }
    #endregion CardHover
}
