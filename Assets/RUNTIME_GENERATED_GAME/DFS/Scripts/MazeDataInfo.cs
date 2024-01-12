using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.MazeGenerator.DFS
{
    [CreateAssetMenu(fileName = "MazeInfo",menuName = "MazeDataInfo")]
    public class MazeDataInfo : ScriptableObject
    {
        [System.Serializable]
        public struct MazeInfoStruct
        {
            public Vector2Int mazeSize;
            public List<CellInfo> mazeCellInfoList;
            public Vector2Int playerCellId;
        }

        [System.Serializable]
        public struct CellInfo
        {
            public bool isLeftWallRemoved,
                isRightWallRemoved,
                isBackWallRemoved,
                isFrontWallRemoved,
                isPlayerPlacedOnThisCell;
        }

        public MazeInfoStruct mazeDataInfo;
    }
}
