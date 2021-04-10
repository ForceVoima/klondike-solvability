using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Klondike
{
    public class Statistics : MonoBehaviour
    {
        [SerializeField] private int[] _suits;
        [SerializeField] private int[] _ranks;

        [SerializeField] private int[] _blocks;

        public static Statistics _instance;
        public static Statistics Instance
        {
            get { return _instance; }
        }

        private List<string> _dataOut;

        public void Init()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate statistics!");
                return;
            }

            _suits = new int[4];
            _ranks = new int[13];
            _blocks = new int[6];
            _dataOut = new List<string>( 1000 );

            //_dataOut.Add("C,S,D,H,A,2,3,4,5,6,7,8,9,10,J,Q,K,Free,Suit,1Solver,1SolverSuit,2Solver,2SoverSuit");
        }

        public void CountsSuitsAndRanks(Card[] cards, int numberOfCards)
        {
            for (int i = 0; i < numberOfCards; i++)
            {
                _ranks[cards[i].Rank-1]++;
                _suits[ ((int)cards[i].Suit) ]++;
            }
        }

        public void Report(Card card, bool suitBlocked, int solversBlocked)
        {
            _ranks[ (card.Rank-1) ]++;
            _suits[ (int)card.Suit ]++;

            if (!suitBlocked)
                _blocks[ solversBlocked*2 ]++;
            else
                _blocks[ ( solversBlocked*2 + 1 ) ]++;
        }

        public string GameData()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _blocks.Length; i++)
            {
                if (i == _blocks.Length-1)
                    sb.Append( _blocks[i].ToString() );
                else
                    sb.Append( _blocks[i].ToString() ).Append(',');

                _blocks[i] = 0;
            }

            return sb.ToString();
        }

        public void SaveData()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _suits.Length; i++)
            {
                sb.Append( _suits[i].ToString() ).Append(',');
                _suits[i] = 0;
            }

            for (int i = 0; i < _ranks.Length; i++)
            {
                sb.Append( _ranks[i].ToString() ).Append(',');
                _ranks[i] = 0;
            }

            for (int i = 0; i < _blocks.Length; i++)
            {
                if (i == _blocks.Length-1)
                    sb.Append( _blocks[i].ToString() );
                else
                    sb.Append( _blocks[i].ToString() ).Append(',');

                _blocks[i] = 0;
            }

            _dataOut.Add( sb.ToString() );
        }

        public void SaveToFile()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string str in _dataOut)
            {
                sb.Append( str ).Append( '\n');
            }
            var folder = Application.streamingAssetsPath;

            if(! Directory.Exists(folder) )
                Directory.CreateDirectory(folder);

            var filePath = Path.Combine(folder, "export.csv");

            if ( File.Exists(filePath))
                File.Delete(filePath);

            File.WriteAllText(filePath, sb.ToString() );

            Debug.Log($"CSV file written to \"{filePath}\"" + " at " + Time.fixedTime );

        #if UNITY_EDITOR
            AssetDatabase.Refresh();
        #endif
        }
    }
}
