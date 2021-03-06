using R2API;
using RoR2;
using UnityEngine;

namespace CultistPlugin
{
    class FreezeFluteItem
    {
        internal static ItemDef FreezeFluteItemDef;
        private static string ItemNameToken = "CULTIST_FREEZEFLUTE_NAME";
        private static string ItemPickupToken = "CULTIST_FREEZEFLUTE_PICKUP";
        private static string ItemDescriptionToken = "CULTIST_FREEZEFLUTE_DESCRIPTION";
        private static string ItemLoreToken = "CULTIST_FREEZEFLUTE_LORE";
        private static float FrostNovaDamageCoefficient = 1.0f;
        private static float FrostNovaForce = 0f;
        private static float FrostNovaBaseRadius = 12f;
        private static float FrostNovaStackRadius = 4f;

        //The Awake() method is run at the very start when the game is initialized.
        public void Init()
        {

            AddItemDef();
            AddLanguageTokens();
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;

            Log.LogInfo(nameof(Init) + " done.");
        }

        private static void AddItemDef()
        {
            FreezeFluteItemDef = ScriptableObject.CreateInstance<ItemDef>();
            FreezeFluteItemDef.name = ItemNameToken;
            FreezeFluteItemDef.nameToken = ItemNameToken;
            FreezeFluteItemDef.pickupToken = ItemPickupToken;
            FreezeFluteItemDef.descriptionToken = ItemDescriptionToken;
            FreezeFluteItemDef.loreToken = ItemLoreToken;
            FreezeFluteItemDef.tier = ItemTier.Tier2;
            FreezeFluteItemDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            FreezeFluteItemDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

            FreezeFluteItemDef.canRemove = true;
            FreezeFluteItemDef.hidden = false;

            var displayRules = new ItemDisplayRuleDict(null);

            ItemAPI.Add(new CustomItem(FreezeFluteItemDef, displayRules));
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add(ItemNameToken, "N'kuhana's Breath");
            LanguageAPI.Add(ItemPickupToken, "Enemies release a frost nova on kill");
            LanguageAPI.Add(ItemDescriptionToken, "Whenever you <style=cIsDamage>kill an enemy</style>, they explode, freezing for <style=cIsUtility>2s</style> <style=cStack>+1s</style> and dealing <style=cIsDamage>100%</style> damage.");
            LanguageAPI.Add(ItemLoreToken, "Bone chilling winds...");
        }

        private void OnCharacterDeath(DamageReport report)
        {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
                return;

            CharacterBody attacker = report.attackerBody;

            //We need an inventory to do check for our item
            if (attacker.inventory)
            {
                //store the amount of our item we have
                int garbCount = attacker.inventory.GetItemCount(FreezeFluteItemDef.itemIndex);
                float frostNovaRadius = FrostNovaBaseRadius + FrostNovaStackRadius * (garbCount - 1);
                if (garbCount > 0)
                {
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/NullifierExplosion"), new EffectData
                    {
                        origin = report.victimBody.transform.position,
                        scale = frostNovaRadius
                    }, true);
                    new BlastAttack
                    {
                        attacker = report.attacker,
                        inflictor = report.attacker,
                        teamIndex = TeamComponent.GetObjectTeam(report.attacker),
                        baseDamage = report.attackerBody.damage * FrostNovaDamageCoefficient,
                        baseForce = FrostNovaForce,
                        damageType = DamageType.Freeze2s,
                        position = report.victimBody.transform.position,
                        procChainMask = report.damageInfo.procChainMask,
                        radius = FrostNovaBaseRadius + FrostNovaStackRadius * (garbCount - 1),
                        procCoefficient = 1f,
                        attackerFiltering = AttackerFiltering.NeverHit,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/CryoCanisterExplosionPrimary.prefab"))
                    }.Fire();
                }
            }
        }
        public void Update()
        {
            //This if statement checks if the player has currently pressed F3.
            if (Input.GetKeyDown(KeyCode.F4))
            {
                //Get the player body to use a position:	
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then drop our defined item in front of the player.

                Log.LogInfo($"Player pressed F3. Spawning our custom item at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(FreezeFluteItemDef.itemIndex), transform.position, transform.forward * 20f);
            }
        }
    }
}
