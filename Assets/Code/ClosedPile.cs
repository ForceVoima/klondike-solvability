using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class ClosedPile : CardPile
    {
        [Header("Closed pile specific")]
        [SerializeField] private BuildPile _build;

        public void Init()
        {
            _type = PileType.ClosedPile;

            _pile = new Card[7];
            _positions = new Vector3[7];

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

            for (int i = 0; i < 7; i++)
            {
                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;

                z -= Settings.Instance.ClosedCardMinSpacing;
            }
        }

        public override void ReceiveCard(Card card)
        {
            card.ClosedCards( _pile, _numberOfCards );
            base.ReceiveCard(card);
        }

        public override void DealTopCard(CardPile pile)
        {
            base.DealTopCard(pile);
            _build.transform.position = _positions[ _numberOfCards ];
        }
    }
}
