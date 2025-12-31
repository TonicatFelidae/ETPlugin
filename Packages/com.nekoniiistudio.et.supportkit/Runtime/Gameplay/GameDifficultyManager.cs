using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ET
{
    public class GameDifficultyManager
    {
        public UnityEvent onDifficultyChange = new();
        public GameDifficulty CurrentDifficulty
        {
            get => _currentDifficulty;
            set
            {
                if (value != _currentDifficulty) 
                { 
                    _currentDifficulty = value;
                    onDifficultyChange?.Invoke();
                }
            }
        }
        private GameDifficulty _currentDifficulty;

        public void ChangeDifficulty(GameDifficulty gameDifficulty)
        {
            _currentDifficulty = gameDifficulty;
        }
    }

    public enum GameDifficulty
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        VeryHard,
        Extreme,
        Impossible
    }
}
