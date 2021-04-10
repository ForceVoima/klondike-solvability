using UnityEngine;
using UE = UnityEditor;

namespace Klondike
{
    [UE.CustomEditor(typeof(AIMaster))]
    public class AIMasterInspector : UE.Editor
    {
        private AIMaster _master;

        protected void OnEnable()
        {
            _master = target as AIMaster;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Solve all"))
            {
                _master.SolveAll();
            }
        }
    }
}