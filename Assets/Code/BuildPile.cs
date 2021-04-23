using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class BuildPile : PlayerPile
    {
        [Header("Build pile specific")]
        [SerializeField] private ClosedPile _closed;
        [SerializeField] private Track _track = Track.Empty;

        public void Init()
        {
            _type = PileType.BuildPile;

            _pile = new Card[13];
            _positions = new Vector3[13];

            _rotation = Quaternion.Euler(
                x: -Settings.Instance.cardAngle,
                y: 0f,
                z: 0f
            );

            float x = 0f;
            float y = 0f;
            float z = 0f;

            for (int i = 0; i < 13; i++)
            {
                _positions[i].x = x;
                _positions[i].y = y;
                _positions[i].z = z;

                z -= Settings.Instance.BuildCardMaxPacing;
            }
        }

        public override void ReceiveCard(Card card, bool moveCardGroup = false)
        {
            card.transform.SetParent(transform);

            _pile[_numberOfCards] = card;
            card.MoveTo(
                position: transform.position + _positions[_numberOfCards],
                rotation: _rotation,
                instant: true,
                pile: _type,
                parent: this,
                moveCardGroup: moveCardGroup
            );

            _numberOfCards++;

            if ( _track == Track.Empty )
                _track = card.Track;

            if ( _numberOfCards > 1 )
                _pile[_numberOfCards-2].Blocked( card );

            if ( _track != card.Track )
                _track = card.Track;
        }

        public override void ReceiveToIndex(Card card, int index)
        {
            card.transform.SetParent(transform);
            _pile[ index ] = card;

            card.MoveTo(
                position: transform.position + _positions[ index ],
                rotation: _rotation,
                instant: true,
                pile: _type,
                parent: this
            );

            _numberOfCards++;

            if ( _track != card.Track )
                _track = card.Track;
        }

        public override void DealTopCard(CardPile pile)
        {
            TurnHistory.Instance.ReportMove(
                card: TopCard,
                source: this,
                target: pile
            );

            base.DealTopCard(pile);
            CheckEmpty();
        }

        public override void TopCardTaken()
        {
            _pile[ _numberOfCards-1 ] = null;
            _numberOfCards--;
        }

        public override void ReturnCard(Card card, CardPile pile, int sourceIndex)
        {
            int index = IndexOf( card );
            pile.ReceiveToIndex( _pile[ index ], sourceIndex );
            _pile[ index ] = null;
            _numberOfCards--;
        }

        public void CheckEmpty()
        {
            if ( _numberOfCards == 0 )
            {
                _track = Track.Empty;

                if ( _closed.TopCard != null )
                    _closed.DealTopCard( this );
            }
            else
                _pile[ _numberOfCards-1 ].UnBlocked();
        }

        public override void DealSequenceOfCards(CardPile targetPile)
        {
            int rank, i;

            if ( !targetPile.hasCards )
            {
                rank = 13;
                i = 0;
            }
            else if ( _track == targetPile.TopCard.Track )
            {
                rank = targetPile.TopCard.Rank - 1;
                i = 0;
            }
            else
                return;

            while ( i < _numberOfCards )
            {
                if ( _pile[i].Rank > rank )
                    i++;
                else if ( _pile[i].Rank == rank )
                    break;
                else
                    return;
            }

            if ( i == 0 )
                _pile[i].Solved();

            while ( i < 13 )
            {
                if ( _pile[i] != null )
                {
                    TurnHistory.Instance.ReportMove(
                        card: _pile[i],
                        source: this,
                        target: targetPile
                    );

                    targetPile.ReceiveCard( card: _pile[i], moveCardGroup: true );
                    _pile[i] = null;
                    _numberOfCards--;
                    i++;
                }
                else
                {
                    CheckEmpty();
                    return;
                }
            }
        }

        public override bool AcceptsCard(Card card)
        {
            if ( _track == Track.Empty && card.Rank == 13 )
                return true;
            else if ( _track == card.Track &&
                      _pile[ _numberOfCards-1 ].Rank - 1 == card.Rank )
            {
                return true;
            }
            else
                return false;
        }

        public override void PopulateHoverList(List<Card> cardList, Card card = null)
        {
            int index = 0;

            if ( card != null )
            {
                index = IndexOf( card );
            }

            for (int i = index; i < _numberOfCards; i++)
            {
                cardList.Add( _pile[i] );
            }
        }
    }
}
