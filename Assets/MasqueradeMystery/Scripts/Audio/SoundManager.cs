using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace MasqueradeMystery
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Gameplay Sounds")]
        public EventReference VictoryJingle;
        public EventReference FailureJingle;
        public EventReference WrongGuessSuit;
        public EventReference WrongGuessDress;
        public EventReference HintsAppearing;
        public EventReference ScoreTally;
        public EventReference AccusationSound;

        [Header("UI Sounds")]
        public EventReference HoverEnter;
        public EventReference HoverExit;

        [Header("Ambient")]
        public EventReference BallroomAmbient;

        private EventInstance ballroomAmbientInstance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to game events
            GameEvents.OnTargetFound += OnTargetFound;
            GameEvents.OnWrongGuess += OnWrongGuess;
            GameEvents.OnHintsGenerated += OnHintsGenerated;
            GameEvents.OnCharacterHoverStart += OnCharacterHoverStart;
            GameEvents.OnCharacterHoverEnd += OnCharacterHoverEnd;
            GameEvents.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            GameEvents.OnTargetFound -= OnTargetFound;
            GameEvents.OnWrongGuess -= OnWrongGuess;
            GameEvents.OnHintsGenerated -= OnHintsGenerated;
            GameEvents.OnCharacterHoverStart -= OnCharacterHoverStart;
            GameEvents.OnCharacterHoverEnd -= OnCharacterHoverEnd;
            GameEvents.OnGameStateChanged -= OnGameStateChanged;

            // Clean up ambient instance
            StopBallroomAmbient();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        // Event handlers
        private void OnTargetFound()
        {
            PlayOneShot(VictoryJingle);
        }

        private void OnWrongGuess(Character character)
        {
            if (character?.Data == null) return;

            if (character.Data.Clothing == ClothingType.Dress)
            {
                PlayOneShot(WrongGuessDress);
            }
            else
            {
                PlayOneShot(WrongGuessSuit);
            }
        }

        private void OnHintsGenerated(List<Hint> hints)
        {
            PlayOneShot(HintsAppearing);
        }

        private void OnCharacterHoverStart(Character character)
        {
            if (character != null)
            {
                PlayOneShot(HoverEnter, character.transform.position);
            }
        }

        private void OnCharacterHoverEnd(Character character)
        {
            if (character != null)
            {
                PlayOneShot(HoverExit, character.transform.position);
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                StartBallroomAmbient();
            }
            else if (state == GameState.RoundEnding || state == GameState.Title ||
                     state == GameState.Won || state == GameState.Lost)
            {
                StopBallroomAmbient();
            }
        }

        // Direct call methods for sounds needing precise timing
        public void PlayFailureJingle()
        {
            PlayOneShot(FailureJingle);
        }

        public void PlayScoreTally()
        {
            PlayOneShot(ScoreTally);
        }

        public void PlayAccusation(Vector3 position)
        {
            PlayOneShot(AccusationSound, position);
        }

        // Ambient sound management
        private void StartBallroomAmbient()
        {
            if (BallroomAmbient.IsNull) return;

            if (!ballroomAmbientInstance.isValid())
            {
                ballroomAmbientInstance = RuntimeManager.CreateInstance(BallroomAmbient);
                ballroomAmbientInstance.start();
            }
        }

        private void StopBallroomAmbient()
        {
            if (ballroomAmbientInstance.isValid())
            {
                ballroomAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                ballroomAmbientInstance.release();
            }
        }

        // Helper for one-shot sounds
        private void PlayOneShot(EventReference sound, Vector3? position = null)
        {
            if (sound.IsNull) return;

            if (position.HasValue)
            {
                RuntimeManager.PlayOneShot(sound, position.Value);
            }
            else
            {
                RuntimeManager.PlayOneShot(sound);
            }
        }
    }
}
