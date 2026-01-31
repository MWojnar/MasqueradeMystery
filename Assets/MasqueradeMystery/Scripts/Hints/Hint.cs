namespace MasqueradeMystery
{
    [System.Serializable]
    public class Hint
    {
        public HintType Type;
        public string DisplayText;
        public bool IsPositive; // true = "has this trait", false = "does NOT have this trait"

        public Hint(HintType type, bool isPositive = true)
        {
            Type = type;
            IsPositive = isPositive;
            DisplayText = GenerateDisplayText();
        }

        private string GenerateDisplayText()
        {
            string text = Type switch
            {
                // Mask category
                HintType.MaskIsAnimal => "Wearing an animal mask",
                HintType.MaskIsNonAnimal => "Wearing a non-animal mask",

                // Animal categories
                HintType.MaskIsMammal => "Their mask is a mammal",
                HintType.MaskIsPredator => "Their mask is a predator",
                HintType.MaskIsAquatic => "Their mask is aquatic",
                HintType.MaskIsPrey => "Their mask is prey",

                // Non-animal traits
                HintType.MaskHasHat => "Their mask has a hat",
                HintType.MaskHasMouth => "Their mask shows a mouth",
                HintType.MaskHasNoHat => "Their mask has no hat",
                HintType.MaskHasNoMouth => "Their mask hides the mouth",

                // Exact masks
                HintType.MaskIsFox => "Wearing a fox mask",
                HintType.MaskIsRabbit => "Wearing a rabbit mask",
                HintType.MaskIsShark => "Wearing a shark mask",
                HintType.MaskIsFish => "Wearing a fish mask",
                HintType.MaskIsPlainEyes => "Wearing a plain eye mask",
                HintType.MaskIsPlainFullFace => "Wearing a plain full-face mask",
                HintType.MaskIsCrowned => "Wearing a crowned mask",
                HintType.MaskIsJester => "Wearing a jester mask",

                // Clothing
                HintType.WearsSuit => "Wearing a suit",
                HintType.WearsDress => "Wearing a dress",

                // Accessories
                HintType.HasBowtie => "Has a bowtie",
                HintType.HasHairbow => "Has a hairbow",
                HintType.HasNoAccessory => "Has no accessory",
                HintType.HasSomeAccessory => "Has an accessory",

                // Dancing
                HintType.IsNotDancing => "Standing alone",
                HintType.IsDancing => "Currently dancing",
                HintType.DancingWithSuitPartner => "Dancing with someone in a suit",
                HintType.DancingWithDressPartner => "Dancing with someone in a dress",

                _ => Type.ToString()
            };

            // Add negation if not positive
            if (!IsPositive)
            {
                text = "NOT: " + text;
            }

            return text;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
