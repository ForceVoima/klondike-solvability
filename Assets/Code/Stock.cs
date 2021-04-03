using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class Stock : CardPile
    {
        [Header("Stock specific")]
        [SerializeField] private GameObject _cards;

        public void Init()
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

        public void Shuffle()
        {
            Card[] array = new Card[_pile.Length];

            for (int i = 0; i < _pile.Length; i++)
            {
                array[i] = _pile[i];
                _pile[i] = null;
            }

            int random = 0;
            Card temp;

            for (int i = 0; i < _pile.Length; i++)
            {
                random = Random.Range(0, _pile.Length - 1);

                temp = array[random];
                array[random] = array[i];
                array[i] = temp;
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i].MoveTo( _positions[i], _rotation, true);
                _pile[i] = array[i];
            }
        }

        public void DealCard(CardPile pile)
        {
            pile.TakeCard( _pile[ _numberOfCards-1 ]);
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
        }
    }
}
