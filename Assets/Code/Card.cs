using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{

    public class Card : MonoBehaviour
    {
        [SerializeField] private Suit _suit = Suit.NotSet;
        public Suit Suit
        {
            get { return _suit; }
            set { _suit = value; }
        }

        [SerializeField, Range(0,13)] int _rank = 0;
        public int Rank
        {
            get { return _rank; }
            set { _rank = value; }
        }

        private Vector3 _endPos;
        private Quaternion _endRot;

        public void MoveTo(Vector3 position, Quaternion rotation, bool instant)
        {
            if (instant)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
        }
    }
}
