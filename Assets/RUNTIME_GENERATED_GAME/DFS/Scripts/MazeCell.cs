using System.Text;
using UnityEngine;

namespace YugantLoyaLibrary.MazeGenerator.DFS
{
    public class MazeCell : MonoBehaviour
    {
        public Vector2Int mazeCellId;

        [SerializeField] private GameObject leftWall,
            rightWall,
            frontWall,
            backWall,
            unVisitedCell;

        public bool IsVisited { get; private set; }
        public bool isLeftWallPlaced = true, isRightWallPlaced = true, isFrontWallPlaced = true, isBackWallPlaced = true;

        public void Visited()
        {
            IsVisited = true;
            unVisitedCell.SetActive(false);
        }

        //Pattern in which the cell data is saved in Json.
        public string GetCellDataForJsonSaving()
        {
            // "CellID" : "{IsLeftWallThere}{IsRightWallThere}{IsFrontWallThere}{IsBackWallThere}" 
            StringBuilder jsonStr = new StringBuilder();

            jsonStr.Append(isLeftWallPlaced ? "1" : "0");
            jsonStr.Append(isRightWallPlaced ? "1" : "0");
            jsonStr.Append(isFrontWallPlaced ? "1" : "0");
            jsonStr.Append(isBackWallPlaced ? "1" : "0");

            Debug.Log($"Json Cell ID : {mazeCellId} , Data : {jsonStr} ");

            return jsonStr.ToString();
        }
        
        public void LeftWallStatus(bool isActive)
        {
            leftWall.SetActive(isActive);
            isLeftWallPlaced = isActive;
        }

        public void RightWallStatus(bool isActive)
        {
            rightWall.SetActive(isActive);
            isRightWallPlaced = isActive;
        }

        public void FrontWallStatus(bool isActive)
        {
            frontWall.SetActive(isActive);
            isFrontWallPlaced = isActive;
        }

        public void BackWallStatus(bool isActive)
        {
            backWall.SetActive(isActive);
            isBackWallPlaced = isActive;
        }
    }
}