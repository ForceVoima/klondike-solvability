using UnityEngine;
using UE = UnityEditor;

namespace Klondike
{
    [UE.CustomEditor(typeof(Stock))]
    public class StockInspector : UE.Editor
    {
        private Stock _target;

        protected void OnEnable()
        {
            _target = target as Stock;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Organize"))
            {
                _target.Init();
            }
        }
    }
}