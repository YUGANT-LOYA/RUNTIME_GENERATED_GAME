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
            Square,
            Octagon
        }

        [Serializable]
        public struct ShapeStruct
        {
            public MazeShape shape;
            public MazeCell shapeScript;
            public GameObject shapeObj;
        }

        public event Action OnCompleteMazeEvent, OnGridCreationComplete;

        public List<ShapeStruct> shapeStructList;
        [SerializeField] private Transform mazeCellContainer;
        [SerializeField] private MazeShape currMazeShape;
        [SerializeField] private float camYOffset = 3f, timeToFindNextCell = 0.02f;
        [SerializeField] private int mazeWidth = 25, mazeDepth = 25;
        public bool shouldStartRandomly = true;
        [SerializeField] Vector2Int startingMazeCell = Vector2Int.zero;
        private MazeCell[,] _mazeGrid;

        private void OnEnable()
        {
            OnCompleteMazeEvent += MazeCompleted;
            OnGridCreationComplete += GridCreationCompleted;
        }

        private void OnDisable()
        {
            OnCompleteMazeEvent -= MazeCompleted;
            OnGridCreationComplete -= GridCreationCompleted;
        }

        private void Start()
        {
            StartCoroutine(StartGeneratingMaze());
        }

        //Access the main Prefab
        public MazeCell GetShapeScriptFromMazeShape(MazeShape shapeEnum)
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

            for (int i = mazeCellContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(mazeCellContainer.GetChild(i).gameObject);
            }

            StartCoroutine(StartGeneratingMaze());
        }

        IEnumerator StartGeneratingMaze()
        {
            _mazeGrid = new MazeCell[mazeWidth, mazeDepth];

            CreateGrid();

            OnGridCreationComplete?.Invoke();

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
            MazeCell currMazeScript = GetShapeScriptFromMazeShape(currMazeShape);

            for (int x = 0; x < mazeWidth; x++)
            {
                for (int z = 0; z < mazeDepth; z++)
                {
                    Vector3 localScale = currMazeScript.transform.localScale;
                    _mazeGrid[x, z] = Instantiate(currMazeScript, new Vector3(localScale.x * x, 0, localScale.z * z),
                        Quaternion.identity,
                        mazeCellContainer);
                    _mazeGrid[x, z].mazeCellId = new Vector2Int(x, z);
                }
            }
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


            //Calls When The Whole Maze is Ready
            OnCompleteMazeEvent?.Invoke();
        }

        void ClearWalls(MazeCell prevCell, MazeCell currCell)
        {
            if (prevCell == null)
                return;

            //Moving From Left to Right
            if (prevCell.mazeCellId.x < currCell.mazeCellId.x)
            {
                prevCell.ClearRightWall();
                currCell.ClearLeftWall();
                return;
            }

            //Moving From Right to Left
            if (prevCell.mazeCellId.x > currCell.mazeCellId.x)
            {
                prevCell.ClearLeftWall();
                currCell.ClearRightWall();
                return;
            }

            //Moving From Front to Back
            if (prevCell.mazeCellId.y > currCell.mazeCellId.y)
            {
                prevCell.ClearBackWall();
                currCell.ClearFrontWall();
                return;
            }

            //Moving From Back to Front
            if (prevCell.mazeCellId.y < currCell.mazeCellId.y)
            {
                prevCell.ClearFrontWall();
                currCell.ClearBackWall();
                return;
            }

            //Moving From TopLeft to TopRight
            // if (prevCell.mazeCellId.y < currCell.mazeCellId.y)
            // {
            //     prevCell.ClearFrontWall();
            //     currCell.ClearBackWall();
            //     return;
            // }
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

            var childCount = mazeCellContainer.childCount;
            Camera.main.transform.position =
                new Vector3(camPos.x / childCount, max + camYOffset, camPos.z / childCount);
        }

        void MazeCompleted()
        {
        }
    }
}