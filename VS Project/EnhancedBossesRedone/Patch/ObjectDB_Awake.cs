using EnhancedBossesRedone.Abstract;
using EnhancedBossesRedone.Data;
using HarmonyLib;
using UnityEngine;

namespace EnhancedBossesRedone.Patch
{
    internal class ObjectDB_WakeUp
    {
        [HarmonyPatch(typeof(ObjectDB), "Awake")]
        public static class ObjectDB_WakeUp_Postfix
        {
            public static void Postfix(ObjectDB __instance)
            {
                if (!ObjectDB_Awake.ObjectDB_Awake_Postfix.alreadyInvoked)
                {
                    return;
                }

                foreach (Boss boss in Main.bossList!)
                {
                    boss.AddToObjectDB(__instance);
                }

                foreach (ExternalItem item in Main.externalItems!)
                {
                    item.AddToObjectDB(__instance);
                }
            }
        }
    }


    internal class ObjectDB_Awake
    {
        [HarmonyPatch(typeof(FejdStartup), "SetupObjectDB")]
        [HarmonyPriority(800)]
        public static class ObjectDB_Awake_Postfix
        {
            public static void Postfix(FejdStartup __instance)
            {

                if (__instance == null)
                {
                    Main.Log!.LogError("ObjectDB not found. Cannot load custom items.");
                    return;
                }

                if (alreadyInvoked)
                {
                    Main.Log!.LogInfo("Already invoked item loading. Skipping");
                    return;
                }

                FejdObjectDB = __instance.m_objectDBPrefab;
                ObjectDB component = FejdObjectDB.GetComponent<ObjectDB>();
                component.UpdateRegisters();

                Main.Log!.LogMessage("Setting up external attacks.");
                SetupExternalAttacks(component);
                Main.Log!.LogMessage("External attacks created successfully.");
                Main.Log!.LogMessage("Setting up custom status effects...");
                SetupStatusEffects(component);
                ObjectDB.instance.CopyOtherDB(component);
                Main.Log!.LogMessage("Done.");
                alreadyInvoked = true;
            }

            private static void SetupExternalAttacks(ObjectDB instance)
            {
                foreach (ExternalItem attack in Main.externalItems!)
                {
                    attack.Setup(instance);
                    attack.AddToObjectDB(instance);
                }
            }

            private static void SetupStatusEffects(ObjectDB instance)
            {
                foreach (StatusEffect effect in Main.statusEffects)
                {
                    instance.AddStatusEffect(effect);
                }
            }

            public static GameObject? FejdObjectDB;
            public static bool alreadyInvoked;
        }
    }
}
