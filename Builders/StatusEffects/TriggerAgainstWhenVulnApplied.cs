using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class TriggerAgainstWhenVulnApplied : SpirefrostBuilder
    {
        internal static string ID => "Trigger Against When Vuln Applied";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("Trigger Against When Snow Applied", ID)
                .WithTextInsert(MakeKeywordInsert(VulnerableKeyword.FullID))
                .SubscribeToAfterAllBuildEvent<StatusEffectTriggerWhenStatusApplied>(data =>
                {
                    data.textOrder = 1;
                    data.targetStatus = TryGet<StatusEffectData>(Vulnerable.ID);
                });
        }
    }
}
