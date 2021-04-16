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
        [SerializeField] private int[] _advancedBlocks;

        public static Statistics _instance;
        public static Statistics Instance
        {
            get { return _instance; }
        }

        private List<string> _dataOut;
        private bool _firstCard = true;

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
            _advancedBlocks = new int[6];
            _dataOut = new List<string>( 1000 );
        }

        public void Report(Card card, bool suitBlocked, int solversBlocked)
        {
            if ( _firstCard )
            {
                _ranks[ (card.Rank-1) ]++;
                _suits[ (int)card.Suit ]++;
                _firstCard = false;
            }

            if (!suitBlocked)
                _blocks[ solversBlocked*2 ]++;
            else
                _blocks[ ( solversBlocked*2 + 1 ) ]++;
        }

        public void AdvancedReport(bool suitBlocked, bool routeA, bool routeB)
        {
            int solversBlocked = 0;

            if (routeA)
                solversBlocked++;

            if (routeB)
                solversBlocked++;

            if ( !suitBlocked )
                _advancedBlocks[ solversBlocked*2 ]++;
            else
                _advancedBlocks[ solversBlocked*2 + 1 ]++;
        }

        public string GameData()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _blocks.Length; i++)
            {
                sb.Append( _blocks[i].ToString() ).Append(',');
            }

            for (int i = 0; i < _advancedBlocks.Length; i++)
            {
                if (i == _blocks.Length-1)
                    sb.Append( _advancedBlocks[i].ToString() );
                else
                    sb.Append( _advancedBlocks[i].ToString() ).Append(',');
            }

            return sb.ToString();
        }

        public void SaveData()
        {
            if ( _blocks[0] == 0 )
                return;

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
                sb.Append( _blocks[i].ToString() ).Append(',');
                _blocks[i] = 0;
            }

            for (int i = 0; i < _advancedBlocks.Length; i++)
            {
                if (i == _blocks.Length-1)
                    sb.Append( _advancedBlocks[i].ToString() );
                else
                    sb.Append( _advancedBlocks[i].ToString() ).Append(',');

                _advancedBlocks[i] = 0;
            }

            _dataOut.Add( sb.ToString() );
            _firstCard = true;
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

        public bool Unsovable()
        {
            return ( _advancedBlocks[5] > 0 );
        }
    }
}
