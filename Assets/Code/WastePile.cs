using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class WastePile : CardPile
    {
        [Header("Waste pile specific")]
        [SerializeField] private Stock _stock;
        [SerializeField] private bool _mouseOver = false;
        public void Init()
        {
            _type = PileType.WasteHeap;

            _pile = new Card[52];
            _positions = new Vector3[52];
            
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;

            for (int i = 0; i < _positions.Length; i++)
            {
                y += Settings.Instance.cardThickness;

                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;
            }

            _rotation = Settings.Instance.faceUp;
        }

        private void Update()
        {
            if ( _mouseOver )
            {
                if ( Input.GetAxis("Mouse ScrollWheel") > 0f )
                    NextCardAction( strong: false );
                else if ( Input.GetAxis("Mouse ScrollWheel") < 0f )
                    PreviousCardAction();
            }            
        }

        private void OnMouseUpAsButton()
        {
            NextCardAction( true );
        }

        private void OnMouseDown()
        {
            Debug.Log("Dragging from " + name);
        }

        private void OnMouseEnter()
        {
            _mouseOver = true;
        }

        private void OnMouseExit()
        {
            _mouseOver = false;
        }

        public void NextCardAction(bool strong)
        {
            if ( _stock.NumberOfCards > 0 )
                _stock.DealTopCard(this);

            else if ( _numberOfCards > 1 && strong )
            {
                //return all current cards
                for (int i = _numberOfCards-1; i >= 0; i--)
                {
                    _stock.ReceiveCard( _pile[i] );
                    _numberOfCards--;
                }
            }
        }

        public void PreviousCardAction()
        {
            if ( _numberOfCards > 1 )
            {
                _stock.ReceiveCard( _pile[ _numberOfCards-1 ] );
                _numberOfCards--;
            }
        }
    }
}
