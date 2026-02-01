namespace MasqueradeMystery
{
    public enum HintType
    {
        // Animal mask categories (overlapping)
        MaskIsMammal,
        MaskIsPredator,
        MaskIsAquatic,
        MaskIsPrey,

        // Human mask traits (overlapping)
        MaskHasHat,
        MaskHasMouth,

        // Clothing hints
        WearsSuit,
        WearsDress,

        // Accessory hints (general only)
        HasAccessory,
        HasNoAccessory,

        // Dancing hints
        IsNotDancing,
        IsDancing,
        DancingWithSuitPartner,
        DancingWithDressPartner
    }
}
