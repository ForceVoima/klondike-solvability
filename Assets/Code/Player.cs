using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class Player : Master
    {
        private static Player _instance;
        public static Player Instance
        {
            get { return _instance; }
        }

        [SerializeField] private AIMaster _ai;

        [SerializeField] private bool _mouseDown = false;
        [SerializeField] private bool _cardHoverActive = false;
        [SerializeField] private bool _cardSequenceHoverActive = false;
        public List<Card> _hoverList;
        [SerializeField] private float _mouseDownTimer = 0f, _betweenClicks = 0f;

        [SerializeField] private PileType _sourceType = PileType.NotSet;
        [SerializeField] private PileType _mouseOverType = PileType.NotSet;

        [SerializeField] private PlayerPile _sourcePile;
        [SerializeField] private PlayerPile _mouseOverPile;
        private Card _activeCard;
        private Card _showSolversCard;
        [SerializeField] private Camera _sceneCamera;
        private Vector3 _mousePointer;

        public int pilesMask;
        public int hoverMask;
        public int cardMask;

        public void Init()
        {
            pilesMask = LayerMask.GetMask("Piles");

            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate settings!");
                return;
            }

            pilesMask = LayerMask.GetMask("Piles");
            hoverMask = LayerMask.GetMask("Default", "Piles");
            cardMask = LayerMask.GetMask("Cards");

            _hoverList = new List<Card>();
        }

        private void Update()
        {
            _betweenClicks += Time.deltaTime;

            if ( _mouseDown )
                _mouseDownTimer += Time.deltaTime;

            if ( Input.GetMouseButtonDown(0) )
            {
                _mouseDown = true;
                _mouseDownTimer = 0f;
                ClickRaycast();
                DraggedCard();
                SetSource();
            }
            
            if ( Input.GetMouseButtonUp(0) )
                MouseButtonOneAction();

            if ( Input.GetMouseButtonUp(1) )
                MouseButtonTwoAction();

            if ( Input.GetMouseButtonDown(2) ||
                 Input.GetMouseButtonDown(3) ||
                 Input.GetMouseButtonDown(4) )
                MouseFourDown();
                
            if ( Input.GetMouseButtonUp(2) ||
                 Input.GetMouseButtonUp(3) ||
                 Input.GetMouseButtonUp(4) )
                MouseFourUp();

            MouseWheelInputs();

            if ( _cardHoverActive )
                CardHoverActions();

            else if ( _cardSequenceHoverActive )
                CardHoverListAction();
        }

        private void MouseButtonOneAction()
        {
            ClickRaycast();

            _mouseDown = false;

            if ( _cardHoverActive )
            {
                _activeCard.EndHover();
                _cardHoverActive = false;
            }

            if ( _cardSequenceHoverActive )
            {
                foreach (Card card in _hoverList)
                {
                    card.EndHover();
                }
                _cardSequenceHoverActive = false;
            }

            if ( _sourceType == PileType.NotSet ||
                 _mouseOverType == PileType.NotSet )
            {
                _betweenClicks = 0f;
            }
            else if ( _sourcePile == _mouseOverPile )
            {
                SelfClick();
            }
            else if ( _sourceType == PileType.Stock )
                StockDrag();
            else if ( _sourceType == PileType.WasteHeap )
                WasteDrag();
            else if ( _sourceType == PileType.BuildPile )
                BuildDrag();
            else if ( _sourceType == PileType.FoundationPile )
                FoundationDrag();
            
            _betweenClicks = 0f;
        }

        private void SelfClick()
        {
            if ( _mouseOverType == PileType.Stock )
            {
                _waste.NextCardAction( strong: true );
            }
            //Double click
            else if ( _betweenClicks < 0.3f )
            {
                AttemptSolve();
            }
        }

        private void MouseButtonTwoAction()
        {
            ClickRaycast();

            if ( _mouseOverType == PileType.Stock )
                _waste.PreviousCardAction();
            else if ( _mouseOverType == PileType.WasteHeap ||
                      _mouseOverType == PileType.BuildPile )
            {
                AttemptSolve();
            }
        }

        private void AttemptSolve()
        {
            Card card = _mouseOverPile.TopCard;

            if ( card == null )
                return;
            
            _ai.SolveUntil( card );
        }

        private void MouseWheelInputs()
        {
            if ( Input.GetAxis("Mouse ScrollWheel") != 0f )
            {
                ClickRaycast();
            }

            if (_mouseOverType == PileType.WasteHeap ||
                _mouseOverType == PileType.Stock )
            {
                if ( Input.GetAxis("Mouse ScrollWheel") > 0f )
                    _waste.NextCardAction( strong: false );

                else if ( Input.GetAxis("Mouse ScrollWheel") < 0f )
                    _waste.PreviousCardAction();
            }
        }

        private void StockDrag()
        {
            if ( _mouseOverType == PileType.WasteHeap )
                _waste.NextCardAction( strong: true );
        }

        private void WasteDrag()
        {
            if ( _mouseOverType == PileType.Stock )
                _waste.PreviousCardAction();

            if ( _mouseOverType == PileType.FoundationPile )
            {
                if ( _mouseOverPile.AcceptsCard( _sourcePile.TopCard ) )
                {
                    TurnHistory.Instance.StartNewMove();
                    _sourcePile.DealTopCard( _mouseOverPile );
                }
            }

            if ( _mouseOverType == PileType.BuildPile )
                BuildPileBuild();
        }

        private void FoundationDrag()
        {
            if ( _mouseOverType == PileType.BuildPile )
                BuildPileBuild();
        }

        private void BuildPileBuild()
        {
            if ( _mouseOverPile.AcceptsCard( _sourcePile.TopCard ) )
            {
                TurnHistory.Instance.StartNewMove();
                _sourcePile.DealTopCard( _mouseOverPile );
            }
        }

        private void BuildDrag()
        {
            if ( _mouseOverType == PileType.FoundationPile )
            {
                if ( _mouseOverPile.AcceptsCard( _sourcePile.TopCard ) )
                {
                    TurnHistory.Instance.StartNewMove();
                    _sourcePile.DealTopCard( _mouseOverPile );
                }
            }

            if ( _mouseOverType == PileType.BuildPile )
            {
                TurnHistory.Instance.StartNewMove();
                _sourcePile.DealSequenceOfCards( _mouseOverPile );
            }
        }

        private void SetSource()
        {
            if ( _cardHoverActive && _activeCard.IsClosed )
            {
                _sourceType = PileType.NotSet;
                _sourcePile = null;
                return;
            }

            _sourceType = _mouseOverType;
            _sourcePile = _mouseOverPile;
        }

        public void Entered(PileType type, PlayerPile pile)
        {
            _mouseOverType = type;
            _mouseOverPile = pile;
        }

        public void Exited(PileType type, PlayerPile pile)
        {
            _mouseOverType = PileType.NotSet;
            _mouseOverPile = null;
        }

        private void ClickRaycast()
        {
            RaycastHit hit;
            Ray ray = _sceneCamera.ScreenPointToRay(Input.mousePosition);

             if ( Physics.Raycast(
                  ray: ray,
                  hitInfo: out hit,
                  maxDistance: 100f,
                  layerMask: pilesMask ) )
            {
                PlayerPile pile = hit.transform.GetComponentInChildren<PlayerPile>();

                if ( pile != null)
                {
                    _mousePointer = hit.point;
                    _mouseOverPile = pile;
                    _mouseOverType = pile.Type;
                    return;
                }
            }

            _mouseOverType = PileType.NotSet;
            _mouseOverPile = null;
        }

        private void DraggedCard()
        {
            if ( _mouseOverType == PileType.Stock ||
                 _mouseOverType == PileType.NotSet )
                 return;
            else if ( _mouseOverType == PileType.WasteHeap )
            {
                if ( _mouseOverPile.hasCards )
                {
                    _activeCard = _mouseOverPile.TopCard;
                    _activeCard.SaveOffset( _mousePointer );
                    _cardHoverActive = true;
                }
                else
                {
                    _activeCard = null;
                    _cardHoverActive = false;
                }
                
                return;
            }

            if ( _mouseOverType == PileType.BuildPile )
            {
                SingleOrSequence();
                return;
            }

            if ( CardRaycast(out _activeCard) )
            {
                if ( _activeCard == null && _mouseOverPile != null )
                     _activeCard = _mouseOverPile.TopCard;

                _activeCard.SaveOffset( _mousePointer );
                _cardHoverActive = true;
                return;
            }
            else if ( _mouseOverPile != null && _mouseOverPile.hasCards )
            {
                _activeCard = _mouseOverPile.TopCard;
                _activeCard.SaveOffset( _mousePointer );
                _cardHoverActive = true;
                return;
            }
            _activeCard = null;
            _cardHoverActive = false;
        }

        private bool CardRaycast(out Card card)
        {
            RaycastHit hit;
            Ray ray = _sceneCamera.ScreenPointToRay(Input.mousePosition);

            if ( Physics.Raycast(
                 ray: ray,
                 hitInfo: out hit,
                 maxDistance: 100f,
                 layerMask: cardMask ) )
            {
                _mousePointer = hit.point;
                card = hit.transform.GetComponent<Card>();
                return true;
            }

            card = null;
            return false;
        }

        private void SingleOrSequence()
        {
            if ( CardRaycast(out _activeCard) )
            {
                if ( _activeCard.IsClosed )
                {
                    _activeCard.SaveOffset( _mousePointer );
                    _cardHoverActive = true;
                    return;
                }
                else
                {
                    _hoverList.Clear();
                    _mouseOverPile.PopulateHoverList( _hoverList, _activeCard );
                    InitCardHoverList();
                    _cardHoverActive = false;
                    _cardSequenceHoverActive = true;
                    return;
                }
            }
            
            if ( _mouseOverPile != null && _mouseOverPile.hasCards )
            {
                _hoverList.Clear();
                _mouseOverPile.PopulateHoverList( _hoverList );
                InitCardHoverList();
                _cardHoverActive = false;
                _cardSequenceHoverActive = true;
                return;
            }
        }

        private void InitCardHoverList()
        {
            for (int i = 0; i < _hoverList.Count; i++)
            {
                _hoverList[i].SaveOffset( _mousePointer );
            }
        }

        private void CardHoverActions()
        {
            RaycastHit hit;
            Ray ray = _sceneCamera.ScreenPointToRay(Input.mousePosition);

             if ( Physics.Raycast(
                  ray: ray,
                  hitInfo: out hit,
                  maxDistance: 100f,
                  layerMask: hoverMask ) )
            {
                _mousePointer = hit.point;
                _activeCard.Hover( hit.point );
            }
        }

        private void CardHoverListAction()
        {
            RaycastHit hit;
            Ray ray = _sceneCamera.ScreenPointToRay(Input.mousePosition);

             if ( Physics.Raycast(
                  ray: ray,
                  hitInfo: out hit,
                  maxDistance: 100f,
                  layerMask: hoverMask ) )
            {
                _mousePointer = hit.point;
                
                foreach (Card card in _hoverList)
                {
                    card.Hover( _mousePointer );
                }
            }
        }

        private void MouseFourDown()
        {
            if ( !CardRaycast( out _showSolversCard ) )
                return;

            if ( _showSolversCard == null || !_showSolversCard.OnTable )
                return;
            
            _showSolversCard.UpdateRoutes();
            _showSolversCard.ShowSolver(true);
        }

        private void MouseFourUp()
        {
            if ( _showSolversCard != null )
                _showSolversCard.ShowSolver(false);
            
            _showSolversCard = null;
        }
    }
}
