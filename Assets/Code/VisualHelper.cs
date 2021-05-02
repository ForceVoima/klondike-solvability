using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Klondike
{
    public class VisualHelper : MonoBehaviour
    {

        public static VisualHelper _instance;
        public static VisualHelper Instance
        {
            get { return _instance; }
        }

        public TMPro.TextMeshProUGUI _below;
        public TMPro.TextMeshProUGUI _card;
        public TMPro.TextMeshProUGUI _above;

        private void Awake()
        {
            if ( _instance == null)
                _instance = this;
            else if (_instance != this)
            {
                Destroy(gameObject);
                Debug.LogWarning("Destroying dublicate statistics!");
                return;
            }

            _below.text = "";
            _card.text = "";
            _above.text = "";
        }

        public void AboveBelowCards(string below, string card, string above)
        {
            _below.text = "Below: " + below;
            _card.text = card;
            _above.text = "Above: " + above;
        }
    }
}
