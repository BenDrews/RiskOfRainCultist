using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace ExamplePlugin {

    class VampireItem {

        private ItemDef itemDef;

        public void Init() {

            //First let's define our item
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Language Tokens, check AddTokens() below.
            itemDef.name = "VAMPIRE_NAME";
            itemDef.nameToken = "VAMPIRE_NAME";
            itemDef.pickupToken = "VAMPIRE_PICKUP";
            itemDef.descriptionToken = "VAMPIRE_DESC";
            itemDef.loreToken = "VAMPIRE_LORE";

            //The tier determines what rarity the item is:
            //Tier1=white, Tier2=green, Tier3=red, Lunar=Lunar, Boss=yellow,
            //and finally NoTier is generally used for helper items, like the tonic affliction
            itemDef.tier = ItemTier.Tier1;

            //You can create your own icons and prefabs through assetbundles, but to keep this boilerplate brief, we'll be using question marks.
            itemDef.pickupIconSprite = Resources.Load<Sprite>("Textures/MiscIcons/texMysteryIcon");
            itemDef.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/PickupModels/PickupMystery");

            //Can remove determines if a shrine of order, or a printer can take this item, generally true, except for NoTier items.
            itemDef.canRemove = true;

            //Hidden means that there will be no pickup notification,
            //and it won't appear in the inventory at the top of the screen.
            //This is useful for certain noTier helper items, such as the DrizzlePlayerHelper.
            itemDef.hidden = false;

            //Now let's turn the tokens we made into actual strings for the game:
            AddTokens();

            //You can add your own display rules here, where the first argument passed are the default display rules: the ones used when no specific display rules for a character are found.
            //For this example, we are omitting them, as they are quite a pain to set up without tools like ItemDisplayPlacementHelper
            var displayRules = new ItemDisplayRuleDict(null);

            //But now we have defined an item, but it doesn't do anything yet. So we'll need to define that ourselves.
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;

            //Then finally add it to R2API
            ItemAPI.Add(new CustomItem(itemDef, displayRules));
        }

        private void AddTokens() {
            LanguageAPI.Add("VAMPIRE_NAME", "Mark of the Vampire");

            LanguageAPI.Add("VAMPIRE_PICKUP", "Restore health on kill");

            LanguageAPI.Add("VAMPIRE_DESC", "Whenever you <style=cIsDamage>kill an enemy</style>, you resture <style=cIsUtility>1</style> health.");
            
            LanguageAPI.Add("VAMPIRE_LORE", "Too bad you can't eat garlic anymore.");
        }

        private void OnCharacterDeath(DamageReport report) {
            //If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody )
                return;
            
            CharacterBody attacker = report.attackerBody;

            //We need an inventory to do check for our item
            if (attacker.inventory)
            {
                //store the amount of our item we have
                int count = attacker.inventory.GetItemCount(itemDef.itemIndex);
                if (count > 0) {
                    HealthComponent component = attacker.GetComponent<HealthComponent>();
                    if (component) {
                        component.Heal(10.0f * count, default(ProcChainMask), true);
                    }
                }
            }
        }

        public void Update() {
            //This if statement checks if the player has currently pressed F3.
            if (Input.GetKeyDown(KeyCode.F3))
            {
                //Get the player body to use a position:	
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                //And then drop our defined item in front of the player.

                Log.LogInfo($"Player pressed F3. Spawning our custom item at coordinates {transform.position}");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(itemDef.itemIndex), transform.position, transform.forward * 20f);
            }   
        }
    }

}