using HarmonyLib;
using Spirefrost.Builders.StatusEffects.Utility;
using System;
using System.Linq;

namespace Spirefrost.StatusEffects
{
    internal class StatusEffectCustomSaveable : StatusEffectData
    {
        [Serializable]
        public class SaveData
        {
            public object[] objects;

            public void Add(params object[] obj)
            {
                objects = objects.AddRangeToArray(obj);
            }
        }

        private SaveData saveable = new SaveData();

        public override void Init()
        {
            MainModFile.Print($"CustomSaveable initialized for {target}");
        }

        public override void RestoreMidBattleData(object data)
        {
            MainModFile.Print($"CustomSaveable loaded for {target}");
            MainModFile.Print($"Got {data}");
            SaveData save = data as SaveData;
            MainModFile.Print($"Can we cast to SaveData? {save != null}");
            if (save != null)
            {
                saveable = save;
                object[] test = save.objects;
                MainModFile.Print($"Got {test.Length} values, loading");
                int index = 0;
                while (index < test.Length)
                {
                    if (test[index] is int next)
                    {
                        MainModFile.Print($"Next chunk is {next} objects long...");
                        if (index + next >= test.Length)
                        {
                            MainModFile.Print($"This will out of bounds!");
                            break;
                        }
                        string type = test[index + 1] as string;
                        string method = test[index + 2] as string;
                        object[] paramArray = new object[0];
                        for (int i = 3; i <= next; i++)
                        {
                            paramArray = paramArray.With(test[index + i]);
                        }
                        MainModFile.Print($"Calling {type.Split(',')[0]}.{method} with {paramArray.Join()}");
                        Type.GetType(type).GetMethod(method, AccessTools.all).Invoke(null, new object[] { target }.AddRangeToArray(paramArray));
                        index += next + 1;
                    }
                    else
                    {
                        MainModFile.Print($"Got unexpected value at index {index}: {test[index]}");
                        break;
                    }
                }
            }
        }

        public override object GetMidBattleData()
        {
            MainModFile.Print($"CustomSaveable saved for {target} with {saveable.objects.Length} objects");
            return saveable;
        }

        public void MakeSaveable(Type type, string methodName, params object[] parameters)
        {
            MainModFile.Print($"CustomSaveable added for {target}: {type.AssemblyQualifiedName.Split(',')[0]}.{methodName} with {parameters.Join()}");
            saveable.Add(2 + parameters.Length);
            saveable.Add(type.AssemblyQualifiedName);
            saveable.Add(methodName);
            saveable.Add(parameters);
        }

        public static StatusEffectCustomSaveable Get(Entity entity)
        {
            return entity.statusEffects.Where(s => s is StatusEffectCustomSaveable).FirstOrDefault() as StatusEffectCustomSaveable;
        }

        public static StatusEffectCustomSaveable GetOrMake(Entity entity)
        {
            StatusEffectCustomSaveable ret = Get(entity);
            if (ret == null)
            {
                StatusEffectSystem.activeEffects.Freeze();
                ret = MainModFile.instance.TryGet<StatusEffectData>(CustomSaveable.ID).Instantiate() as StatusEffectCustomSaveable;
                ret.Apply(1, entity, entity);
                StatusEffectSystem.activeEffects.Add(ret);
                StatusEffectSystem.activeEffects.Thaw();
            }
            return ret;
        }
    }
}
