using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField] private GameObject leftWall, rightWall, frontWall, backWall,unVisitedCell;
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
}