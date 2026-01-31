namespace MasqueradeMystery
{
    public enum AnimalMaskType
    {
        None,
        Fox,
        Rabbit,
        Shark,
        Fish
    }

    public enum NonAnimalMaskType
    {
        None,
        PlainEyes,
        PlainFullFace,
        Crowned,
        Jester
    }

    [System.Serializable]
    public struct MaskIdentifier
    {
        public bool IsAnimalMask;
        public AnimalMaskType AnimalMask;
        public NonAnimalMaskType NonAnimalMask;

        // Category properties based on the diagram
        public bool IsMammal => IsAnimalMask && (AnimalMask == AnimalMaskType.Fox || AnimalMask == AnimalMaskType.Rabbit);
        public bool IsPredator => IsAnimalMask && (AnimalMask == AnimalMaskType.Fox || AnimalMask == AnimalMaskType.Shark);
        public bool IsAquatic => IsAnimalMask && (AnimalMask == AnimalMaskType.Shark || AnimalMask == AnimalMaskType.Fish);
        public bool IsPrey => IsAnimalMask && (AnimalMask == AnimalMaskType.Rabbit || AnimalMask == AnimalMaskType.Fish);

        // Non-animal mask properties
        public bool HasHat => !IsAnimalMask && (NonAnimalMask == NonAnimalMaskType.Crowned || NonAnimalMask == NonAnimalMaskType.Jester);
        public bool HasMouth => !IsAnimalMask && (NonAnimalMask == NonAnimalMaskType.PlainFullFace || NonAnimalMask == NonAnimalMaskType.Jester);

        public string GetDisplayName()
        {
            if (IsAnimalMask)
                return AnimalMask.ToString();
            return NonAnimalMask.ToString();
        }

        public override string ToString()
        {
            return GetDisplayName();
        }
    }
}
