using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace Vehicle
{
    class Itab_Pawn_DriverHealth : ITab_Pawn_Health
    {
        public Itab_Pawn_DriverHealth()
        {
            this.size = new Vector2(300f, 450f);
        }
        public override bool IsVisible
        {
            get { return true; }
        }
        protected override void FillTab()
        {
            float fieldHeight = 30.0f;

            Vehicle vehicle = this.SelThing as Vehicle;

            ConceptDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.GuiFrame);
            Text.Font = GameFont.Small;
            Rect innerRect1 = GenUI.ContractedBy(new Rect(0.0f, 0.0f, this.size.x, this.size.y), 10f);
            GUI.BeginGroup(innerRect1);
            Rect mountedRect = new Rect(0.0f, 30.0f, innerRect1.width, fieldHeight);
            float mountedRectY = mountedRect.y;
            Widgets.ListSeparator(ref mountedRectY, innerRect1.width, "Mounted Character");
            mountedRect.y += fieldHeight;
            Rect thingIconRect = new Rect(mountedRect.x, mountedRect.y, 30f, fieldHeight);
            Rect thingLabelRect = new Rect(mountedRect.x + 35f, mountedRect.y + 5.0f, innerRect1.width - 35f, fieldHeight);
            Rect thingButtonRect = new Rect(mountedRect.x, mountedRect.y, innerRect1.width, fieldHeight);

            if (vehicle.IsMounted)
            {
                Pawn driver = vehicle.Driver;
                Widgets.ThingIcon(thingIconRect, driver.def);
                Widgets.Label(thingLabelRect, driver.Label);
                if (Widgets.InvisibleButton(thingButtonRect))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    FloatMenuOption dismount = new FloatMenuOption("Dismount " + driver.LabelBase, () =>
                    {
                        vehicle.Dismount();
                    });
                    options.Add(dismount);

                    Find.WindowStack.Add(new FloatMenu(options));
                }
                thingIconRect.y += fieldHeight;
                thingLabelRect.y += fieldHeight;
            }
            Rect storageRect = new Rect(0.0f, thingIconRect.y, innerRect1.width, fieldHeight);
            float storageRectY = storageRect.y;
            Widgets.ListSeparator(ref storageRectY, innerRect1.width, "Storage");
            storageRect.y += fieldHeight;
            thingIconRect.y = storageRect.y;
            thingLabelRect.y = storageRect.y;
            thingButtonRect.y = storageRect.y;
            foreach (var thing in vehicle.inventory.container)
            {
                if ((thing.ThingID.IndexOf("Human_Corpse") <= -1) ? false : true)
                    Widgets.DrawTextureFitted(thingIconRect, ContentFinder<Texture2D>.Get("Things/Pawn/IconHuman_Corpse"), 1.0f);
                else if ((thing.ThingID.IndexOf("Corpse") <= -1) ? false : true)
                {
                    Corpse corpse = thing as Corpse;
                    Widgets.ThingIcon(thingIconRect, corpse.innerPawn.def);
                }
                else
                    Widgets.ThingIcon(thingIconRect, thing);
                Widgets.Label(thingLabelRect, thing.Label);
                if (Widgets.InvisibleButton(thingButtonRect))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    FloatMenuOption dropit = new FloatMenuOption("Drop it", () =>
                    {
                        Thing dummy;
                        vehicle.inventory.container.TryDrop(thing, vehicle.Position, ThingPlaceMode.Near, out dummy);
                    });
                    options.Add(dropit);

                    Find.WindowStack.Add(new FloatMenu(options));
                }
                thingIconRect.y += fieldHeight;
                thingLabelRect.y += fieldHeight;
            }
            GUI.EndGroup();
        }
    }
}