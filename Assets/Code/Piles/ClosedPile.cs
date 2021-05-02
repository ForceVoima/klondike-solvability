using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class ClosedPile : CardPile
    {
        [Header("Closed pile specific")]
        [SerializeField] private BuildPile _build;
        [SerializeField, Range(1,7)] private int _pileNumber;

        public void Init()
        {
            _type = PileType.ClosedPile;

            _pile = new Card[8];
            _positions = new Vector3[8];

            _rotation = Quaternion.Euler(
                x: -Settings.Instance.cardAngle,
                y: 0f,
                z: 0f
            );

            float x = transform.position.x;
            float y = transform.position.y +
                      Settings.Instance.cardThickness +
                      Mathf.Sin(Mathf.Deg2Rad * Settings.Instance.cardAngle) *
                      Settings.Instance.cardHeight/2f;
            float z = transform.position.z;

            for (int i = 0; i < 8; i++)
            {
                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;

                z -= Settings.Instance.ClosedCardMinSpacing;
            }
        }

        public override void ReceiveCard(Card card, bool moveCardGroup = false)
        {
            if ( hasCards ) 
            {
                TopCard.cardAbove = card;
                card.cardBelow = TopCard;
            }

            card.ClosedCards( _pile, _pileNumber, _numberOfCards );
            base.ReceiveCard(card);
            _build.transform.position = _positions[ _numberOfCards ];
        }

        public override void DealTopCard(CardPile pile)
        {
            TurnHistory.Instance.ReportMove(
                card: TopCard,
                source: this,
                target: pile
            );

            pile.ReceiveCard( _pile[ _numberOfCards-1 ]);
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
            
            _build.transform.position = _positions[ _numberOfCards ];
        }
    }
}
