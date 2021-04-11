using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

namespace Klondike
{
    public class GameMaster : Master
    {
        [SerializeField] protected Statistics _stats;
        [SerializeField] protected Settings _settings;
        [SerializeField] protected Player _player;
        [SerializeField] protected AIMaster _ai;

        [SerializeField, Header("Custom game")] private string _gameFileName;

        public int _cardsInFoundation = 0;
        public int _gameProgress = -7;
        public int _gameSelector = 1;
        private int _totalGames;
        private int _wonGames;
        private float _winRate;

        [SerializeField] private GameObject _mainCamera, _winCamera;

        [Header("UI elements")]
        public TMPro.TextMeshProUGUI gameNameText;
        public TMPro.TextMeshProUGUI winRateText;
        public TMPro.TextMeshProUGUI gameProgressText;
        public TMPro.TextMeshProUGUI bestResultText;
        public UnityEngine.UI.Button _newGameButton;
        public UnityEngine.UI.Button _solveButton;

        [SerializeField] private KlondikeGame _currentGame;

        public GameDatabase _database;

        public static GameMaster _instance;
        public static GameMaster Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate GameMaster!");
                return;
            }

            Application.targetFrameRate = 100;

            _stats.Init();
            _settings.Init();
            _player.Init();

            _stock.Init();
            _stock.Shuffle();
            _ai.Init();

            _waste.Init();

            foreach (FoundationPile pile in _foundations)
                pile.Init();

            foreach (ClosedPile pile in _closedPiles)
                pile.Init();

            foreach (BuildPile pile in _buildPiles)
                pile.Init();

            LoadJson();
            LoadGame( _currentGame.piles );
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

            _currentGame = new KlondikeGame( _database.allGames.Count + 1 );
            
            for (int i = 0; i < _closedPiles.Length; i++)
            {
                _currentGame.piles[i] = _closedPiles[i].ToString();
            }

            _currentGame.piles[7] = _stock.ToString();

            if ( _currentGame.gameID != 0 && !_database.allGames.Contains(_currentGame) )
                _database.allGames.Add( _currentGame );

            _gameSelector = _database.allGames.Count - 1;

            UpdateUI();
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
            
            _stock.ResetCards();

            _ai.Reset();
            Statistics.Instance.SaveData();
            _cardsInFoundation = 0;
            _gameProgress = -7;
            StopAllCoroutines();
            _mainCamera.SetActive(true);
            _winCamera.SetActive(false);
        }

        public void NewGame()
        {
            SaveProgress();

            Reset();
            NewDeal();
            _currentGame.stats = _stats.GameData();

            foreach (BuildPile pile in _buildPiles)
                pile.CheckEmpty();

            _ai.HighlightSolvable();
        }

        public void LoadCustomGame()
        {
            Reset();

            string path = Path.Combine( Application.streamingAssetsPath, _gameFileName);

            string allText = File.ReadAllText(path);
            string[] lines = allText.Split("\n"[0]);

            _currentGame.piles = lines;
            LoadGame(lines);
            _currentGame.stats = _stats.GameData();

            
            if ( !_database.allGames.Contains(_currentGame) )
                _database.allGames.Add( _currentGame );
        }

        public void Restart()
        {
            SaveProgress();
            Reset();
            LoadGame( _currentGame.piles );
        }

        private void LoadGame(string[] lines)
        {
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

            if ( lines.Length >= 8 )
            {
                lineData = lines[7].Split('-');

                for (int j = 0; j < lineData.Length; j++)
                {
                    ReadCard( lineData[j], out suit, out rank);

                    _stock.OrderCard( suit, rank, j );
                }
            }

            foreach (BuildPile pile in _buildPiles)
                pile.CheckEmpty();

            _ai.HighlightSolvable();
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

        public void DeclareWin()
        {
            SaveProgress();

            if ( !_database.unlocked )
                Unlock();

            UpdateWinRate();
            StartCoroutine( ThrowCardsWin() );
        }

        public IEnumerator ThrowCardsWin()
        {
            yield return new WaitForSeconds(0.5f);
            
            _mainCamera.SetActive(false);
            _winCamera.SetActive(true);

            int foundation = 0;

            while ( foundation < 4 )
            {
                _foundations[foundation].StartWinThrow();
                foundation++;
                yield return new WaitForSeconds(0.25f);
            }

            yield return null;
        }

        private void Unlock()
        {
            _database.unlocked = true;
            _newGameButton.interactable = true;
            _solveButton.interactable = true;
        }

        public void FoundationAddedCard()
        {
            _cardsInFoundation++;

            if ( _cardsInFoundation == 52 )
                DeclareWin();
        }

        public void FoundationTakenCard()
        {
            _cardsInFoundation--;
        }

        public void CardOpened()
        {
            _gameProgress++;
            gameProgressText.text = _gameProgress + "/21";
        }

        private void SaveProgress()
        {
            if ( _gameProgress >= _currentGame.bestOpens )
            {
                _currentGame.bestOpens = _gameProgress;
                bestResultText.text = "Best " + _gameProgress + "/21";
                
                if ( _cardsInFoundation > _currentGame.bestFoundations )
                    _currentGame.bestFoundations = _cardsInFoundation;
            }
        }

        public void SaveJson()
        {
            string content;
            var folder = Application.streamingAssetsPath;
            var filePath = Path.Combine(folder, "database.json");

            if ( File.Exists(filePath))
                File.Delete(filePath);

            content = JsonUtility.ToJson(
                obj: _database,
                prettyPrint: true
            );
    
            File.WriteAllText(filePath, content);

        #if UNITY_EDITOR
            AssetDatabase.Refresh();
        #endif
        }

        public void LoadJson()
        {
            string path = Path.Combine( Application.streamingAssetsPath, "database.json");

            if ( !File.Exists(path) )
            {                
                _database = new GameDatabase();
                return;
            }

            string allText = File.ReadAllText(path);
            _database = new GameDatabase();
            JsonUtility.FromJsonOverwrite( allText, _database );

            _currentGame = _database.allGames[ _database.allGames.Count - 1 ];
            _gameSelector = _database.allGames.Count - 1;
            UpdateUI();

            if ( _database.unlocked )
                Unlock();
        }

        public void Exit()
        {
            SaveProgress();
            SaveJson();
            Application.Quit();
        }

        private void UpdateUI()
        {
            gameNameText.text = "Game: " + _currentGame.gameID + "/" + _database.allGames.Count;
            bestResultText.text = "Best " + _currentGame.bestOpens + "/21";
            UpdateWinRate();
        }

        private void UpdateWinRate()
        {
            _totalGames = _database.allGames.Count;
            _wonGames = 0;

            for (int i = 0; i < _database.allGames.Count; i++)
            {
                if ( _database.allGames[i].bestFoundations >= 52 )
                    _wonGames++;
            }

            _winRate = (float) _wonGames * 100f / (float) _totalGames;
            _winRate = Mathf.Round( _winRate );

            winRateText.text = "Win rate: " + _winRate + "%";
        }

        public void NextGame()
        {
            if ( _gameSelector < _database.allGames.Count - 1 )
            {
                SaveProgress();
                Reset();
                _gameSelector++;
                _currentGame = _database.allGames[ _gameSelector ];
                LoadGame( _currentGame.piles );
                UpdateUI();
            }
        }

        public void PreviousGame()
        {
            if ( _gameSelector > 0 )
            {
                SaveProgress();
                Reset();
                _gameSelector--;
                _currentGame = _database.allGames[ _gameSelector ];
                LoadGame( _currentGame.piles );
                UpdateUI();
            }
        }
    }
}
