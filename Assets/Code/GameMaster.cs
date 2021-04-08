using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Klondike
{
    public class GameMaster : Master
    {
        [SerializeField] protected Statistics _stats;
        [SerializeField] protected Settings _settings;
        [SerializeField] protected Player _player;
        public int _games = 0;

        [SerializeField, Header("Custom game")] private string _gameFileName;

        private void Awake()
        {
            Application.targetFrameRate = 100;

            _stats.Init();
            _settings.Init();
            _player.Init();

            _stock.Init();
            _stock.Shuffle();

            _waste.Init();

            foreach (FoundationPile pile in _foundations)
                pile.Init();

            foreach (ClosedPile pile in _closedPiles)
                pile.Init();

            foreach (BuildPile pile in _buildPiles)
                pile.Init();

            NewGame();
        }

        private void NewDeal()
        {
            _stock.Shuffle();

            int pile = 1;

            while (pile <= 7)
            {
                for (int i = pile-1; i < 7; i++)
                {
                    _stock.DealTopCard( _closedPiles[i] );
                }

                pile++;
            }
        }

        private void Reset()
        {
            foreach (FoundationPile pile in _foundations)
                pile.ResetCards( _stock );

            foreach (ClosedPile pile in _closedPiles)
                pile.ResetCards( _stock );

            foreach (BuildPile pile in _buildPiles)
                pile.ResetCards( _stock );

            _waste.ResetCards( _stock );

            Statistics.Instance.SaveData();
        }

        public void NewGame()
        {
            Reset();
            NewDeal();

            foreach (BuildPile pile in _buildPiles)
                pile.CheckEmpty();
        }

        /*
        private void Update()
        {
            if (_games < 1)
            {
                NewGame();
                _games++;

                //if (_games == 100000)
                //    Statistics.Instance.SaveToFile();
            }
        }
        */

        public void LoadCustomGame()
        {
            Reset();

            string path = Path.Combine( Application.streamingAssetsPath, _gameFileName);

            string allText = File.ReadAllText(path);
            string[] lines = allText.Split("\n"[0]);
            string[] lineData;

            Suit suit;
            int rank;

            for (int i = 0; i < 7; i++)
            {
                lineData = lines[i].Split('-');

                for (int j = 0; j < lineData.Length; j++)
                {
                    ReadCard( lineData[j], out suit, out rank);

                    _stock.DealCardTo(
                        pile: _closedPiles[i],
                        suit: suit,
                        rank: rank
                    );
                }
            }

            foreach (BuildPile pile in _buildPiles)
                pile.CheckEmpty();
        }

        private void ReadCard(string text, out Suit suit, out int rank)
        {
            char suitChar = text[0];
            char rankChar = text[1];

            if ( suitChar == 'C')
                suit = Suit.Club;
            else if ( suitChar == 'S')
                suit = Suit.Spade;
            else if ( suitChar == 'H')
                suit = Suit.Heart;
            else
                suit = Suit.Diamond;

            if ( rankChar == 'A' )
                rank = 1;
            else if ( rankChar == 'X' )
                rank = 10;
            else if ( rankChar == 'J' )
                rank = 11;
            else if ( rankChar == 'Q' )
                rank = 12;
            else if ( rankChar == 'K' )
                rank = 13;
            else
                rank = int.Parse( text.Substring(1) );
        }
    }
}
