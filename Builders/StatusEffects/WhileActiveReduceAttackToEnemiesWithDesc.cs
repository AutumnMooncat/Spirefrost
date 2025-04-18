using Deadpan.Enums.Engine.Components.Modding;

namespace Spirefrost.Builders.StatusEffects
{
    internal class WhileActiveReduceAttackToEnemiesWithDesc : SpirefrostBuilder
    {
        internal static string ID => "While Active Reduce Attack To Enemies (With Desc)";

        internal static string FullID => Extensions.PrefixGUID(ID, MainModFile.instance);

        internal static object GetBuilder()
        {
            return StatusCopy("While Active Reduce Attack To Enemies (No Ping, No Desc)", ID)
                .WithStackable(true)
                .WithCanBeBoosted(true)
                .WithText("While active, reduce <keyword=attack> of all enemies by <{a}>");
        }
    }
}
