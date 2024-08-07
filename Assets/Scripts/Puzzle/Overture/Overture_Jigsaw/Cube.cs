using System.Collections;
using System.Collections.Generic;
using DataSystem;
using QFramework;
using UnityEngine;

namespace Puzzle.Overture.Jigsaw
{
    public enum CubeType
    {
        Empty,
        Normal,
        Hole
    }

    public class Cube : MonoBehaviour
    {
        public CubeType Type;
        public Vector2 CurrentPosition;

        public Collider2D col;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
        }

        private void OnMouseDown()
        {
            if (Type == CubeType.Empty) return;

            Puzzle.HoldingCube = this;
            col.enabled = false;
        }

        private void OnMouseOver()
        {
            if (Puzzle.HoldingCube != null && Puzzle.HoldingCube != this && Type == CubeType.Empty)
            {
                if (Vector2.Distance(Puzzle.HoldingCube.CurrentPosition, CurrentPosition) == 1)
                {
                    Swap(Puzzle.HoldingCube);
                    AudioKit.PlaySound("Cube-Slide");
                    if (Puzzle.CORRECT.TryGetValue(CurrentPosition, out var _id))
                    {
                        UserDictionary.Unlock(_id);
                    }
                }
            }
        }

        private void Swap(Cube _cube)
        {
            col.enabled = false;

            (_cube.transform.localPosition, transform.localPosition) = (transform.localPosition, _cube.transform.localPosition);
            (_cube.CurrentPosition, CurrentPosition) = (CurrentPosition, _cube.CurrentPosition);

            col.enabled = true;
        }

        private IEnumerator MoveToCoroutine(int _x)
        {
            while (!Mathf.Approximately(transform.localPosition.x, _x))
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(_x, 0, 0), 0.1f);
            }
            transform.localPosition = new Vector3(_x, 0, 0);

            yield return null;
        }

        private void OnMouseUp()
        {
            if (Type is CubeType.Normal or CubeType.Hole)
            {
                if (Puzzle.CORRECT.TryGetValue(CurrentPosition, out var _id))
                {
                    if (Type == CubeType.Normal)
                    {
                        Puzzle.Record[CurrentPosition] = false;
                    }
                    else
                    {
                        UserDictionary.AddRelatedPuzzleAndSave(_id, Puzzle.Instance.Id);
                        Puzzle.Record[CurrentPosition] = true;
                    }
                }
            }
            if (Type == CubeType.Empty) return;

            Puzzle.HoldingCube = null;
            col.enabled = true;
        }
    }
}