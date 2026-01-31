using System;
using System.Collections.Generic;

namespace MasqueradeMystery
{
    public static class GameEvents
    {
        // Character interactions
        public static Action<Character> OnCharacterHoverStart;
        public static Action<Character> OnCharacterHoverEnd;
        public static Action<Character> OnCharacterClicked;

        // Game state
        public static Action<GameState> OnGameStateChanged;
        public static Action<List<Hint>> OnHintsGenerated;
        public static Action OnTargetFound;
        public static Action OnWrongGuess;

        // Clear all listeners (useful for cleanup)
        public static void ClearAll()
        {
            OnCharacterHoverStart = null;
            OnCharacterHoverEnd = null;
            OnCharacterClicked = null;
            OnGameStateChanged = null;
            OnHintsGenerated = null;
            OnTargetFound = null;
            OnWrongGuess = null;
        }
    }
}
