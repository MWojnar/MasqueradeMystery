using UnityEngine;

namespace MasqueradeMystery
{
    [System.Serializable]
    public class CharacterData
    {
        public int CharacterId;
        public MaskIdentifier Mask;
        public ClothingType Clothing;
        public Accessories Accessories;
        public DanceState DanceState;
        public bool IsPlayer;

        // Reference to dance partner (by ID, resolved after spawn)
        public int DancePartnerId = -1;

        // Position in the ballroom
        public Vector2 Position;

        // Computed properties for hint matching
        public bool HasBowtie => (Accessories & Accessories.Bowtie) != 0;
        public bool HasHairbow => (Accessories & Accessories.Hairbow) != 0;
        public bool HasAnyAccessory => Accessories != Accessories.None;
        public bool IsDancing => DanceState != DanceState.NotDancing;

        public CharacterData Clone()
        {
            return new CharacterData
            {
                CharacterId = CharacterId,
                Mask = Mask,
                Clothing = Clothing,
                Accessories = Accessories,
                DanceState = DanceState,
                DancePartnerId = DancePartnerId,
                Position = Position,
                IsPlayer = IsPlayer
            };
        }

        public override string ToString()
        {
            return $"Character {CharacterId}: {Mask} | {Clothing} | {Accessories} | {DanceState}";
        }
    }
}
