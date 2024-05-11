namespace CharacterCustomization
{
    public readonly struct SavedPart
    {
        public readonly string PartName;
        public readonly bool IsEnabled;
        public readonly int VariantIndex;

        public SavedPart(string partName, bool isEnabled, int variantIndex)
        {
            PartName = partName;
            IsEnabled = isEnabled;
            VariantIndex = variantIndex;
        }
    }
}