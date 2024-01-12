using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YugantLoyaLibrary.MazeGenerator.DFS
{
    public static class GameEventHandler
    {
        public static event Action OnMazeCreationStartEvent, OnMazeCreationCompleteEvent, OnGridCreationCompleteEvent;

        public static void MazeCreationStarted() => OnMazeCreationStartEvent?.Invoke();
        public static void MazeCreationCompleted() => OnMazeCreationCompleteEvent?.Invoke();
        public static void GridCreationCompleted() => OnGridCreationCompleteEvent?.Invoke();
    }
}