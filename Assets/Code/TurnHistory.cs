using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class TurnHistory : MonoBehaviour
    {
        private static TurnHistory _instance;
        public static TurnHistory Instance
        {
            get { return _instance; }
        }

        [SerializeField] private Stock _stock;
        [SerializeField] private WastePile _waste;

        [SerializeField] private List<Move> _moves;
        [SerializeField] private Move _currentMove;
        [SerializeField] private int _moveNumber = 0;
        

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate AI!");
                return;
            }

            _moves = new List<Move>();
        }

        public void Reset()
        {
            _moves.Clear();
        }

        public void Undo()
        {
            if ( _moves.Count <= 0 )
                return;

            int index = _moves.Count - 1;
            int move = _moves[ index ].moveNumber;
            _currentMove = _moves[ index ];

            while ( _currentMove.moveNumber == move )
            {
                if ( _currentMove.type == MoveType.Stock )
                    ReturnToStock( _currentMove );

                else if ( _currentMove.type == MoveType.TableToFoundation )
                    ReturnToTable( _currentMove );

                else if ( _currentMove.type == MoveType.ClosedToOpen )
                    ReturnToClosed( _currentMove );

                else if ( _currentMove.type == MoveType.TableToTable )
                    ReturnToBuildPile( _currentMove );

                else if ( _currentMove.type == MoveType.WorryBack )
                    ReturnToTable( _currentMove );

                _moves.RemoveAt( index );
                
                index--;

                if ( index < 0 )
                    break;

                if ( _moves[ index ].moveNumber == move )
                {
                    _currentMove = _moves[ index ];
                }
                else
                {
                    break;
                }
            }
        }

        public void StartNewMove()
        {
            if ( _moves.Count >= 1 )
                _moveNumber = _moves[ _moves.Count - 1 ].moveNumber + 1;
            else
                _moveNumber = 0;
        }

        public void ReportMove(Card card, CardPile source, CardPile target)
        {
            _currentMove = new Move( _moveNumber, card, source, target );

            if ( source.Type == PileType.Stock )
                _currentMove.sourceIndex = source.IndexOf( card );
            else if ( source.Type == PileType.WasteHeap )
                _currentMove.sourceIndex =
                    _stock.NumberOfCards - 1 +
                    _waste.NumberOfCards - source.IndexOf( card );
            else
                _currentMove.sourceIndex = source.IndexOf( card );

            _moves.Add( _currentMove );
        }

        private void ReturnToStock(Move move)
        {
            int newIndex = move.sourceIndex - (_stock.NumberOfCards - 1);

            if ( newIndex == 1 )
            {
                if ( move.sourcePile.Type == PileType.Stock )
                    move.targetPile.DealTopCard( _stock );

                else if ( move.sourcePile.Type == PileType.WasteHeap )
                    move.targetPile.DealTopCard( _waste );
            }
            else if ( move.sourceIndex < _stock.NumberOfCards )
            {
                move.targetPile.TopCardTaken();
                _stock.ReceiveCardToIndex( move.sourceCard, move.sourceIndex );
            }
            else
            {
                move.targetPile.TopCardTaken();
                newIndex = (_waste.NumberOfCards + 1) - newIndex;
                _waste.ReceiveCardToIndex( move.sourceCard, newIndex );
            }

            AIMaster.Instance.UpdateSolvable( move.sourceCard.Suit );
        }

        private void ReturnToTable(Move move)
        {
            move.targetPile.ReturnTopCard( move.sourcePile );
        }

        private void ReturnToClosed(Move move)
        {
            move.targetPile.ReturnTopCard( move.sourcePile );
        }

        private void ReturnToBuildPile(Move move)
        {
            move.targetPile.ReturnCard( move.sourceCard, move.sourcePile, move.sourceIndex );
        }
    }
}
