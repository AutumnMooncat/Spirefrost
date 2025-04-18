using Deadpan.Enums.Engine.Components.Modding;
using System;
using System.Linq;
using UnityEngine;

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

        internal static StatusEffectDataBuilder StatusCopy(string oldName, string newName) => DataCopy<StatusEffectData, StatusEffectDataBuilder>(oldName, newName);

        internal static CardDataBuilder CardCopy(string oldName, string newName) => DataCopy<CardData, CardDataBuilder>(oldName, newName);

        internal static ClassDataBuilder TribeCopy(string oldName, string newName) => DataCopy<ClassData, ClassDataBuilder>(oldName, newName);

        internal static T DataCopy<Y, T>(string oldName, string newName) where Y : DataFile where T : DataFileBuilder<Y, T>, new()
        {
            Y data = MainModFile.instance.Get<Y>(oldName).InstantiateKeepName();
            data.name = MainModFile.instance.GUID + "." + newName;
            T builder = data.Edit<Y, T>();
            builder.Mod = MainModFile.instance;
            return builder;
        }

        internal static T[] DataList<T>(params string[] names) where T : DataFile => names.Select((s) => TryGet<T>(s)).ToArray();

        internal static RewardPool CreateRewardPool(string name, string type, DataFile[] list)
        {
            RewardPool pool = ScriptableObject.CreateInstance<RewardPool>();
            pool.name = name;
            pool.type = type;            //The usual types are Units, Items, Charms, and Modifiers.
            pool.list = list.ToList();
            return pool;
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
        internal static T MakeConstraint<T>() where T : TargetConstraint
        {
            T constraint = ScriptableObject.CreateInstance<T>();
            return constraint;
        }

        internal static T MakeConstraint<T>(Action<T> action) where T : TargetConstraint
        {
            T constraint = MakeConstraint<T>();
            action(constraint);
            return constraint;
        }
    }
}
