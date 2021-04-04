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
        }
    }
}