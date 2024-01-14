using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace YugantLoyaLibrary.MazeGenerator.DFS
{
    public class MazeGenerator : MonoBehaviour
    {
        public enum MazeShape
        {
            Square
        }

        [Serializable]
        public struct ShapeStruct
        {
            public MazeShape shape;
            public MazeCell shapeScript;
            public GameObject shapeObj;
        }

        public bool isMazeCompleted;
        public int mazeNum = 1;
        public bool boolLoadingMazeFile = false;
        [ShowIf("boolLoadingMazeFile")] public TextAsset mazeJsonFile;
        public List<ShapeStruct> shapeStructList;
        [SerializeField] private Transform mazeCellContainer;
        [SerializeField] private MazeShape currMazeShape;
        [SerializeField] private float camYOffset = 2f, timeToFindNextCell = 0.02f;
        public int mazeWidth = 25, mazeDepth = 25;
        public bool shouldStartRandomly = true;

        [HideIf("shouldStartRandomly")] [SerializeField]
        Vector2Int startingMazeCell = Vector2Int.zero;

        private Camera _cam;
        private MazeCell[,] _mazeGrid;
        public bool isPlayerLocationAssigned, isTargetLocationAssigned;
        public Vector2Int playerLocation, targetLocation;
        public List<MazeCell> totalMazeCellList;

        private void OnEnable()
        {
            GameEventHandler.OnGridCreationCompleteEvent += GridCreationCompleted;
            GameEventHandler.OnMazeCreationStartEvent += MazeCreationStarted;
            GameEventHandler.OnMazeCreationCompleteEvent += MazeCreationCompleted;
        }


        private void OnDisable()
        {
            GameEventHandler.OnGridCreationCompleteEvent -= GridCreationCompleted;
            GameEventHandler.OnMazeCreationStartEvent -= MazeCreationStarted;
            GameEventHandler.OnMazeCreationCompleteEvent -= MazeCreationCompleted;
        }

        private void Awake()
        {
            _cam = Camera.main;
        }

        private void Start()
        {
            totalMazeCellList = new List<MazeCell>();
            //Only runs when we need to create new maze generation
            if (!boolLoadingMazeFile)
            {
                StartCoroutine(StartGeneratingMaze());
            }
            else
            {
                //This will load the file of mazeJsonFile and create maze using it.

                if (mazeJsonFile != null)
                {
                    LoadMazeDataUsingJson();
                }
                else
                {
                    Debug.LogError("There is No File attached to mazeJsonFile to already created maze !");
                }
            }
        }

        private void MazeCreationStarted()
        {
            isMazeCompleted = false;
        }

        public Transform GetMazeCellContainer()
        {
            return mazeCellContainer;
        }

        public List<MazeCell> GetMazeCellList()
        {
            return new List<MazeCell>(totalMazeCellList);
        }

        //Access the main Prefab
        private MazeCell GetShapeScriptFromMazeShape(MazeShape shapeEnum)
        {
            foreach (ShapeStruct shapeStruct in shapeStructList)
            {
                if (shapeStruct.shape == shapeEnum)
                {
                    return shapeStruct.shapeScript;
                }
            }

            return null;
        }

        [Button]
        public void RecreateMaze()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Only Available in Play Mode !!");
                return;
            }

            StopAllCoroutines();
            _mazeGrid = new MazeCell[0, 0];
            totalMazeCellList.Clear();

            for (int i = mazeCellContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(mazeCellContainer.GetChild(i).gameObject);
            }

            StartCoroutine(StartGeneratingMaze());
        }

        IEnumerator StartGeneratingMaze()
        {
            CreateGrid();

            GameEventHandler.MazeCreationStarted();

            MazeCell prevCell = null;

            if (!shouldStartRandomly)
            {
                if (startingMazeCell.x - 1 >= 0 && startingMazeCell.x >= 0 && startingMazeCell.x < mazeWidth &&
                    startingMazeCell.y >= 0 && startingMazeCell.y < mazeDepth)
                {
                    prevCell = _mazeGrid[startingMazeCell.x - 1, startingMazeCell.y];
                }
            }
            else
            {
                int randomX = UnityEngine.Random.Range(0, mazeWidth);
                int randomY = UnityEngine.Random.Range(0, mazeDepth);

                startingMazeCell = new Vector2Int(randomX, randomY);

                if (startingMazeCell.x - 1 >= 0)
                {
                    prevCell = _mazeGrid[startingMazeCell.x - 1, startingMazeCell.y];
                }
            }

            yield return GenerateMaze(prevCell, _mazeGrid[startingMazeCell.x, startingMazeCell.y]);
        }

        private void CreateGrid()
        {
            _mazeGrid = new MazeCell[mazeWidth, mazeDepth];
            
            MazeCell currMazeScript = GetShapeScriptFromMazeShape(currMazeShape);

            for (int x = 0; x < mazeWidth; x++)
            {
                for (int z = 0; z < mazeDepth; z++)
                {
                    Vector3 localScale = currMazeScript.transform.localScale;
                    _mazeGrid[x, z] = Instantiate(currMazeScript, new Vector3(localScale.x * x, 0, localScale.z * z),
                        Quaternion.identity,
                        mazeCellContainer);
                    _mazeGrid[x,z].transform.SetAsLastSibling();
                    _mazeGrid[x, z].mazeCellId = new Vector2Int(x, z);
                    totalMazeCellList.Add(_mazeGrid[x, z]);
                    currMazeScript.gameObject.name = $"Cell_{x}_{z}";
                }
            }

            GameEventHandler.GridCreationCompleted();
        }

        private void MazeCreationCompleted()
        {
            isMazeCompleted = true;
        }

        bool IsAllCellVisited()
        {
            foreach (MazeCell cell in totalMazeCellList)
            {
                if (!cell.IsVisited)
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerator GenerateMaze(MazeCell prevCell, MazeCell currCell)
        {
            currCell.Visited();
            ClearWalls(prevCell, currCell);

            yield return new WaitForSeconds(timeToFindNextCell);

            MazeCell nextCell;

            do
            {
                nextCell = GetNextUnVisitedMazeCell(currCell);

                if (nextCell != null)
                {
                    yield return GenerateMaze(currCell, nextCell);
                }
            } while (nextCell != null);


            if (IsAllCellVisited() && !isMazeCompleted)
            {
                //Calls When The Whole Maze is Ready
                Debug.Log("Maze Creation Completed !");
                GameEventHandler.MazeCreationCompleted();
            }
        }

        void ClearWalls(MazeCell prevCell, MazeCell currCell)
        {
            if (prevCell == null)
                return;

            //Moving From Left to Right
            if (prevCell.mazeCellId.x < currCell.mazeCellId.x)
            {
                prevCell.RightWallStatus(false);
                currCell.LeftWallStatus(false);
                return;
            }

            //Moving From Right to Left
            if (prevCell.mazeCellId.x > currCell.mazeCellId.x)
            {
                prevCell.LeftWallStatus(false);
                currCell.RightWallStatus(false);
                return;
            }

            //Moving From Front to Back
            if (prevCell.mazeCellId.y > currCell.mazeCellId.y)
            {
                prevCell.BackWallStatus(false);
                currCell.FrontWallStatus(false);
                return;
            }

            //Moving From Back to Front
            if (prevCell.mazeCellId.y < currCell.mazeCellId.y)
            {
                prevCell.FrontWallStatus(false);
                currCell.BackWallStatus(false);
                return;
            }
        }

        MazeCell GetNextUnVisitedMazeCell(MazeCell currCell)
        {
            IEnumerable<MazeCell> unvisitedCellList = GetUnvisitedCell(currCell);

            return unvisitedCellList.OrderBy(c => UnityEngine.Random.Range(1, 10)).FirstOrDefault();
        }

        IEnumerable<MazeCell> GetUnvisitedCell(MazeCell currCell)
        {
            int x = currCell.mazeCellId.x;
            int z = currCell.mazeCellId.y;

            //Check whether the right cell is visited or not.
            if (x + 1 < mazeWidth)
            {
                MazeCell rightCell = _mazeGrid[x + 1, z];

                if (!rightCell.IsVisited)
                {
                    yield return rightCell;
                }
            }

            //Check whether the Left cell is visited or not.
            if (x - 1 >= 0)
            {
                MazeCell leftCell = _mazeGrid[x - 1, z];

                if (!leftCell.IsVisited)
                {
                    yield return leftCell;
                }
            }

            //Check whether the Front cell is visited or not.
            if (z + 1 < mazeDepth)
            {
                MazeCell frontCell = _mazeGrid[x, z + 1];

                if (!frontCell.IsVisited)
                {
                    yield return frontCell;
                }
            }

            //Check whether the back cell is visited or not.
            if (z - 1 >= 0)
            {
                MazeCell backCell = _mazeGrid[x, z - 1];

                if (!backCell.IsVisited)
                {
                    yield return backCell;
                }
            }
        }

        void GridCreationCompleted()
        {
            Vector3 camPos = Vector3.zero;

            foreach (Transform trans in mazeCellContainer)
            {
                camPos += trans.position;
            }

            int max = mazeDepth > mazeWidth ? mazeDepth : mazeWidth;

            int childCount = mazeCellContainer.childCount;
            
            Camera.main.transform.position =
                new Vector3(camPos.x / childCount, max * 2 + camYOffset, camPos.z / childCount);
        }

        private void LoadMazeDataUsingJson()
        {
            TextAsset mazeFile = mazeJsonFile;
            string[] data = mazeFile.text.Split('\n');
            
            //First Line is of Grid Size
            string[] gridSizeLine = data[0].Split(',');
            mazeWidth = int.Parse(gridSizeLine[0]);
            mazeDepth = int.Parse(gridSizeLine[1]);
            
            //Second Line is for reading the Source and Destination of Player.
            
            
            //Creating Grids
            CreateGrid();

            int dataIndex = 2;
            
            for (int i = 0; i < totalMazeCellList.Count; i++)
            {
                MazeCell cell = totalMazeCellList[i];
                
                //This is the data of each cell of maze about their wall and other data.
                string[] cellDataStr = data[dataIndex].Split(',');

                //Vector2Int cellId = new Vector2Int(cellDataStr[0][0], cellDataStr[0][1]);
                
                //cellDataStr[1] is the data of walls
                string wallDetailStr = cellDataStr[1];
                Debug.Log("Wall Detail : " + wallDetailStr);
                cell.LeftWallStatus(int.Parse(wallDetailStr[0].ToString()) == 1);
                cell.RightWallStatus(int.Parse(wallDetailStr[1].ToString()) == 1);
                cell.FrontWallStatus(int.Parse(wallDetailStr[2].ToString()) == 1);
                cell.BackWallStatus(int.Parse(wallDetailStr[3].ToString()) == 1);
                cell.Visited();
                dataIndex++;
            }
            
            
            
            // for (int i = 0; i < data.Length; i++)
            // {
            //     string line = data[i];
            //
            //     if (i == 0)
            //     {
            //         //First Line is of Grid Size
            //         mazeWidth = int.Parse(line[0].ToString());
            //         mazeDepth = int.Parse(line[1].ToString());
            //     }
            //     else if (i == 1)
            //     {
            //         //Second Line is for reading the Source and Destination of Player.
            //         
            //     }
            //     else
            //     {
            //         //This is the data of each cell of maze about their wall and other data.
            //         string[] cellDataStr = line.Split(',');
            //
            //         Vector2Int cellId = new Vector2Int(cellDataStr[0][0], cellDataStr[0][1]);
            //         for (var index = 0; index < cellDataStr.Length; index++)
            //         {
            //             var str = cellDataStr[index];
            //         }
            //     }
            // }
        }
    }
}