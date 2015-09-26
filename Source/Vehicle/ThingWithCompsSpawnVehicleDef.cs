using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace Vehicle
{
	public class ThingWithCompsSpawnVehicleDef : ThingDef
	{
        public PawnKindDef spawnPawnDef;

        //Cannot use it because defdatabase is not loaded in that time
        public virtual void DefaultDef()
        {
            this.defName = "SpawnVehicle";
            this.category = ThingCategory.Item;
            this.thingClass = System.Type.GetType("ThingWithCompsSpawnVehicle");
            this.drawerType = DrawerType.None;
            this.tickerType = TickerType.Normal;
            this.useHitPoints = false;
            this.recipeMaker = new RecipeMakerProperties();
            this.recipeMaker.workSpeedStat = DefDatabase<StatDef>.GetNamed("SmithingSpeed");
            this.recipeMaker.workSkill = SkillDefOf.Crafting;
            this.recipeMaker.workSkillLearnPerTick = 0.25f;
            this.recipeMaker.effectWorking = DefDatabase<EffecterDef>.GetNamed("Cook");
            this.recipeMaker.soundWorking = DefDatabase<SoundDef>.GetNamed("Recipe_Machining");
            this.recipeMaker.recipeUsers = new List<ThingDef>();
            this.recipeMaker.recipeUsers.Add(DefDatabase<ThingDef>.GetNamed("TableVehicleShop"));
            this.recipeMaker.defaultIngredientFilter = new ThingFilter();
            this.recipeMaker.defaultIngredientFilter.categories = new List<string>();
            this.recipeMaker.defaultIngredientFilter.categories.Add("Root");
            this.recipeMaker.defaultIngredientFilter.exceptedCategories = new List<string>();
            this.recipeMaker.defaultIngredientFilter.exceptedCategories.Add("Silver");
            this.recipeMaker.defaultIngredientFilter.exceptedCategories.Add("Gold");
        }

        public void FullCopy(ThingWithCompsSpawnVehicleDef thingWithCompsSpawnVehicleDef)
        {
            throw new NotImplementedException();
        }
	}
}
