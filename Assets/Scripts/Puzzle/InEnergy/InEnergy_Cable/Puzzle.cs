
using DataSystem;
using Kuchinashi;
using Puzzle.Tutorial.P2;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D.IK;
using UnityEngine.UI;

namespace Puzzle.InEnergy.Cable
{
    public enum EnergyType
    {
        Red,
        Green,
        Yellow
    }

    public enum CableState
    {
        Up,
        Left,
        Down,
        Right
    }

    public class Puzzle : PuzzleBase
    {
        public static Puzzle Instance;
        private static string Path = "";
        private const int CAPACITY = 25;

        private List<CableState> CableStates;
        private List<Cable> Cables;

        private void Awake()
        {
            Instance = this;

            CableStates = new List<CableState>(CAPACITY);
            Cables = new List<Cable>(CAPACITY);

            Initialize();
        }

        private Coroutine CurrentCoroutine = null;
        private Button backButton;

        private void Initialize()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                int row = (i / 5) + 1;
                int column = (i % 5) + 1;
                string CableName = row + "," + column;
                Transform cable = transform.Find("Interactable/Cables/" + CableName);
                // if (cable != null) Debug.Log(cable.name);
                // else Debug.Log(CableName + " not found");
                Cables.Add(cable.GetComponent<Cable>());
                CableStates.Add(CableState.Up);
            }

            Refresh();
        }

        private void CheckState()
        {
            
        }

        private void Refresh()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                Cables[i].SetState(CableStates[i]);
            }
        }

        private void SaveData()
        {
            
        }

        public static void SetState(CableState cableState, Cable cable)
        {
            int id = Instance.Cables.IndexOf(cable);
            Instance.CableStates[id] = cableState;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (GameProgressData.GetPuzzleProgress(Id) == PuzzleProgress.Solved)
            {
                
            }
            else
            {
                if (GameProgressData.GetPuzzleProgress(Id) == PuzzleProgress.NotFound)
                {
                    
                }

                //CurrentCoroutine = StartCoroutine(CheckAnswerCoroutine());
            }

            backButton = transform.Find("Menu/Back").GetComponent<Button>();
            backButton.onClick.AddListener(() => {
                PuzzleManager.Exit();
            });

            List<string> ids = new List<string>();
            foreach (var c in GetComponentsInChildren<Character>())
            {
                ids.Add(c.data.Id);
            }
            UserDictionary.Unlock(ids);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (CurrentCoroutine != null)
            {
                StopCoroutine(CurrentCoroutine);
                CurrentCoroutine = null;
            }
        }
    }
}