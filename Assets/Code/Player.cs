﻿using System.Collections;
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

        [SerializeField] private bool _mouseDown = false;
        [SerializeField] private float _mouseDownTimer = 0f, _betweenClicks = 0f;

        [SerializeField] private PileType _sourceType = PileType.NotSet;
        [SerializeField] private PileType _mouseOverType = PileType.NotSet;

        [SerializeField] private PlayerPile _sourcePile;
        [SerializeField] private PlayerPile _mouseOverPile;

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate settings!");
                return;
            }
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
                SetSource();
            }
            
            if ( Input.GetMouseButtonUp(0) )
                MouseButtonOneAction();

            if ( Input.GetMouseButtonUp(1) )
                MouseButtonTwoAction();

            MouseWheelInputs();
        }

        private void MouseButtonOneAction()
        {
            _mouseDown = false;

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

            foreach (FoundationPile pile in _foundations)
            {
                if ( pile.AcceptsCard( card ) )
                {
                    _mouseOverPile.DealTopCard( pile );
                    return;
                }
            }
        }

        private void MouseWheelInputs()
        {
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
                _sourcePile.DealTopCard( _mouseOverPile );
        }

        private void BuildDrag()
        {
            if ( _mouseOverType == PileType.FoundationPile )
            {
                if ( _mouseOverPile.AcceptsCard( _sourcePile.TopCard ) )
                    _sourcePile.DealTopCard( _mouseOverPile );
            }

            if ( _mouseOverType == PileType.BuildPile )
                _sourcePile.DealSequenceOfCards( _mouseOverPile );
        }

        private void SetSource()
        {
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
    }
}
