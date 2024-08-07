using System.Collections;
using System.Collections.Generic;
using Cameras;
using DataSystem;
using QFramework;
using UnityEngine;

namespace Puzzle.InEnergy.WeighBeaker
{
    public class Tank : MonoBehaviour
    {
        public Collider2D col;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
        }

        private void OnMouseOver()
        {
            if (Puzzle.Instance.HoldingBottle == null) return;
            Puzzle.Instance.Target = InteractTarget.Tank;
        }

        private void OnMouseExit()
        {
            if (Puzzle.Instance.HoldingBottle == null) return;
            Puzzle.Instance.Target = InteractTarget.None;
        }
    }
}