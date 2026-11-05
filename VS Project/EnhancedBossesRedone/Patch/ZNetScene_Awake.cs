using EnhancedBossesRedone.Abstract;
using EnhancedBossesRedone.Data;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedBossesRedone.Patch
{
    internal class ZNetScene_Awake
    {
        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPriority(Priority.Last)]
        public static class ZNetScene_Awake_Postfix
        {
            public static void Postfix(ZNetScene __instance)
            {
                if (ObjectDB_Awake.ObjectDB_Awake_Postfix.FejdObjectDB == null)
                {
                    Main.Log!.LogError("No fejd");
                    return;
                }

                ConfigManager.ParseJson();
                ObjectDB objectDB = ObjectDB_Awake.ObjectDB_Awake_Postfix.FejdObjectDB.GetComponent<ObjectDB>();

                RestoreSupportedShaders();
                LoadModdedAssets(__instance);
                
                SetupBosses(__instance, objectDB);
                Main.ReloadPrefabHolder();
            }


            private static void SetupBosses(ZNetScene instance, ObjectDB objectDB)
            {
                foreach (Boss boss in Main.bossList!)
                {
                    boss.SetupCustomAttacks(objectDB);
                    boss.SetupCharacter(instance);
                    boss.SetupLateCustomAttacks();
                    boss.AddToObjectDB(objectDB);
                    boss.AddCustomAttacksToPrefab();
                    boss.AddBossPrefab(instance);
                    Main.Log!.LogMessage("Added " + boss.customAttacks!.Count.ToString() + " custom attacks to " + boss.bossName + ".");
                }
            }

            private static void LoadModdedAssets(ZNetScene instance)
            {
                foreach (ExternalEntity entity in Main.externalEntities!)
                {
                    entity.Setup(instance);
                    entity.AddToPrefabs(instance);
                }
            }

            private static void RestoreSupportedShaders()
            {
                Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();
                foreach (Shader shader in Resources.FindObjectsOfTypeAll<Shader>())
                {
                    if (shader.isSupported)
                    {
                        shaders[shader.name] = shader;
                    }
                }

                foreach (GameObject prefab in Main.Bundle!.LoadAllAssets<GameObject>())
                {
                    foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (material == null || material.shader == null || material.shader.isSupported)
                            {
                                continue;
                            }

                            string shaderName = material.shader.name;
                            if (shaders.TryGetValue(shaderName, out Shader replacement))
                            {
                                material.shader = replacement;
                                Main.Log!.LogInfo("Restored shader " + shaderName + " on " + prefab.name);
                            }
                            else
                            {
                                Main.Log!.LogWarning("No supported shader " + shaderName + " for " + prefab.name);
                            }
                        }
                    }
                }
            }
        }
    }
}
