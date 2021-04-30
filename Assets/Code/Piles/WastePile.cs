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
                pile: _type,
                parent: this
            );

            _numberOfCards++;
        }

        public void ReceiveCardToIndex(Card card, int index)
        {
            int i = _numberOfCards;
            Vector3 currentPos;
            Quaternion currentRot;

            while ( i > index )
            {
                _pile[i] = _pile[i-1];
                currentPos = _pile[i-1].transform.position;
                currentPos.y = _positions[i].y;
                currentRot = _pile[i-1].transform.rotation;

                _pile[i].MoveTo(
                    position: currentPos,
                    rotation: currentRot,
                    instant: true,
                    pile: _type,
                    parent: this
                );
                i--;
            }

            currentPos.x = _positions[ index ].x + Random.Range(-0.5f, 0.5f);
            currentPos.y = _positions[ index ].y;
            currentPos.z = _positions[ index ].z + Random.Range(-0.5f, 0.5f);

            currentRot = Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f);

            card.transform.SetParent(transform);
            _pile[ index ] = card;

            card.MoveTo(
                position: currentPos,
                rotation: currentRot,
                instant: true,
                pile: _type,
                parent: this
            );

            _numberOfCards++;
        }

        public override void DealTopCard(CardPile pile)
        {
            TurnHistory.Instance.ReportMove(
                card: TopCard,
                source: this,
                target: pile
            );
            
            base.DealTopCard(pile);
        }

        
        public override void DealCardTo(CardPile pile, Suit suit, int rank)
        {
            int i = IndexOf(suit, rank);
            
            TurnHistory.Instance.ReportMove(
                card: _pile[i],
                source: this,
                target: pile
            );

            pile.ReceiveCard( _pile[i] );
            _numberOfCards--;

            Vector3 currentPos;
            Quaternion currentRot;

            while (i < _numberOfCards)
            {
                _pile[i] = _pile[i+1];
                currentPos = _pile[i].transform.position;
                currentPos.y = _positions[i].y;
                currentRot = _pile[i].transform.rotation;

                _pile[i].MoveTo(
                    position: currentPos,
                    rotation: currentRot,
                    instant: true,
                    pile: _type,
                    parent: this
                );
                i++;
            }

            _pile[_numberOfCards] = null;
        }

        public void NextCardAction(bool strong)
        {
            if ( _stock.hasCards )
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

            _pile[ _numberOfCards ] = null;
        }
    }
}
