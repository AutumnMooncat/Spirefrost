using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnApplyRegenToAllies : SpirefrostBuilder
    {
        internal static string ID => "On Turn Apply Regen To Allies";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Apply Shell To Allies", ID)
                .WithText($"Apply <{{a}}>{MakeKeywordInsert(RegenKeyword.FullID)} to all allies")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Regen.ID);
                    data.noTargetTypeArgs = new string[] { MakeSpriteInsert(RegenIcon.SpriteID) };
                });
        }
    }
}
