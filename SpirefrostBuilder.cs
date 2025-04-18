using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost
{
    internal class SpirefrostBuilder
    {
        internal static T TryGet<T>(string name) where T : DataFile
        {
            return MainModFile.instance.TryGet<T>(name);
        }

        internal static CardData.StatusEffectStacks SStack(string name, int amount) => new CardData.StatusEffectStacks(TryGet<StatusEffectData>(name), amount);

        internal static CardData.TraitStacks TStack(string name, int amount) => new CardData.TraitStacks(TryGet<TraitData>(name), amount);

        internal static StatusEffectDataBuilder StatusCopy(string oldName, string newName)
        {
            StatusEffectData data = TryGet<StatusEffectData>(oldName).InstantiateKeepName();
            data.name = MainModFile.instance.GUID + "." + newName;
            data.targetConstraints = new TargetConstraint[0];
            StatusEffectDataBuilder builder = data.Edit<StatusEffectData, StatusEffectDataBuilder>();
            builder.Mod = MainModFile.instance;
            return builder;
        }

        internal static string MakeCardInsert(string fullID)
        {
            return $"<card={fullID}>";
        }

        internal static string MakeKeywordInsert(string fullID)
        {
            return $"<keyword={fullID}>";
        }

        internal static string MakeSpriteInsert(string spriteID)
        {
            return $"<sprite name={spriteID}>";
        }
    }
}
