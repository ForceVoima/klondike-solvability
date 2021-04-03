using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class GameMaster : MonoBehaviour
    {
        [SerializeField] private Stock _stock;
        // Waste heap

        //Foundations

        [SerializeField] private ClosedPile[] _closedPiles;

        private void Awake()
        {
            _stock.Init();

            foreach (ClosedPile pile in _closedPiles)
            {
                pile.Init();
            }

            NewDeal();
        }

        private void NewDeal()
        {
            _stock.Shuffle();

            int pile = 1;

            while (pile <= 7)
            {
                for (int i = pile-1; i < 7; i++)
                {
                    _stock.DealCard( _closedPiles[i] );
                }

                pile++;
            }
        }

        public void SetSpacing()
        {

        }
    }
}
