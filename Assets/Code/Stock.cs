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
        [SerializeField] private Settings _settings;
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

            _numberOfCards = 0;

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
            _allCards = new Card[_pile.Length];

            for (int i = 0; i < _pile.Length; i++)
            {
                _allCards[i] = _pile[i];
                _pile[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true,
                    pile: _type,
                    parent: this
                );

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
                array[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true,
                    pile: _type,
                    parent: this
                );
                _pile[i] = array[i];
            }
        }

        public void ResetCards()
        {
            for (int i = 0; i < _pile.Length; i++)
            {
                _pile[i].Reset();
            }
        }

        public void ReceiveCardToIndex(Card card, int index)
        {
            int i = _numberOfCards;
            Vector3 currentPos;

            while (i > index)
            {
                _pile[i] = _pile[i-1];
                currentPos = _pile[i-1].transform.position;

                _pile[i].MoveTo(
                    position: currentPos,
                    rotation: _rotation,
                    instant: true,
                    pile: _type,
                    parent: this
                );
                i--;
            }

            currentPos = _positions[ index ];

            card.transform.SetParent(transform);
            _pile[ index ] = card;

            card.MoveTo(
                position: currentPos,
                rotation: _rotation,
                instant: true,
                pile: _type,
                parent: this
            );

            _numberOfCards++;
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

            while (i < _numberOfCards)
            {
                _pile[i] = _pile[i+1];
                _pile[i].MoveTo(
                    position: _positions[i],
                    rotation: _rotation,
                    instant: true,
                    parent: this,
                    pile: _type
                );
                i++;
            }

            _pile[_numberOfCards] = null;
        }

        public void OrderCard(Suit suit, int rank, int order)
        {
            int cardSlot = IndexOf(suit, rank);
            int targetSlot = (_numberOfCards - 1) - order;

            Card temp = _pile[ targetSlot ];
            _pile[ targetSlot ] = _pile[ cardSlot ];
            _pile[ cardSlot ] = temp;
        }

        public Card RequestCard(Suit suit, int rank)
        {
            return _allCards[ (int)suit*13 + rank - 1 ];
        }
    }
}
