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
            topLeftWall,
            topRightWall,
            bottomLeftWall,
            bottomRightWall,
            unVisitedCell;

        public bool IsVisited { get; private set; }


        public void Visited()
        {
            IsVisited = true;
            unVisitedCell.SetActive(false);
        }

        public void ClearLeftWall()
        {
            leftWall.SetActive(false);
        }

        public void ClearRightWall()
        {
            rightWall.SetActive(false);
        }

        public void ClearFrontWall()
        {
            frontWall.SetActive(false);
        }

        public void ClearBackWall()
        {
            backWall.SetActive(false);
        }

        public void ClearTopLeftWall()
        {
            topLeftWall.SetActive(false);
        }

        public void ClearTopRightWall()
        {
            topRightWall.SetActive(false);
        }

        public void ClearBottomLeftWall()
        {
            bottomLeftWall.SetActive(false);
        }

        public void ClearBottomRightWall()
        {
            bottomRightWall.SetActive(false);
        }
    }
}