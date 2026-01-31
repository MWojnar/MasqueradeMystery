namespace MasqueradeMystery
{
    public enum HintType
    {
        // Mask category hints
        MaskIsAnimal,
        MaskIsNonAnimal,

        // Animal mask specific categories
        MaskIsMammal,
        MaskIsPredator,
        MaskIsAquatic,
        MaskIsPrey,

        // Non-animal mask specific traits
        MaskHasHat,
        MaskHasMouth,
        MaskHasNoHat,
        MaskHasNoMouth,

        // Exact mask types (more specific hints)
        MaskIsFox,
        MaskIsRabbit,
        MaskIsShark,
        MaskIsFish,
        MaskIsPlainEyes,
        MaskIsPlainFullFace,
        MaskIsCrowned,
        MaskIsJester,

        // Clothing hints
        WearsSuit,
        WearsDress,

        // Accessory hints
        HasBowtie,
        HasHairbow,
        HasNoAccessory,
        HasSomeAccessory,

        // Dancing hints
        IsNotDancing,
        IsDancing,
        DancingWithSuitPartner,
        DancingWithDressPartner
    }
}
