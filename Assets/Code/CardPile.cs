using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class CardPile : MonoBehaviour
    {
        [SerializeField] protected PileType _type = PileType.NotSet;

        public PileType Type { get { return _type; } }

        [SerializeField] protected Settings _settings;
        [SerializeField] protected Card[] _pile;
        [SerializeField, Range(0,52)] protected int _numberOfCards;
        protected Vector3[] _positions;
        protected Quaternion _rotation;

        public virtual void TakeCard(Card card)
        {
        }
    }
}
