using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class WastePile : PlayerPile
    {
        [Header("Waste pile specific")]
        [SerializeField] private Stock _stock;

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

        public override void ReceiveCard(Card card, bool moveCardGroup = false)
        {
            float random = Random.Range(-15f, 15f);
            Vector3 offset = new Vector3(
               Random.Range(-0.5f, 0.5f),
               0f,
               Random.Range(-0.5f, 0.5f) 
            );

            card.transform.SetParent(transform);

            _pile[_numberOfCards] = card;
            card.MoveTo(
                position: _positions[_numberOfCards] + offset,
                rotation: Quaternion.Euler(0f, random, 0f),
                instant: true,
                pile: _type
            );

            _numberOfCards++;
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
