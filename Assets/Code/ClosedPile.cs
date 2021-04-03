using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class ClosedPile : CardPile
    {
        // [Header("Closed Pile specific")]

        public void Init()
        {
            _type = PileType.ClosedPile;

            _pile = new Card[7];
            _positions = new Vector3[7];

            _rotation = Quaternion.Euler(
                x: - _settings.cardAngle,
                y: 0f,
                z: 0f
            );

            float x = transform.position.x;
            float y = transform.position.y +
                      _settings.cardThickness +
                      Mathf.Sin(Mathf.Deg2Rad * _settings.cardAngle) *
                      _settings.cardHeight/2f;
            float z = transform.position.z;

            for (int i = 0; i < 7; i++)
            {
                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;

                z -= _settings.ClosedCardMinSpacing;
            }
        }

        public override void TakeCard(Card card)
        {
            card.transform.SetParent(transform);

            _pile[_numberOfCards] = card;
            card.MoveTo(
                position: _positions[_numberOfCards],
                rotation: _rotation,
                instant: true
            );

            _numberOfCards++;
        }
    }
}
