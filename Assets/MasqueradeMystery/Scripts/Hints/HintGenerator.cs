using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasqueradeMystery
{
    public class HintGenerator
    {
        private List<CharacterData> allCharacters;
        private List<Character> characterObjects;

        // Result of generation
        public List<Hint> GeneratedHints { get; private set; }
        public CharacterData TargetCharacter { get; private set; }

        public HintGenerator(List<Character> characters)
        {
            characterObjects = characters;
            allCharacters = characters.Select(c => c.Data).ToList();
        }

        /// <summary>
        /// Generates hints first, then finds or creates exactly one matching character.
        /// Returns the target character.
        /// </summary>
        public CharacterData GenerateHintsAndFindTarget(int hintCount)
        {
            // Step 1: Generate random non-contradictory hints
            GeneratedHints = GenerateRandomHints(hintCount);

            // Step 2: Find all characters matching these hints (excluding player)
            var matchingCharacters = HintEvaluator.GetMatchingCharacters(allCharacters, GeneratedHints)
                .Where(c => !c.IsPlayer).ToList();

            if (matchingCharacters.Count == 0)
            {
                // No one matches - create a character that matches and replace a random one
                TargetCharacter = CreateMatchingCharacter();
                ReplaceRandomCharacter(TargetCharacter);
            }
            else if (matchingCharacters.Count == 1)
            {
                // Exactly one matches - perfect!
                TargetCharacter = matchingCharacters[0];
            }
            else
            {
                // Multiple match - pick one as target, randomize the others
                TargetCharacter = matchingCharacters[Random.Range(0, matchingCharacters.Count)];

                foreach (var character in matchingCharacters)
                {
                    if (character != TargetCharacter)
                    {
                        RandomizeUntilNoMatch(character);
                    }
                }
            }

            return TargetCharacter;
        }

        private List<Hint> GenerateRandomHints(int count)
        {
            List<Hint> hints = new List<Hint>();

            // Category 1: Mask subcategory (animal or human traits)
            bool isAnimalMask = Random.value > 0.5f;
            if (isAnimalMask)
            {
                // Pick one animal trait
                HintType[] animalTraits = { HintType.MaskIsMammal, HintType.MaskIsPredator, HintType.MaskIsAquatic, HintType.MaskIsPrey };
                hints.Add(new Hint(animalTraits[Random.Range(0, animalTraits.Length)]));
            }
            else
            {
                // Pick one or two human mask traits
                bool hasHat = Random.value > 0.5f;
                bool hasMouth = Random.value > 0.5f;

                // Must pick at least one
                if (!hasHat && !hasMouth)
                {
                    if (Random.value > 0.5f) hasHat = true;
                    else hasMouth = true;
                }

                if (hasHat) hints.Add(new Hint(HintType.MaskHasHat));
                if (hasMouth) hints.Add(new Hint(HintType.MaskHasMouth));
            }

            // Category 2: Clothing
            bool wearsSuit = Random.value > 0.5f;
            hints.Add(new Hint(wearsSuit ? HintType.WearsSuit : HintType.WearsDress));

            // Category 3: Accessory
            bool hasAccessory = Random.value > 0.5f;
            hints.Add(new Hint(hasAccessory ? HintType.HasAccessory : HintType.HasNoAccessory));

            // Category 4: Dancing (optional, only add if we need more hints)
            if (hints.Count < count)
            {
                // Pick a dance state
                float danceRoll = Random.value;
                if (danceRoll < 0.4f)
                {
                    hints.Add(new Hint(HintType.IsNotDancing));
                }
                else if (danceRoll < 0.7f)
                {
                    hints.Add(new Hint(HintType.IsDancing));
                }
                else
                {
                    // Dancing with specific partner type
                    hints.Add(new Hint(Random.value > 0.5f ? HintType.DancingWithSuitPartner : HintType.DancingWithDressPartner));
                }
            }

            // Trim to count if we have too many
            while (hints.Count > count)
            {
                // Remove a random hint (but preserve order for remaining hints)
                hints.RemoveAt(Random.Range(0, hints.Count));
            }

            return hints;
        }

        private CharacterData CreateMatchingCharacter()
        {
            CharacterData data = new CharacterData { CharacterId = -1 }; // Will be assigned proper ID when replacing

            // Set attributes based on hints
            foreach (var hint in GeneratedHints)
            {
                ApplyHintToCharacter(data, hint);
            }

            // Fill in any unset attributes randomly
            FinalizeCharacterData(data);

            return data;
        }

        private void ApplyHintToCharacter(CharacterData data, Hint hint)
        {
            // Since MaskIdentifier is a struct, we need to get, modify, and reassign it
            switch (hint.Type)
            {
                case HintType.MaskIsMammal:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = true;
                        mask.AnimalMask = Random.value > 0.5f ? AnimalMaskType.Fox : AnimalMaskType.Rabbit;
                        data.Mask = mask;
                    }
                    break;
                case HintType.MaskIsPredator:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = true;
                        mask.AnimalMask = Random.value > 0.5f ? AnimalMaskType.Fox : AnimalMaskType.Shark;
                        data.Mask = mask;
                    }
                    break;
                case HintType.MaskIsAquatic:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = true;
                        mask.AnimalMask = Random.value > 0.5f ? AnimalMaskType.Shark : AnimalMaskType.Fish;
                        data.Mask = mask;
                    }
                    break;
                case HintType.MaskIsPrey:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = true;
                        mask.AnimalMask = Random.value > 0.5f ? AnimalMaskType.Rabbit : AnimalMaskType.Fish;
                        data.Mask = mask;
                    }
                    break;

                case HintType.MaskHasHat:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = false;
                        // If already has mouth requirement, must be Jester (has both)
                        if (mask.NonAnimalMask == NonAnimalMaskType.PlainFullFace ||
                            mask.NonAnimalMask == NonAnimalMaskType.Jester)
                        {
                            mask.NonAnimalMask = NonAnimalMaskType.Jester;
                        }
                        else
                        {
                            // Crowned or Jester have hats
                            mask.NonAnimalMask = Random.value > 0.5f ? NonAnimalMaskType.Crowned : NonAnimalMaskType.Jester;
                        }
                        data.Mask = mask;
                    }
                    break;
                case HintType.MaskHasMouth:
                    {
                        var mask = data.Mask;
                        mask.IsAnimalMask = false;
                        // If already has hat requirement, must be Jester (has both)
                        if (mask.NonAnimalMask == NonAnimalMaskType.Crowned ||
                            mask.NonAnimalMask == NonAnimalMaskType.Jester)
                        {
                            mask.NonAnimalMask = NonAnimalMaskType.Jester;
                        }
                        else
                        {
                            // PlainFullFace or Jester have mouths
                            mask.NonAnimalMask = Random.value > 0.5f ? NonAnimalMaskType.PlainFullFace : NonAnimalMaskType.Jester;
                        }
                        data.Mask = mask;
                    }
                    break;

                case HintType.WearsSuit:
                    data.Clothing = ClothingType.Suit;
                    break;
                case HintType.WearsDress:
                    data.Clothing = ClothingType.Dress;
                    break;

                case HintType.HasAccessory:
                    data.Accessories = data.Clothing == ClothingType.Suit ? Accessories.Bowtie : Accessories.Hairbow;
                    break;
                case HintType.HasNoAccessory:
                    data.Accessories = Accessories.None;
                    break;

                case HintType.IsNotDancing:
                    data.DanceState = DanceState.NotDancing;
                    data.DancePartnerId = -1;
                    break;
                case HintType.IsDancing:
                    // Will need to set up dancing later
                    if (data.DanceState == DanceState.NotDancing)
                    {
                        data.DanceState = Random.value > 0.5f ? DanceState.DancingWithSuitPartner : DanceState.DancingWithDressPartner;
                    }
                    break;
                case HintType.DancingWithSuitPartner:
                    data.DanceState = DanceState.DancingWithSuitPartner;
                    break;
                case HintType.DancingWithDressPartner:
                    data.DanceState = DanceState.DancingWithDressPartner;
                    break;
            }
        }

        private void FinalizeCharacterData(CharacterData data)
        {
            // Ensure mask is set (must reassign struct)
            var mask = data.Mask;
            bool maskModified = false;

            if (mask.IsAnimalMask && mask.AnimalMask == AnimalMaskType.None)
            {
                mask.AnimalMask = (AnimalMaskType)Random.Range(1, 5);
                maskModified = true;
            }
            else if (!mask.IsAnimalMask && mask.NonAnimalMask == NonAnimalMaskType.None)
            {
                mask.NonAnimalMask = (NonAnimalMaskType)Random.Range(1, 5);
                maskModified = true;
            }

            if (maskModified)
            {
                data.Mask = mask;
            }

            // Accessory - ensure it matches clothing type
            if (data.HasAnyAccessory)
            {
                data.Accessories = data.Clothing == ClothingType.Suit ? Accessories.Bowtie : Accessories.Hairbow;
            }
        }

        private void ReplaceRandomCharacter(CharacterData newData)
        {
            // Pick a random non-dancing, non-player character to replace (simpler than handling dance pairs)
            var nonDancingCharacters = characterObjects.Where(c => !c.Data.IsDancing && !c.Data.IsPlayer).ToList();

            Character toReplace;
            if (nonDancingCharacters.Count > 0)
            {
                toReplace = nonDancingCharacters[Random.Range(0, nonDancingCharacters.Count)];
            }
            else
            {
                // Fallback: pick any character and break their dance pair
                toReplace = characterObjects[Random.Range(0, characterObjects.Count)];
                if (toReplace.Data.IsDancing)
                {
                    BreakDancePair(toReplace);
                }
            }

            // Copy position and ID
            newData.CharacterId = toReplace.Data.CharacterId;
            newData.Position = toReplace.Data.Position;

            // Handle dance state - if new character needs to dance but replaced one didn't
            if (newData.IsDancing && !toReplace.Data.IsDancing)
            {
                // Can't easily set up dancing, so force to not dancing
                newData.DanceState = DanceState.NotDancing;
                newData.DancePartnerId = -1;

                // Remove any dance-related hints since they no longer apply
                GeneratedHints.RemoveAll(h =>
                    h.Type == HintType.IsDancing ||
                    h.Type == HintType.DancingWithSuitPartner ||
                    h.Type == HintType.DancingWithDressPartner);
            }

            // Update the character object
            toReplace.UpdateData(newData);

            // Update our reference
            int index = allCharacters.FindIndex(c => c.CharacterId == newData.CharacterId);
            if (index >= 0)
            {
                allCharacters[index] = newData;
            }
        }

        private void RandomizeUntilNoMatch(CharacterData character)
        {
            // Skip if character is player
            if (character.IsPlayer) return;

            int maxAttempts = 50;
            int attempts = 0;

            while (HintEvaluator.DoesCharacterMatchAllHints(character, GeneratedHints) && attempts < maxAttempts)
            {
                // Randomly change one attribute to break the match
                int attributeToChange = Random.Range(0, 4);

                switch (attributeToChange)
                {
                    case 0: // Change mask (must reassign struct)
                        {
                            var mask = character.Mask;
                            if (mask.IsAnimalMask)
                            {
                                // Change to different animal or to human
                                if (Random.value > 0.3f)
                                {
                                    mask.AnimalMask = (AnimalMaskType)Random.Range(1, 5);
                                }
                                else
                                {
                                    mask.IsAnimalMask = false;
                                    mask.NonAnimalMask = (NonAnimalMaskType)Random.Range(1, 5);
                                }
                            }
                            else
                            {
                                // Change to different human mask or to animal
                                if (Random.value > 0.3f)
                                {
                                    mask.NonAnimalMask = (NonAnimalMaskType)Random.Range(1, 5);
                                }
                                else
                                {
                                    mask.IsAnimalMask = true;
                                    mask.AnimalMask = (AnimalMaskType)Random.Range(1, 5);
                                }
                            }
                            character.Mask = mask;
                        }
                        break;

                    case 1: // Change clothing
                        character.Clothing = character.Clothing == ClothingType.Suit ? ClothingType.Dress : ClothingType.Suit;
                        // Update accessory to match
                        if (character.HasAnyAccessory)
                        {
                            character.Accessories = character.Clothing == ClothingType.Suit ? Accessories.Bowtie : Accessories.Hairbow;
                        }
                        break;

                    case 2: // Change accessory
                        if (character.HasAnyAccessory)
                        {
                            character.Accessories = Accessories.None;
                        }
                        else
                        {
                            character.Accessories = character.Clothing == ClothingType.Suit ? Accessories.Bowtie : Accessories.Hairbow;
                        }
                        break;

                    case 3: // Change dance state
                        if (character.IsDancing)
                        {
                            // Break the dance pair
                            var charObj = characterObjects.Find(c => c.Data.CharacterId == character.CharacterId);
                            if (charObj != null)
                            {
                                BreakDancePair(charObj);
                            }
                            character.DanceState = DanceState.NotDancing;
                            character.DancePartnerId = -1;
                        }
                        else
                        {
                            // Can't easily make them dance, try another attribute
                            continue;
                        }
                        break;
                }

                attempts++;
            }

            // Update the visual
            var charObject = characterObjects.Find(c => c.Data.CharacterId == character.CharacterId);
            if (charObject != null)
            {
                charObject.UpdateData(character);
            }
        }

        /// <summary>
        /// Breaks a dance pair by making the partner stop dancing too.
        /// </summary>
        private void BreakDancePair(Character character)
        {
            if (!character.Data.IsDancing) return;

            // Find and update partner
            var partner = characterObjects.Find(c => c.Data.CharacterId == character.Data.DancePartnerId);
            if (partner != null)
            {
                partner.Data.DanceState = DanceState.NotDancing;
                partner.Data.DancePartnerId = -1;
                partner.UpdateData(partner.Data);
            }

            // Update this character
            character.Data.DanceState = DanceState.NotDancing;
            character.Data.DancePartnerId = -1;
        }

        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
