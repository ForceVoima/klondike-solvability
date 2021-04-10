using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class FoundationPile : PlayerPile
    {
        [Header("Foundation pile specific")]
        [SerializeField] private Suit _suit = Suit.NotSet;

        public void Init()
        {
            _type = PileType.FoundationPile;

            _pile = new Card[13];
            _positions = new Vector3[13];
            
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
        public override void ResetCards(Stock stock)
        {
            base.ResetCards(stock);
            StopAllCoroutines();
        }

        public override bool AcceptsCard(Card card)
        {
            if ( _suit == Suit.NotSet && card.Rank == 1 )
                return true;

            else if ( card.Suit == _suit && card.Rank == _numberOfCards + 1 )
                return true;

            else
                return false;
        }

        public override void ReceiveCard(Card card, bool moveCardGroup = false)
        {
            base.ReceiveCard(card);

            if ( _suit == Suit.NotSet )
                _suit = card.Suit;

            GameMaster.Instance.FoundationAddedCard();
            AIMaster.Instance.CardFounded(card.Suit, card.Rank);
        }

        public override void DealTopCard(CardPile pile)
        {
            base.DealTopCard(pile);
            GameMaster.Instance.FoundationTakenCard();
        }

        public void StartWinThrow()
        {
            StartCoroutine( ThrowCardsWin() );
        }

        public IEnumerator ThrowCardsWin()
        {
            int cards = 13;
            Vector3 impulse = new Vector3(0f, 20f, 0f);
            Vector3 torque = new Vector3();
            Card card;
            float angle;

            while ( cards > 0 )
            {
                card = _pile[ cards-1 ];

                angle = Random.Range( 200f, 300f );
                torque.x = Random.Range( 0f, 20f );
                torque.z = Random.Range( 0f, 20f );

                impulse.x = Mathf.Cos( Mathf.Deg2Rad * angle ) * 30f;
                impulse.z = Mathf.Sin( Mathf.Deg2Rad * angle ) * 30f;

                card.EnablePhysics();

                card.ThrowCard(
                    impulse: impulse,
                    mode: ForceMode.Impulse,
                    torque: torque
                );
                 
                cards--;

                yield return new WaitForSeconds(1f);
            }

            yield return null;
        }
    }
}
