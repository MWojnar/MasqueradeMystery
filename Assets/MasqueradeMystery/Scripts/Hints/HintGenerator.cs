using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MasqueradeMystery
{
    public class HintGenerator
    {
        private CharacterData target;
        private List<CharacterData> allCharacters;

        public HintGenerator(CharacterData target, List<CharacterData> allCharacters)
        {
            this.target = target;
            this.allCharacters = allCharacters;
        }

        public List<Hint> GenerateHints(int count)
        {
            List<Hint> allPossibleHints = GetAllTrueHintsForTarget();
            List<Hint> selectedHints = new List<Hint>();

            // Shuffle hints to add variety
            ShuffleList(allPossibleHints);

            // Sort hints by how much they narrow down possibilities (fewer matches = more useful)
            // Add small random factor to avoid always picking the same hints
            allPossibleHints = allPossibleHints
                .OrderBy(h => HintEvaluator.CountMatchingCharacters(allCharacters, h) + Random.Range(-2f, 2f))
                .ToList();

            foreach (var hint in allPossibleHints)
            {
                if (selectedHints.Count >= count) break;

                // Check how many characters remain after adding this hint
                var testHints = new List<Hint>(selectedHints) { hint };
                int remaining = HintEvaluator.CountMatchingCharacters(allCharacters, testHints);

                // Only add hints that still leave at least 1 match (the target)
                // and provide meaningful filtering (reduce the pool)
                if (remaining >= 1)
                {
                    int previousRemaining = selectedHints.Count > 0
                        ? HintEvaluator.CountMatchingCharacters(allCharacters, selectedHints)
                        : allCharacters.Count;

                    // Only add if it actually filters some characters
                    if (remaining < previousRemaining || selectedHints.Count == 0)
                    {
                        selectedHints.Add(hint);
                    }
                }
            }

            // If we couldn't get enough useful hints, fill with any true hints
            foreach (var hint in allPossibleHints)
            {
                if (selectedHints.Count >= count) break;
                if (!selectedHints.Contains(hint))
                {
                    selectedHints.Add(hint);
                }
            }

            return selectedHints;
        }

        private List<Hint> GetAllTrueHintsForTarget()
        {
            List<Hint> hints = new List<Hint>();

            // Mask category hints
            hints.Add(new Hint(target.Mask.IsAnimalMask ? HintType.MaskIsAnimal : HintType.MaskIsNonAnimal));

            // Animal-specific category hints
            if (target.Mask.IsAnimalMask)
            {
                if (target.Mask.IsMammal) hints.Add(new Hint(HintType.MaskIsMammal));
                if (target.Mask.IsPredator) hints.Add(new Hint(HintType.MaskIsPredator));
                if (target.Mask.IsAquatic) hints.Add(new Hint(HintType.MaskIsAquatic));
                if (target.Mask.IsPrey) hints.Add(new Hint(HintType.MaskIsPrey));

                // Exact animal mask
                switch (target.Mask.AnimalMask)
                {
                    case AnimalMaskType.Fox: hints.Add(new Hint(HintType.MaskIsFox)); break;
                    case AnimalMaskType.Rabbit: hints.Add(new Hint(HintType.MaskIsRabbit)); break;
                    case AnimalMaskType.Shark: hints.Add(new Hint(HintType.MaskIsShark)); break;
                    case AnimalMaskType.Fish: hints.Add(new Hint(HintType.MaskIsFish)); break;
                }
            }
            else
            {
                // Non-animal mask traits
                if (target.Mask.HasHat)
                    hints.Add(new Hint(HintType.MaskHasHat));
                else
                    hints.Add(new Hint(HintType.MaskHasNoHat));

                if (target.Mask.HasMouth)
                    hints.Add(new Hint(HintType.MaskHasMouth));
                else
                    hints.Add(new Hint(HintType.MaskHasNoMouth));

                // Exact non-animal mask
                switch (target.Mask.NonAnimalMask)
                {
                    case NonAnimalMaskType.PlainEyes: hints.Add(new Hint(HintType.MaskIsPlainEyes)); break;
                    case NonAnimalMaskType.PlainFullFace: hints.Add(new Hint(HintType.MaskIsPlainFullFace)); break;
                    case NonAnimalMaskType.Crowned: hints.Add(new Hint(HintType.MaskIsCrowned)); break;
                    case NonAnimalMaskType.Jester: hints.Add(new Hint(HintType.MaskIsJester)); break;
                }
            }

            // Clothing hints
            hints.Add(new Hint(target.Clothing == ClothingType.Suit ? HintType.WearsSuit : HintType.WearsDress));

            // Accessory hints
            if (target.HasBowtie)
                hints.Add(new Hint(HintType.HasBowtie));
            else if (target.HasHairbow)
                hints.Add(new Hint(HintType.HasHairbow));
            else
                hints.Add(new Hint(HintType.HasNoAccessory));

            if (target.HasAnyAccessory)
                hints.Add(new Hint(HintType.HasSomeAccessory));

            // Dance hints
            hints.Add(new Hint(target.IsDancing ? HintType.IsDancing : HintType.IsNotDancing));

            if (target.DanceState == DanceState.DancingWithSuitPartner)
                hints.Add(new Hint(HintType.DancingWithSuitPartner));
            else if (target.DanceState == DanceState.DancingWithDressPartner)
                hints.Add(new Hint(HintType.DancingWithDressPartner));

            return hints;
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
