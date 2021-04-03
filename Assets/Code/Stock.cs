using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class Stock : CardPile
    {
        [Header("Stock specific")]
        [SerializeField] private GameObject _cards;

        private void Awake()
        {
            _type = PileType.Stock;

            _positions = new Vector3[52];

            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;

            for (int i = 0; i < 52; i++)
            {
                y += _settings.cardThickness;

                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;
            }

            _rotation = _settings.faceDown;

            InitCards();
        }

        private void InitCards()
        {
            _pile = GetComponentsInChildren<Card>();

            for (int i = 0; i < _pile.Length; i++)
            {
                _pile[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true );

                _numberOfCards++;
            }
        }
    }
}
