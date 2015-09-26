using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vehicle
{
    public class CompRecipeDefModify : ThingComp
    {
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            this.SetupThingWithCompsSpawnVehicleDef();
        }

        public void SetupThingWithCompsSpawnVehicleDef()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs.Where(x => x.thingClass == System.Type.GetType("Vehicle")))
            {
                if (DefDatabase<ThingDef>.GetNamedSilentFail("Spawn" + thingDef.defName) != null)
                    continue;

                ThingWithCompsSpawnVehicleDef thingWithCompsSpawnVehicle = new ThingWithCompsSpawnVehicleDef();
                thingWithCompsSpawnVehicle.DefaultDef();

                thingWithCompsSpawnVehicle.defName = "Spawn" + thingDef.defName;
                if (DefDatabase<PawnKindDef>.GetNamed(thingDef.defName) != null)
                    thingWithCompsSpawnVehicle.spawnPawnDef = DefDatabase<PawnKindDef>.GetNamed(thingDef.defName);
                else
                    Log.Error(thingDef.defName + " doesn't have PawnKindDef");
                thingWithCompsSpawnVehicle.label = thingDef.label;
                thingWithCompsSpawnVehicle.description = thingDef.description;
                thingWithCompsSpawnVehicle.costStuffCount = thingDef.costStuffCount;
                if (thingDef.statBases.Find(x => x.stat == StatDefOf.WorkToMake) != null)
                    thingWithCompsSpawnVehicle.recipeMaker.workAmount = (int)thingDef.statBases.Find(x => x.stat == StatDefOf.WorkToMake).value;
                else
                {
                    Log.Error(thingDef.defName + " doesn't have WorkToMake value. Set 1000 default value");
                    thingWithCompsSpawnVehicle.recipeMaker.workAmount = 1000;
                }

                thingWithCompsSpawnVehicle.PostLoad();
                DefDatabase<ThingWithCompsSpawnVehicleDef>.Add(thingWithCompsSpawnVehicle);

                RecipeDef recipeDef = FullCopyRecipeDef(DefDatabase<RecipeDef>.GetNamed("Make_SpawnVehicleAbstract"));

                recipeDef.defName = "Make_" + "Spawn" + thingDef.defName;
                    string key1 = "RecipeMake";
                    object[] objArray1 = new object[1];
                    int index1 = 0;
                    string str1 = thingDef.label;
                    objArray1[index1] = (object) str1;
                    string str2 = Translator.Translate(key1, objArray1);
                recipeDef.label = str2;
                    string key2 = "RecipeMakeJobString";
                    object[] objArray2 = new object[1];
                    int index2 = 0;
                    string str3 = thingDef.label;
                    objArray2[index2] = (object) str3;
                    string str4 = Translator.Translate(key2, objArray2);
                recipeDef.jobString = str4;
                if (thingDef.MadeFromStuff)
                {
                    IngredientCount ingredientCount = new IngredientCount();
                    ingredientCount.SetBaseCount((float)thingDef.costStuffCount);
                    ingredientCount.filter.SetAllowAllWhoCanMake(thingDef);
                    recipeDef.ingredients.Add(ingredientCount);
                    recipeDef.fixedIngredientFilter.SetAllowAllWhoCanMake(thingDef);
                    recipeDef.productHasIngredientStuff = true;
                }
                recipeDef.products = new List<ThingCount>();
                recipeDef.products.Add(new ThingCount(thingDef, 1));
                ThingDef unfinishedVehicle = FullCopyUnfinishedThingDef(DefDatabase<ThingDef>.GetNamed("UnfinishedAbstract"));
                unfinishedVehicle.defName = "Unfinished" + thingDef.defName;
                unfinishedVehicle.graphicData.texPath = thingDef.graphicData.texPath;
                unfinishedVehicle.graphicData.graphicClass = thingDef.graphicData.graphicClass;
                unfinishedVehicle.graphicData.drawSize = thingDef.graphicData.drawSize;
                unfinishedVehicle.PostLoad();
                DefDatabase<ThingDef>.Add(unfinishedVehicle);
                recipeDef.unfinishedThingDef = unfinishedVehicle;

                recipeDef.PostLoad();
                DefDatabase<RecipeDef>.Add(recipeDef);
            }
        }

        public RecipeDef FullCopyRecipeDef(RecipeDef recipeDef)
        {
            if (recipeDef == null)
                return null;

            RecipeDef copy = new RecipeDef();

            copy.defName = recipeDef.defName;
            copy.workSpeedStat = recipeDef.workSpeedStat;
            copy.workSkill = recipeDef.workSkill;
            copy.workSkillLearnFactor = recipeDef.workSkillLearnFactor;
            copy.effectWorking = recipeDef.effectWorking;
            copy.soundWorking = recipeDef.soundWorking;
            copy.recipeUsers = GenList.ListFullCopyOrNull(recipeDef.recipeUsers);
            copy.defaultIngredientFilter = new ThingFilter();
            copy.defaultIngredientFilter.categories = GenList.ListFullCopyOrNull(recipeDef.defaultIngredientFilter.categories);
            copy.defaultIngredientFilter.exceptedThingDefs = GenList.ListFullCopyOrNull(recipeDef.defaultIngredientFilter.exceptedThingDefs);
            copy.unfinishedThingDef = recipeDef.unfinishedThingDef;

            return copy;
        }

        public ThingDef FullCopyUnfinishedThingDef(ThingDef unfinishedThingDef)
        {
            if (unfinishedThingDef == null)
                return null;

            ThingDef copy = new ThingDef();

            copy.defName = unfinishedThingDef.defName;
            copy.label = unfinishedThingDef.label;
            copy.thingClass = unfinishedThingDef.thingClass;
            copy.category = unfinishedThingDef.category;
            copy.label = unfinishedThingDef.label;
            copy.graphicData = unfinishedThingDef.graphicData;
            copy.altitudeLayer = unfinishedThingDef.altitudeLayer;
            copy.useHitPoints = unfinishedThingDef.useHitPoints;
            copy.isUnfinishedThing = unfinishedThingDef.isUnfinishedThing;
            copy.selectable = unfinishedThingDef.selectable;
            copy.tradeability = unfinishedThingDef.tradeability;
            copy.drawerType = unfinishedThingDef.drawerType;
            copy.statBases = GenList.ListFullCopyOrNull(unfinishedThingDef.statBases);
            copy.comps = GenList.ListFullCopyOrNull(unfinishedThingDef.comps);
            copy.alwaysHaulable = unfinishedThingDef.alwaysHaulable;
            copy.rotatable = unfinishedThingDef.rotatable;
            copy.pathCost = unfinishedThingDef.pathCost;
            copy.thingCategories = GenList.ListFullCopyOrNull(unfinishedThingDef.thingCategories);
            copy.stuffCategories = GenList.ListFullCopyOrNull(unfinishedThingDef.stuffCategories);

            return copy;
        }
    }
}
