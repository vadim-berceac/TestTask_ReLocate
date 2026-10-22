public static class WearableItemInstanceFactory
{
    public static WearableItemInstance Create(
        PropBones propBones,
        WearableItemData data)
    {
        return data switch
        {
            WeaponData weaponData => new WeaponInstance(propBones, weaponData),
            _ => new WearableItemInstance(propBones, data)
        };
    }
}
