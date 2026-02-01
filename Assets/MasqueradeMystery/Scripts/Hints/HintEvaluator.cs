using System.Collections.Generic;
using System.Linq;

namespace MasqueradeMystery
{
    public static class HintEvaluator
    {
        public static bool DoesCharacterMatchHint(CharacterData character, Hint hint)
        {
            return hint.Type switch
            {
                // Animal categories
                HintType.MaskIsMammal => character.Mask.IsMammal,
                HintType.MaskIsPredator => character.Mask.IsPredator,
                HintType.MaskIsAquatic => character.Mask.IsAquatic,
                HintType.MaskIsPrey => character.Mask.IsPrey,

                // Human mask traits
                HintType.MaskHasHat => character.Mask.HasHat,
                HintType.MaskHasMouth => character.Mask.HasMouth,

                // Clothing
                HintType.WearsSuit => character.Clothing == ClothingType.Suit,
                HintType.WearsDress => character.Clothing == ClothingType.Dress,

                // Accessories
                HintType.HasAccessory => character.HasAnyAccessory,
                HintType.HasNoAccessory => !character.HasAnyAccessory,

                // Dancing
                HintType.IsNotDancing => !character.IsDancing,
                HintType.IsDancing => character.IsDancing,
                HintType.DancingWithSuitPartner => character.DanceState == DanceState.DancingWithSuitPartner,
                HintType.DancingWithDressPartner => character.DanceState == DanceState.DancingWithDressPartner,

                _ => false
            };
        }

        public static bool DoesCharacterMatchAllHints(CharacterData character, List<Hint> hints)
        {
            return hints.All(h => DoesCharacterMatchHint(character, h));
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
