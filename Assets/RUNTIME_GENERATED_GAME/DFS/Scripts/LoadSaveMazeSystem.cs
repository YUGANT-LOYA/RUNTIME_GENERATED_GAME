using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YugantLoyaLibrary.MazeGenerator.DFS
{
    [RequireComponent(typeof(MazeGenerator))]
    public class LoadSaveMazeSystem : MonoBehaviour
    {
        [Serializable]
        public struct MazeDataStruct
        {
            public Vector2Int cellId;
            public string cellWallInfo;
        }
        
        public Button saveMazeButton;
        public List<MazeCell> mazeCellList = new List<MazeCell>();
        public MazeDataStruct mazeDataStruct;

        private void OnEnable()
        {
            GameEventHandler.OnMazeCreationStartEvent += DeactivateSaveSystem;
            GameEventHandler.OnMazeCreationCompleteEvent += ActivateSaveSystem;
        }

        private void OnDisable()
        {
            GameEventHandler.OnMazeCreationStartEvent -= DeactivateSaveSystem;
            GameEventHandler.OnMazeCreationCompleteEvent -= ActivateSaveSystem;
        }

        public void SaveIntoJson()
        {
            mazeCellList.Clear();
            mazeCellList = GetComponent<MazeGenerator>().GetMazeCellList();
            StringBuilder strBuilder = new StringBuilder();
            foreach (MazeCell cell in mazeCellList)
            {
                strBuilder.Clear();
                MazeDataStruct mazeData = new MazeDataStruct();
                mazeData.cellId = cell.mazeCellId;
                strBuilder.Append(cell.GetCellDataForJsonSaving());
                mazeData.cellWallInfo = strBuilder.ToString();
            }
        }

        void ActivateSaveSystem()
        {
            saveMazeButton.interactable = true;
        }
        
        void DeactivateSaveSystem()
        {
            saveMazeButton.interactable = false;
        }
    }
}