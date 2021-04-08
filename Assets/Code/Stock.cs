using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class Stock : PlayerPile
    {
        [Header("Stock specific")]
        [SerializeField] private GameObject _cards;

        [SerializeField] private Card[] _allCards;
        public static Stock _instance;
        public static Stock Instance
        {
            get { return _instance; }
        }

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate stock!");
                return;
            }

            _type = PileType.Stock;

            _positions = new Vector3[52];

            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;

            for (int i = 0; i < 52; i++)
            {
                y += Settings.Instance.cardThickness;

                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;
            }

            _rotation = Settings.Instance.faceDown;

            InitCards();
        }

        private void InitCards()
        {
            _pile = GetComponentsInChildren<Card>();
            _allCards = new Card[_pile.Length];

            for (int i = 0; i < _pile.Length; i++)
            {
                _allCards[i] = _pile[i];
                _pile[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true );

                _numberOfCards++;
            }

            for (int i = 0; i < _pile.Length; i++)
            {
                _pile[i].Init();
            }
        }

        public void Shuffle()
        {
            Card[] array = new Card[_pile.Length];

            for (int i = 0; i < _pile.Length; i++)
            {
                _pile[i].Reset();
                array[i] = _pile[i];
                _pile[i] = null;
            }

            int random = 0;
            Card temp;

            for (int i = 0; i < _pile.Length; i++)
            {
                random = Random.Range(0, _pile.Length);

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

        public void DealCardTo(CardPile pile, Suit suit, int rank)
        {
            int i = 0;

            while (i < _pile.Length)
            {
                if ( _pile[i].Suit == suit && _pile[i].Rank == rank)
                    break;
                
                i++;
            }

            pile.ReceiveCard( _pile[i]);
            _numberOfCards--;

            while (i < _numberOfCards)
            {
                _pile[i] = _pile[i+1];
                _pile[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true
                );
                i++;
            }

            _pile[_numberOfCards] = null;
        }

        public Card RequestCard(Suit suit, int rank)
        {
            return _allCards[ (int)suit*13 + rank - 1 ];
        }
    }
}
