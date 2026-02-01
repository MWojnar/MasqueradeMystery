using System.Collections.Generic;
using UnityEngine;

namespace MasqueradeMystery
{
    public class CharacterSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Character characterPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private int characterCount = 20;
        [SerializeField, Range(0f, 0.5f)] private float dancingPairPercentage = 0.3f;
        [SerializeField] private Rect spawnArea = new Rect(-10, -5, 20, 10);
        [SerializeField] private float minDistanceBetweenCharacters = 1.5f;
        [SerializeField] private float dancePartnerDistance = 1.0f;

        [Header("Player Settings")]
        [SerializeField] private bool spawnPlayer = true;

        private List<Character> spawnedCharacters = new List<Character>();
        private Character playerCharacter;

        public Character PlayerCharacter => playerCharacter;
        private List<Vector2> usedPositions = new List<Vector2>();

        public List<Character> SpawnedCharacters => spawnedCharacters;

        public List<Character> SpawnCharacters()
        {
            ClearExistingCharacters();

            // Generate all character data
            List<CharacterData> allData = GenerateCharacterData();

            // Spawn all characters
            foreach (var data in allData)
            {
                Character character = Instantiate(characterPrefab, transform);
                character.Initialize(data);
                spawnedCharacters.Add(character);
            }

            // Resolve dance partner references
            ResolveDancePartners();

            // Spawn player character
            if (spawnPlayer)
            {
                SpawnPlayerCharacter(allData.Count);
            }

            Debug.Log($"Spawned {spawnedCharacters.Count} characters");
            return spawnedCharacters;
        }

        private void SpawnPlayerCharacter(int nextId)
        {
            CharacterData playerData = GenerateRandomCharacter(nextId);
            playerData.IsPlayer = true;
            playerData.DanceState = DanceState.NotDancing;
            playerData.Position = Vector2.zero; // Center of scene

            Character player = Instantiate(characterPrefab, transform);
            player.Initialize(playerData);
            player.gameObject.AddComponent<PlayerController>();

            // Remove hoverable from player
            var hoverable = player.GetComponent<CharacterHoverable>();
            if (hoverable != null) Destroy(hoverable);

            // Enable permanent white outline for player
            var visuals = player.GetComponent<CharacterVisuals>();
            if (visuals != null)
            {
                visuals.SetOutlineColor(Color.white);
                visuals.SetOutline(true);
            }

            spawnedCharacters.Add(player);
            playerCharacter = player;
        }

        public void ClearExistingCharacters()
        {
            foreach (var character in spawnedCharacters)
            {
                if (character != null)
                {
                    Destroy(character.gameObject);
                }
            }
            spawnedCharacters.Clear();
            usedPositions.Clear();
            playerCharacter = null;
        }

        private List<CharacterData> GenerateCharacterData()
        {
            List<CharacterData> dataList = new List<CharacterData>();
            int id = 0;

            // Calculate how many dancing pairs
            int pairCount = Mathf.FloorToInt(characterCount * dancingPairPercentage / 2);
            int soloCount = characterCount - (pairCount * 2);

            // Generate dancing pairs
            for (int i = 0; i < pairCount; i++)
            {
                Vector2 pairPosition = GetValidPosition();

                // Generate two characters that dance together
                CharacterData leader = GenerateRandomCharacter(id++);
                CharacterData follower = GenerateRandomCharacter(id++);

                // Set up dance relationship based on partner's clothing
                leader.DancePartnerId = follower.CharacterId;
                follower.DancePartnerId = leader.CharacterId;

                leader.DanceState = follower.Clothing == ClothingType.Suit
                    ? DanceState.DancingWithSuitPartner
                    : DanceState.DancingWithDressPartner;
                follower.DanceState = leader.Clothing == ClothingType.Suit
                    ? DanceState.DancingWithSuitPartner
                    : DanceState.DancingWithDressPartner;

                // Position near each other
                leader.Position = pairPosition + Vector2.left * dancePartnerDistance * 0.5f;
                follower.Position = pairPosition + Vector2.right * dancePartnerDistance * 0.5f;

                // Mark both positions as used
                usedPositions.Add(leader.Position);
                usedPositions.Add(follower.Position);

                dataList.Add(leader);
                dataList.Add(follower);
            }

            // Generate solo characters
            for (int i = 0; i < soloCount; i++)
            {
                CharacterData solo = GenerateRandomCharacter(id++);
                solo.DanceState = DanceState.NotDancing;
                solo.Position = GetValidPosition();
                usedPositions.Add(solo.Position);
                dataList.Add(solo);
            }

            return dataList;
        }

        private CharacterData GenerateRandomCharacter(int id)
        {
            CharacterData data = new CharacterData { CharacterId = id };

            // Random mask (50/50 animal vs non-animal)
            data.Mask.IsAnimalMask = Random.value > 0.5f;
            if (data.Mask.IsAnimalMask)
            {
                data.Mask.AnimalMask = (AnimalMaskType)Random.Range(1, 5); // Skip None
            }
            else
            {
                data.Mask.NonAnimalMask = (NonAnimalMaskType)Random.Range(1, 5); // Skip None
            }

            // Random clothing
            data.Clothing = Random.value > 0.5f ? ClothingType.Suit : ClothingType.Dress;

            // Random accessory (50% chance, matching clothing type)
            if (Random.value > 0.5f)
            {
                data.Accessories = data.Clothing == ClothingType.Suit
                    ? Accessories.Bowtie
                    : Accessories.Hairbow;
            }
            else
            {
                data.Accessories = Accessories.None;
            }

            return data;
        }

        private Vector2 GetValidPosition()
        {
            // Use CharacterBounds if available, otherwise fall back to spawnArea
            Rect area = SceneBounds.Instance != null
                ? SceneBounds.Instance.CharacterBounds
                : spawnArea;

            int maxAttempts = 100;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 position = new Vector2(
                    Random.Range(area.xMin, area.xMax),
                    Random.Range(area.yMin, area.yMax)
                );

                bool valid = true;
                foreach (var used in usedPositions)
                {
                    if (Vector2.Distance(position, used) < minDistanceBetweenCharacters)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    return position;
                }
            }

            // Fallback: return random position even if too close
            Debug.LogWarning("Could not find valid position after max attempts");
            return new Vector2(
                Random.Range(area.xMin, area.xMax),
                Random.Range(area.yMin, area.yMax)
            );
        }

        private void ResolveDancePartners()
        {
            foreach (var character in spawnedCharacters)
            {
                if (character.Data.DancePartnerId >= 0 && character.DancePartner == null)
                {
                    Character partner = spawnedCharacters.Find(c => c.Data.CharacterId == character.Data.DancePartnerId);
                    if (partner != null)
                    {
                        character.SetDancePartner(partner);
                    }
                }
            }
        }

        // Get all character data (for hint system)
        public List<CharacterData> GetAllCharacterData()
        {
            List<CharacterData> data = new List<CharacterData>();
            foreach (var character in spawnedCharacters)
            {
                data.Add(character.Data);
            }
            return data;
        }

        // Visualize spawn area in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(
                spawnArea.x + spawnArea.width / 2f,
                spawnArea.y + spawnArea.height / 2f,
                0
            );
            Vector3 size = new Vector3(spawnArea.width, spawnArea.height, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
