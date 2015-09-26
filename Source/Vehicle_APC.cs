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
    public class Vehicle_APC : Vehicle
    {
        //private static readonly IntVec2 drawSize = new IntVec2(3, 3);
        public Graphic_Single graphic_Outside;
        public Graphic_Single graphic_WheelL;
        public Graphic_Single graphic_WheelR;
        public Graphic_Single graphic_Hetch;
        public Graphic_Single graphic_HetchOpen;
        public Graphic_Single graphic_BackDoor;
        public Graphic_Single graphic_BackDoorInside;
        public Graphic_Single graphic_SideHole;

        public bool backDoorIsOpen;
        public bool hetchIsOpen;

        Vector3 componentOffset = new Vector3(0, 0, 0);
        public Component_TurretGun turretGun;

        public override void MountOn(Pawn pawn)
        {
            base.MountOn(pawn);
            if (pawn.Faction == Faction.OfColony)
                this.drafter.Drafted = true;
        }

        public override void CheckCallDriver()
        {
            ;
        }

        public override void SpawnSetup()
        {
 	         base.SpawnSetup();

             UpdateGraphics();

             backDoorIsOpen = false;
             hetchIsOpen = false;

             turretGun = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Component_TurretGun"), (ThingDef)null) as Component_TurretGun;
        }

        private void UpdateGraphics()
        {
            graphic_Outside = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_Roof", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_WheelL = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_wheel", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_WheelR = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_wheel", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_Hetch = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_Hetch", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_HetchOpen = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_HetchOpen", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_BackDoor = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_BackDoor", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_BackDoorInside = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_BackDoorInside", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
            graphic_SideHole = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Vehicle/APC_SideHole", def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
        }


        public override void Tick()
        {
            base.Tick();

            if (turretGun == null)
            {
                turretGun = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Component_TurretGun"), (ThingDef)null) as Component_TurretGun;
                turretGun.Position = (this.Position.ToVector3() + componentOffset).ToIntVec3();
                turretGun.SpawnSetup();
                turretGun.SetFaction(Faction.OfColony);
            }
            turretGun.Position = (this.Position.ToVector3() + componentOffset).ToIntVec3();
        }

        public override void DrawAt(Vector3 drawLoc)
        {
 	         base.DrawAt(drawLoc);
             Vector3 turretLoc = turretGun.Position.ToVector3() + componentOffset; turretLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn) + 0.02f;
             turretGun.DrawAt(turretLoc);

             Vector3 wheelLLoc = drawLoc; wheelLLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Waist) - 0.01f;
             Vector3 wheelRLoc = drawLoc; wheelRLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Waist) - 0.01f;
             Vector3 wheelLOffset = new Vector3(-1.0f, 0f, 0f);
             Vector3 wheelROffset = new Vector3(1.0f, 0f, 0f);

             if (graphic_WheelL == null)
                 UpdateGraphics();

             graphic_WheelL.Draw(wheelLLoc + wheelLOffset.RotatedBy(this.Rotation.AsAngle), this.Rotation, this);
             graphic_WheelR.Draw(wheelRLoc + wheelROffset.RotatedBy(this.Rotation.AsAngle), this.Rotation, this);

             if (!backDoorIsOpen)
                graphic_BackDoorInside.Draw(drawLoc, this.Rotation, this);

             if (!Find.Selector.IsSelected(this))
             {
                 Vector3 outsideLoc = drawLoc; outsideLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn) + 0.01f;

                 graphic_Outside.Draw(outsideLoc, this.Rotation, this);
                 if (!backDoorIsOpen)
                     graphic_BackDoor.Draw(drawLoc, this.Rotation, this);
                 if (!hetchIsOpen)
                     graphic_Hetch.Draw(drawLoc, this.Rotation, this);
                 else
                     graphic_HetchOpen.Draw(drawLoc, this.Rotation, this);
                 graphic_SideHole.Draw(drawLoc, this.Rotation, this);
             }
        }

    }
}