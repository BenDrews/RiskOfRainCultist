using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Security;

[module: UnverifiableCode]
namespace CultistPlugin
{
	//This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute specifies that we have a dependency on R2API, as we're using it to add our item to the game.
    //You don't need this if you're not using R2API in your plugin, it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency("com.bepis.r2api",BepInDependency.DependencyFlags.HardDependency)]
	
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
	//This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	
	//We will be using 3 modules from R2API: ItemAPI to add our item, ItemDropAPI to have our item drop ingame, and LanguageAPI to add our language tokens.
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(ItemDropAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(SurvivorAPI), nameof(PrefabAPI), nameof(SoundAPI))]
	
	//This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class CultistPlugin : BaseUnityPlugin
	{
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = developerPrefix + "." + PluginName;
        public const string PluginName = "Cultist";
        public const string PluginVersion = "1.0.0";
        public const string developerPrefix = "BEN_DREWS";

        private VampireItem vampireItem;
        private FreezeFluteItem freezeFluteItem;

        public static CultistPlugin instance;
        private CultistCharacter cultistCharacter;

		//The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {   
            instance = this;

            //Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            vampireItem = new VampireItem();
            vampireItem.Init();
            freezeFluteItem = new FreezeFluteItem();
            freezeFluteItem.Init();

            cultistCharacter = new CultistCharacter();
            cultistCharacter.Awake();

            // This line of log will appear in the bepinex console when the Awake method is done.
            Log.LogInfo(nameof(Awake) + " done.");
        }
        public void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            cultistCharacter.LateSetup(obj);
        }

        //The Update() method is run on every frame of the game.
        private void Update()
        {
            vampireItem.Update();
            freezeFluteItem.Update();
        }
    }
}
