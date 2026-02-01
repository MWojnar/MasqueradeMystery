namespace MasqueradeMystery
{
    public enum ClothingType
    {
        None,
        Suit,
        Dress
    }

    [System.Flags]
    public enum Accessories
    {
        None = 0,
        Bowtie = 1 << 0,   // For Suit
        Hairbow = 1 << 1   // For Dress
    }
}
