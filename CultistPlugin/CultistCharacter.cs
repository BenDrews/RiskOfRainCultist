using BepInEx;
using CultistPlugin.Modules.Survivors;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

namespace CultistPlugin
{

    public class CultistCharacter
    {
        internal List<SurvivorBase> Survivors = new List<SurvivorBase>();

        public void Awake()
        {
            // load assets and read config
            Modules.Assets.Initialize();
            Modules.Config.ReadConfig();
            Modules.States.RegisterStates(); // register states for networking
            Modules.Buffs.RegisterBuffs(); // add and register custom buffs/debuffs
            Modules.Projectiles.RegisterProjectiles(); // add and register custom projectiles
            Modules.Tokens.AddTokens(); // register name tokens
            Modules.ItemDisplays.PopulateDisplays(); // collect item display prefabs for use in our display rules

            // survivor initialization
            new Cultist().Initialize();

            // now make a content pack and add it- this part will change with the next update
            new Modules.ContentPacks().Initialize();

            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;

            Hook();
        }

        public void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            // have to set item displays later now because they require direct object references..
            Modules.Survivors.Cultist.instance.SetItemDisplays();
        }

        private void Hook()
        {
            // run hooks here, disabling one is as simple as commenting out the line
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        float scytheExecutionThreshold = 0.5f;

        private void Execute(HealthComponent self, DamageInfo damageInfo)
        {
            if (self.combinedHealthFraction < scytheExecutionThreshold)
            {
                float executionHealthLost = UnityEngine.Mathf.Max(self.combinedHealth, 0);
                if (self.health > 0f)
                {
                    self.Networkhealth = 0f;
                }
                if (self.shield > 0f)
                {
                    self.shield = 0f;
                }
                DamageReport damageReport = new DamageReport(damageInfo, self, executionHealthLost, self.combinedHealth);
                if (!self.alive)
                {
                    self.killingDamageType = damageInfo.damageType;
                    GlobalEventManager.ServerCharacterExecuted(damageReport, executionHealthLost);
                    IOnKilledServerReceiver[] components = self.GetComponents<IOnKilledServerReceiver>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        components[i].OnKilledServer(damageReport);
                    }
                    if (damageInfo.attacker)
                    {
                        IOnKilledOtherServerReceiver[] components2 = damageInfo.attacker.GetComponents<IOnKilledOtherServerReceiver>();
                        for (int i = 0; i < components2.Length; i++)
                        {
                            components2[i].OnKilledOtherServer(damageReport);
                        }
                    }
                    if (Util.CheckRoll(self.globalDeathEventChanceCoefficient * 100f, 0f, null))
                    {
                        GlobalEventManager.instance.OnCharacterDeath(damageReport);
                        return;
                    }
                }
            }
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            bool overrideDamage = false;
            if (damageInfo.attacker)
            {
                CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (characterBody)
                {
                    if (characterBody.HasBuff(Modules.Buffs.cultistBuff))
                    {
                        if ((damageInfo.damageType & DamageType.ResetCooldownsOnKill) != 0)
                        {
                            overrideDamage = true;
                            damageInfo.damageType = damageInfo.damageType & ~(DamageType.ResetCooldownsOnKill);
                            orig(self, damageInfo);
                            // Try to execute if below threshold
                            Execute(self, damageInfo);
                            // On kill get a buff
                            if (!self.alive) {
                                characterBody.AddTimedBuff(RoR2Content.Buffs.WarCryBuff, 2.0f);
                            }
                        }
                    }
                }
            }

            if (!overrideDamage)
            {
                orig(self, damageInfo);
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            // a simple stat hook, adds armor after stats are recalculated
            if (self)
            {
                if (self.HasBuff(Modules.Buffs.armorBuff))
                {
                    self.armor += 300f;
                }
            }
        }
    }
}