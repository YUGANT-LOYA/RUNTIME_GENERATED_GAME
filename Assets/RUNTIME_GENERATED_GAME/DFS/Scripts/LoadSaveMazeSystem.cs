using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private MazeGenerator _mazeGenerator;
        public List<MazeCell> mazeCellList = new List<MazeCell>();
        public List<MazeDataStruct> mazeDataStructList;

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

        private void Awake()
        {
            _mazeGenerator = GetComponent<MazeGenerator>();
            saveMazeButton.onClick.AddListener(SaveIntoJson);
        }

        private void SaveIntoJson()
        {
            mazeCellList.Clear();
            mazeCellList = _mazeGenerator.GetMazeCellList();
            StringBuilder strBuilder = new StringBuilder();

            // Specify the file path
            string filePath = Path.Combine("Assets/RUNTIME_GENERATED_GAME/DFS/MazeJson",
                $"Maze_{_mazeGenerator.mazeNum}.json");

            //If File exists 
            if (File.Exists(filePath))
            {
                _mazeGenerator.mazeNum++;
                Debug.Log($"File Already Exists , so changing File Name to  Maze_{_mazeGenerator.mazeNum}.json");
                SaveIntoJson();
                return;
            }
            else
            {
                //Prints the gridSize [x,y]
                //Prints Source and Destination Location [S,D]
                //Prints all the cell internal Data Info [XY,WallInfo in terms of 0 and 1]

                strBuilder.AppendLine($"{_mazeGenerator.mazeWidth},{_mazeGenerator.mazeDepth}");

                //if(_mazeGenerator.isPlayerLocationAssigned)
                strBuilder.AppendLine();

                foreach (MazeCell cell in mazeCellList)
                {
                    MazeDataStruct mazeData = new MazeDataStruct
                    {
                        cellId = cell.mazeCellId
                    };

                    strBuilder.Append($"{cell.mazeCellId.x}{cell.mazeCellId.y},");
                    strBuilder.Append(cell.GetCellDataForJsonSaving());
                    mazeData.cellWallInfo = strBuilder.ToString();
                    mazeDataStructList.Add(mazeData);
                    strBuilder.AppendLine();
                }

                // Write the JSON data to the file
                File.WriteAllText(filePath, strBuilder.ToString());

#if UNITY_EDITOR
                // Refresh the asset database to make Unity aware of the new file
                UnityEditor.AssetDatabase.Refresh();
#endif
                Debug.Log("JSON file created at: " + filePath);
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