using System.Collections.Generic;
using System.Linq;

namespace MasqueradeMystery
{
    public static class HintEvaluator
    {
        public static bool DoesCharacterMatchHint(CharacterData character, Hint hint)
        {
            bool matches = hint.Type switch
            {
                // Mask category
                HintType.MaskIsAnimal => character.Mask.IsAnimalMask,
                HintType.MaskIsNonAnimal => !character.Mask.IsAnimalMask,

                // Animal categories
                HintType.MaskIsMammal => character.Mask.IsMammal,
                HintType.MaskIsPredator => character.Mask.IsPredator,
                HintType.MaskIsAquatic => character.Mask.IsAquatic,
                HintType.MaskIsPrey => character.Mask.IsPrey,

                // Non-animal traits
                HintType.MaskHasHat => character.Mask.HasHat,
                HintType.MaskHasMouth => character.Mask.HasMouth,
                HintType.MaskHasNoHat => !character.Mask.IsAnimalMask && !character.Mask.HasHat,
                HintType.MaskHasNoMouth => !character.Mask.IsAnimalMask && !character.Mask.HasMouth,

                // Exact masks
                HintType.MaskIsFox => character.Mask.IsAnimalMask && character.Mask.AnimalMask == AnimalMaskType.Fox,
                HintType.MaskIsRabbit => character.Mask.IsAnimalMask && character.Mask.AnimalMask == AnimalMaskType.Rabbit,
                HintType.MaskIsShark => character.Mask.IsAnimalMask && character.Mask.AnimalMask == AnimalMaskType.Shark,
                HintType.MaskIsFish => character.Mask.IsAnimalMask && character.Mask.AnimalMask == AnimalMaskType.Fish,
                HintType.MaskIsPlainEyes => !character.Mask.IsAnimalMask && character.Mask.NonAnimalMask == NonAnimalMaskType.PlainEyes,
                HintType.MaskIsPlainFullFace => !character.Mask.IsAnimalMask && character.Mask.NonAnimalMask == NonAnimalMaskType.PlainFullFace,
                HintType.MaskIsCrowned => !character.Mask.IsAnimalMask && character.Mask.NonAnimalMask == NonAnimalMaskType.Crowned,
                HintType.MaskIsJester => !character.Mask.IsAnimalMask && character.Mask.NonAnimalMask == NonAnimalMaskType.Jester,

                // Clothing
                HintType.WearsSuit => character.Clothing == ClothingType.Suit,
                HintType.WearsDress => character.Clothing == ClothingType.Dress,

                // Accessories
                HintType.HasBowtie => character.HasBowtie,
                HintType.HasHairbow => character.HasHairbow,
                HintType.HasNoAccessory => !character.HasAnyAccessory,
                HintType.HasSomeAccessory => character.HasAnyAccessory,

                // Dancing
                HintType.IsNotDancing => !character.IsDancing,
                HintType.IsDancing => character.IsDancing,
                HintType.DancingWithSuitPartner => character.DanceState == DanceState.DancingWithSuitPartner,
                HintType.DancingWithDressPartner => character.DanceState == DanceState.DancingWithDressPartner,

                _ => false
            };

            // Apply positive/negative modifier
            return hint.IsPositive ? matches : !matches;
        }

        public static bool DoesCharacterMatchAllHints(CharacterData character, List<Hint> hints)
        {
            return hints.All(h => DoesCharacterMatchHint(character, h));
        }

        public static int CountMatchingCharacters(List<CharacterData> characters, Hint hint)
        {
            return characters.Count(c => DoesCharacterMatchHint(c, hint));
        }

        public static int CountMatchingCharacters(List<CharacterData> characters, List<Hint> hints)
        {
            return characters.Count(c => DoesCharacterMatchAllHints(c, hints));
        }

        public static List<CharacterData> GetMatchingCharacters(List<CharacterData> characters, List<Hint> hints)
        {
            return characters.Where(c => DoesCharacterMatchAllHints(c, hints)).ToList();
        }
    }
}
