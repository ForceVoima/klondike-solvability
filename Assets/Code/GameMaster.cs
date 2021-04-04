﻿using System.Collections;
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

        public int _games = 0;

        private bool _gameRunning = false;

        private void Awake()
        {
            _stock.Init();

            foreach (ClosedPile pile in _closedPiles)
            {
                pile.Init();
            }
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

            Statistics.Instance.SaveData();
        }

        private void Reset()
        {
            for(int i = 0; i < 7; i++)
            {
                _closedPiles[i].ResetCards( _stock );
            }
        }

        public void NewGame()
        {
            Reset();
            NewDeal();
        }

        private void Update()
        {
            if (_games < 1000)
            {
                NewGame();
                _games++;

                if (_games == 1000)
                    Statistics.Instance.SaveToFile();
            }
        }
    }
}
