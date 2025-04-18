using Deadpan.Enums.Engine.Components.Modding;
using Spirefrost.Builders.Icons;
using Spirefrost.Builders.Keywords;
using Spirefrost.Builders.StatusEffects.IconEffects;

namespace Spirefrost.Builders.StatusEffects
{
    internal class OnTurnApplyAmplifyToAllyBehind : SpirefrostBuilder
    {
        internal static string ID => "On Turn Apply Amplify To AllyBehind";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("On Turn Apply Shell To Allies", ID)
                .WithText($"Apply <{{a}}>{MakeKeywordInsert(AmplifyKeyword.FullID)} to ally behind")
                .SubscribeToAfterAllBuildEvent<StatusEffectApplyXOnTurn>(data =>
                {
                    data.effectToApply = TryGet<StatusEffectData>(Amplify.ID);
                    data.applyToFlags = StatusEffectApplyX.ApplyToFlags.AllyBehind;
                    data.noTargetTypeArgs = new string[] { MakeSpriteInsert(AmplifyIcon.SpriteID) };
                });
        }
    }
}
