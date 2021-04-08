using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Klondike
{
    public class PlayerPile : CardPile
    {
        public void OnMouseEnter()
        {
            Player.Instance.Entered( _type, this );
        }

        public void OnMouseExit()
        {
            Player.Instance.Exited( _type, this );
        }

        public virtual bool AcceptsCard(Card card)
        {
            Debug.Log("AcceptsCard not implemented in " + name);
            return false;
        }

        public virtual void DealSequenceOfCards(CardPile pile)
        {   
        }
    }
}
