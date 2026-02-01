namespace MasqueradeMystery
{
    [System.Serializable]
    public class Hint
    {
        public HintType Type;
        public string DisplayText;

        public Hint(HintType type)
        {
            Type = type;
            DisplayText = GenerateDisplayText();
        }

        private string GenerateDisplayText()
        {
            return Type switch
            {
                // Animal categories
                HintType.MaskIsMammal => "Animal mask: Mammal",
                HintType.MaskIsPredator => "Animal mask: Predator",
                HintType.MaskIsAquatic => "Animal mask: Aquatic",
                HintType.MaskIsPrey => "Animal mask: Prey",

                // Human mask traits
                HintType.MaskHasHat => "Human mask with hat",
                HintType.MaskHasMouth => "Human mask with visible mouth",

                // Clothing
                HintType.WearsSuit => "Wearing a suit",
                HintType.WearsDress => "Wearing a dress",

                // Accessories
                HintType.HasAccessory => "Has an accessory",
                HintType.HasNoAccessory => "Has no accessory",

                // Dancing
                HintType.IsNotDancing => "Standing alone",
                HintType.IsDancing => "Currently dancing",
                HintType.DancingWithSuitPartner => "Dancing with someone in a suit",
                HintType.DancingWithDressPartner => "Dancing with someone in a dress",

                _ => Type.ToString()
            };
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
