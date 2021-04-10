using UnityEngine;
using UE = UnityEditor;

namespace Klondike
{
    [UE.CustomEditor(typeof(GameMaster))]
    public class GameMasterInspector : UE.Editor
    {
        private GameMaster _master;

        protected void OnEnable()
        {
            _master = target as GameMaster;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("NewGame"))
            {
                _master.NewGame();
            }

            if (GUILayout.Button("Load custom game"))
            {
                _master.LoadCustomGame();
            }

            if (GUILayout.Button("Restart"))
            {
                _master.Restart();
            }

            if (GUILayout.Button("Save JSON"))
            {
                _master.SaveJson();
            }
        }
    }
}