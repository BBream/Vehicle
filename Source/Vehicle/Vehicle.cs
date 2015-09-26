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
    public class Vehicle : Pawn, IThingContainerOwner
    {

        #region Variables and short method
        // ==================================

        private int tickTime;
        private const int tickPerTickRare = 400;

        public bool callDriver;
        public bool visibleInside;
        public bool isStandby;
        public int autoDismountTick = 0;
        public int thresholdAutoDismount = 4800;
        public VehicleDef vehicleDef;

        //Data of mounting
        public ThingContainer driverContainer;

        //Data of drawing
        public List<Graphic_Single> extraGraphics;

        //Data of Parts
        public List<Parts_TurretGun> turretGuns;

        //Method for mounting
        public ThingContainer GetContainer() { return driverContainer; }
        public IntVec3 GetPosition() { return this.Position; }
        public IntVec3 MountPos { get { return (Position + vehicleDef.vehicle.mountPosOffset.RotatedBy(this.Rotation)); } }
        public bool IsMounted { get { return (Driver != null) ? true : false; } }
        public Pawn Driver 
        { 
            get 
            {
                return (driverContainer != null && driverContainer.Count > 0) ? driverContainer.First() as Pawn : null; 
            } 
        }
        public virtual void MountOn(Pawn pawn) 
        {
            if (this.IsMounted                                             //No Space
                || (this.Faction != null && this.Faction != pawn.Faction)) //Not your vehicle
                return;

            if (pawn.Faction == Faction.OfColony && (pawn.needs.food.CurCategory == HungerCategory.Starving || pawn.needs.rest.CurCategory == RestCategory.Exhausted))
            {
                Messages.Message(pawn.LabelCap + "cannot mount on " + this.LabelCap + ": " + pawn.LabelCap + "is starving or exhausted", MessageSound.RejectInput);
                return;
            }

            this.pather.StopDead();
            this.jobs.StopAll();

            driverContainer.TryAdd(pawn);
            pawn.holder = this.GetContainer();
            callDriver = false;
        }
        public virtual void Dismount()
        {
            if (!this.IsMounted)
                return;

            this.pather.StopDead();
            this.jobs.StopAll();

            Thing dummy;
            if (driverContainer.Count > 0)
            {
                Driver.holder = null;
                driverContainer.TryDrop(Driver, MountPos, ThingPlaceMode.Near, out dummy);
            }
        }
        public virtual void BoardOn(Pawn pawn) 
        {
            if (this.inventory.container.Count(x => x is Pawn) >= vehicleDef.vehicle.boardableNum //No Space
                || (this.Faction != null && this.Faction != pawn.Faction))                        //Not your vehicle
                return;

            if (pawn.Faction == Faction.OfColony && (pawn.needs.food.CurCategory == HungerCategory.Starving || pawn.needs.rest.CurCategory == RestCategory.Exhausted))
            {
                Messages.Message(pawn.LabelCap + "cannot board on " + this.LabelCap + ": " + pawn.LabelCap + "is starving or exhausted", MessageSound.RejectInput);
                return;
            }

            this.inventory.container.TryAdd(pawn);
            pawn.holder = this.inventory.GetContainer();
        }
        public virtual void Unboard(Pawn pawn)
        {
            if (vehicleDef.vehicle.boardableNum <= 0 && this.inventory.container.Count(x => x is Pawn) <= 0)
                return;

            Thing dummy;
            if (this.inventory.container.Contains(pawn))
            {
                pawn.holder = null;
                this.inventory.container.TryDrop(pawn, MountPos, ThingPlaceMode.Near, out dummy);
            }
        }
        public virtual void UnboardAll()
        {
            if (vehicleDef.vehicle.boardableNum <= 0 && this.inventory.container.Count(x => x is Pawn) <= 0)
                return;

            Thing dummy;
            foreach (Pawn crew in this.inventory.container.Where(x => x is Pawn).ToList())
            {
                crew.holder = null;
                this.inventory.container.TryDrop(crew, MountPos, ThingPlaceMode.Near, out dummy);
            }
        }
        //public virtual void CheckCallDriver();

        #endregion

        #region Thing basic method 
        // ==================================

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            tickTime = 0;
            this.training = null;
            callDriver = false;
            visibleInside = false;
            driverContainer = new ThingContainer(this, true);
            driverContainer.owner = this;
            inventory = new Pawn_InventoryTracker(this);

            //def.race.corpseDef = ThingDefOf.Steel;
            vehicleDef = def as VehicleDef;

            //Work setting
            this.drafter = new Pawn_DraftController(this);
            callDriver = false;
            isStandby = false;

            turretGuns = new List<Parts_TurretGun>();
            if (!vehicleDef.vehicle.turretGunDefs.NullOrEmpty())
                for (int i = 0; i < vehicleDef.vehicle.turretGunDefs.Count; i++)
                {
                    Parts_TurretGun turretGun = new Parts_TurretGun();
                    turretGun.parent = this;
                    turretGun.parts_TurretGunDef = vehicleDef.vehicle.turretGunDefs[i];
                    turretGun.SpawnSetup();
                    turretGuns.Add(turretGun);
                }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.LookDeep<ThingContainer>(ref driverContainer, "driverContainer");
            Scribe_Collections.LookList<Parts_TurretGun>(ref turretGuns, "turretGuns", LookMode.Deep);
            Scribe_Values.LookValue<bool>(ref callDriver, "callDriver");
            Scribe_Values.LookValue<bool>(ref visibleInside, "visibleInside");
            Scribe_Values.LookValue<bool>(ref isStandby, "isStandby");
            Scribe_Values.LookValue<int>(ref autoDismountTick, "autoDismountTick");
            Scribe_Values.LookValue<int>(ref thresholdAutoDismount, "thresholdAutoDismount");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            Dismount();
            foreach (Parts_TurretGun turretGun in turretGuns)
                turretGun.Destroy(mode);
            //Thing dummy;

            //Drop resources
            /*foreach (ThingCount thingCount in def.costList)
            {
                Thing thing = ThingMaker.MakeThing(thingCount.thingDef);
                thing.stackCount = thingCount.count / 2;
                GenThing.TryDropAndSetForbidden(thing, this.Position, ThingPlaceMode.Near, out dummy, true);
            }*/
        }

        public override void Tick()
        {
            base.Tick();
            tickTime++;
            if (IsMounted)
            {
                //driver.Position = this.Position + driverOffset.ToIntVec3().RotatedBy(this.Rotation);
                driverContainer.ThingContainerTick();
                foreach (Parts_TurretGun turretGun in turretGuns)
                    turretGun.Tick();

                if (Driver.Downed || Driver.Dead || this.Downed || this.Dead)
                    this.Dismount();
            }
            if (this.Downed || this.Dead)
                this.UnboardAll();

            if (tickTime % tickPerTickRare == 0)
                this.TickRare();

        }

        public override void TickRare()
        {
            base.TickRare();

            if (IsMounted && (Driver.needs.food.CurCategory == HungerCategory.Starving || Driver.needs.rest.CurCategory == RestCategory.Exhausted))
                this.Dismount();
            //CheckCallDriver();
        }

        #endregion

        #region Thing graphic method
        // ==================================
        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            if (!Find.Selector.IsSelected(this))
                visibleInside = false;

            if (IsMounted && (vehicleDef.vehicle.driverVisible == PawnVisible.Always || (visibleInside && vehicleDef.vehicle.driverVisible == PawnVisible.IfSelected)))
            {
                Vector3 driverLoc = drawLoc; driverLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                Driver.Rotation = this.Rotation;
                Driver.DrawAt(drawLoc + vehicleDef.vehicle.driverOffset.RotatedBy(this.Rotation.AsAngle));
                Driver.DrawGUIOverlay();
            }
            if (vehicleDef.vehicle.boardableNum > 0 && this.inventory.container.Count(x => x is Pawn) > 0)
            {
                List<Thing> crews = this.inventory.container.Where(x => x is Pawn).ToList();
                for(int i = 0; i < crews.Count; i++)
                    if(vehicleDef.vehicle.crewsVisible[i] == PawnVisible.Always || (visibleInside && vehicleDef.vehicle.crewsVisible[i] == PawnVisible.IfSelected))
                    {
                        Vector3 crewLoc = drawLoc; crewLoc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
                        crews[i].Rotation = this.Rotation;
                        crews[i].DrawAt(crewLoc + vehicleDef.vehicle.crewsOffset[i].RotatedBy(this.Rotation.AsAngle));
                        crews[i].DrawGUIOverlay();
                    }
            }
            if (extraGraphics.NullOrEmpty())
                UpdateGraphics();

            IEnumerable<ExtraGraphicDef> partGraphicDefs = null;
            if(vehicleDef.vehicle.turretGunDefs != null)
                partGraphicDefs = vehicleDef.vehicle.turretGunDefs.Select(x => x.turretTopExtraGraphicDef);
            for (int i = 0; i < turretGuns.Count(); i++)
                if (partGraphicDefs != null && !(partGraphicDefs.ElementAt(i).InvisibleWhenSelected && visibleInside))
                    turretGuns[i].DrawAt(drawLoc + partGraphicDefs.ElementAt(i).drawingOffset.RotatedBy(this.Rotation.AsAngle));

            for (int i = 0; i < extraGraphics.Count(); i++)
                if (!(vehicleDef.vehicle.extraGraphicDefs[i].InvisibleWhenSelected && visibleInside))
                    extraGraphics[i].Draw(drawLoc + vehicleDef.vehicle.extraGraphicDefs[i].drawingOffset.RotatedBy(this.Rotation.AsAngle), this.Rotation, this);
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            foreach (Parts_TurretGun turretGun in turretGuns)
                turretGun.DrawExtraSelectionOverlays();
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            if (!GenText.NullOrEmpty(inspectString))
                stringBuilder.Append(inspectString);
            foreach (Parts_TurretGun turretGun in turretGuns)
                stringBuilder.Append(turretGun.GetInspectString());
            return stringBuilder.ToString();
        }

        private void UpdateGraphics()
        {
            extraGraphics = new List<Graphic_Single>();

            foreach (ExtraGraphicDef drawingDef in vehicleDef.vehicle.extraGraphicDefs)
            {
                Graphic_Single graphic = GraphicDatabase.Get<Graphic_Single>(drawingDef.graphicPath, def.graphic.Shader, def.graphic.drawSize, def.graphic.color, def.graphic.colorTwo) as Graphic_Single;
                extraGraphics.Add(graphic);
            }
        }
        #endregion

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(dinfo, out absorbed);
            foreach (Parts_TurretGun turretGun in turretGuns)
                turretGun.PreApplyDamage(dinfo, out absorbed);
            //Vehicle's brain is mounting point. If it damaged, driver is also damaged.
            //if (dinfo.Part.Value)
            //    dinfo.Part.Value.Injury.Body.
        }

        #region Thing Gizmo FloatingOptionMenu

        public override IEnumerable<Gizmo> GetGizmos()
        {
            //Hunt Gizmo is not needed.
            //foreach (var baseGizmo in base.GetGizmos())
            //    yield return baseGizmo;

            if (this.Faction == Faction.OfColony && IsMounted)
            {
                Command_Action dismountGizmo = new Command_Action();

                dismountGizmo.defaultLabel = "Dismount";
                dismountGizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconDismount");
                dismountGizmo.activateSound = SoundDef.Named("Click");
                dismountGizmo.defaultDesc = "Dismount";
                dismountGizmo.action = () => { this.Dismount(); };

                yield return dismountGizmo;

                Command_Toggle draftGizmo = new Command_Toggle();

                draftGizmo.hotKey = KeyBindingDefOf.CommandColonistDraft;
                draftGizmo.isActive = () => this.drafter.Drafted;
                draftGizmo.toggleAction = () =>
                {
                    this.drafter.Drafted = !this.drafter.Drafted;
                    ConceptDatabase.KnowledgeDemonstrated(ConceptDefOf.Drafting, KnowledgeAmount.SpecificInteraction);
                };
                draftGizmo.defaultDesc = Translator.Translate("CommandToggleDraftDesc");
                draftGizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft", true);
                draftGizmo.turnOnSound = SoundDef.Named("DraftOn");
                draftGizmo.turnOffSound = SoundDef.Named("DraftOff");
                draftGizmo.defaultLabel = (!this.drafter.Drafted) ? Translator.Translate("CommandDraftLabel") : Translator.Translate("CommandUndraftLabel");
                if (this.drafter.pawn.Downed)
                {
                    Command_Toggle commandToggle = draftGizmo;
                    string key = "IsIncapped";
                    object[] objArray = new object[1];
                    int index = 0;
                    string nameStringShort = this.drafter.pawn.NameStringShort;
                    objArray[index] = (object)nameStringShort;
                    string reason = Translator.Translate(key, objArray);
                    commandToggle.Disable(reason);
                }
                draftGizmo.tutorHighlightTag = "ToggleDrafted";

                yield return draftGizmo;

                Designator_Move designator = new Designator_Move();

                designator.vehicle = this;
                designator.defaultLabel = "Move";
                designator.defaultDesc = "Move vehicle";
                designator.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconMove");
                designator.activateSound = SoundDef.Named("Click");
                designator.hotKey = KeyBindingDefOf.Misc1;

                yield return designator;

                if (!turretGuns.NullOrEmpty())
                {
                    Designator_ForcedTarget designator2 = new Designator_ForcedTarget();

                    designator2.turretGuns = turretGuns;
                    designator2.defaultLabel = "Set forced target";
                    designator2.defaultDesc = "Set forced target";
                    designator2.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack");
                    designator2.activateSound = SoundDef.Named("Click");
                    designator2.hotKey = KeyBindingDefOf.Misc2;

                    yield return designator2;

                    Command_Action haltGizmo = new Command_Action();

                    haltGizmo.defaultLabel = "Stop forced target";
                    haltGizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt");
                    haltGizmo.activateSound = SoundDef.Named("Click");
                    haltGizmo.defaultDesc = "Stop forced target ";
                    haltGizmo.action = () => 
                    {
                        foreach (Parts_TurretGun turretGun in turretGuns)
                            turretGun.forcedTarget = null;
                    };

                    yield return haltGizmo;
                }
            }
            else if (!IsMounted && this.Faction == Faction.OfColony)
            {
                Designator_Mount designator = new Designator_Mount();

                designator.vehicle = this;
                designator.mountPos = MountPos;
                designator.defaultLabel = "Mount";
                designator.defaultDesc = "Mount";
                designator.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconMount");
                designator.activateSound = SoundDef.Named("Click");

                yield return designator;
            }
            else if (!IsMounted && this.Faction != Faction.OfColony)
            {
                Designator_Claim designatorClaim = new Designator_Claim();

                designatorClaim.vehicle = this;
                designatorClaim.defaultLabel = "Claim";
                designatorClaim.defaultDesc = "Claim";
                designatorClaim.icon = ContentFinder<Texture2D>.Get("UI/Commands/Claim");
                designatorClaim.activateSound = SoundDef.Named("Click");

                yield return designatorClaim;
            }

            if (this.Faction == Faction.OfColony && vehicleDef.vehicle.boardableNum > 0 && this.inventory.container.Count(x => x is Pawn) < vehicleDef.vehicle.boardableNum)
            {
                Designator_Board designatorBoard = new Designator_Board();

                designatorBoard.vehicle = this;
                designatorBoard.mountPos = MountPos;
                designatorBoard.defaultLabel = "Board";
                designatorBoard.defaultDesc = "Board";
                designatorBoard.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconBoard");
                designatorBoard.activateSound = SoundDef.Named("Click");

                yield return designatorBoard;
            }

            if (this.Faction == Faction.OfColony && vehicleDef.vehicle.boardableNum > 0 && this.inventory.container.Count(x => x is Pawn) > 0)
            {
                Command_Action commandUnboardAll = new Command_Action();

                commandUnboardAll.defaultLabel = "UnboardAll";
                commandUnboardAll.defaultDesc = "UnboardAll";
                commandUnboardAll.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconUnboardAll");
                commandUnboardAll.activateSound = SoundDef.Named("Click");
                commandUnboardAll.action = () => { this.UnboardAll(); };

                yield return commandUnboardAll;
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (myPawn.Faction != Faction.OfColony)
                yield break;

            foreach (FloatMenuOption fmo in base.GetFloatMenuOptions(myPawn))
                yield return fmo;

            if (this.Faction == myPawn.Faction)
            {
                // order to mount
                FloatMenuOption fmoMount = new FloatMenuOption();

                fmoMount.label = "Mount on " + this.LabelBase;
                fmoMount.priority = MenuOptionPriority.High;
                fmoMount.action = () =>
                {
                    Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("Mount"), this, MountPos);
                    myPawn.drafter.TakeOrderedJob(jobNew);
                };
                if (this.IsMounted)
                {
                    fmoMount.label = "Already mounted";
                    fmoMount.Disabled = true;
                }

                yield return fmoMount;

                // order to board
                FloatMenuOption fmoBoard = new FloatMenuOption();

                fmoBoard.label = "Board on " + this.LabelBase;
                fmoBoard.priority = MenuOptionPriority.High;
                fmoBoard.action = () =>
                {
                    Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("Board"), this, MountPos);
                    myPawn.drafter.TakeOrderedJob(jobNew);
                };
                if (vehicleDef.vehicle.boardableNum > 0 && this.inventory.container.Count(x => x is Pawn) >= vehicleDef.vehicle.boardableNum)
                {
                    fmoMount.label = "No space for boarding";
                    fmoMount.Disabled = true;
                }

                yield return fmoBoard;
            }
            else
            {
                FloatMenuOption fmoClaim = new FloatMenuOption();

                fmoClaim.label = "Claim " + this.LabelBase;
                fmoClaim.priority = MenuOptionPriority.High;
                fmoClaim.action = () =>
                {
                    Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("ClaimVehicle"), this);
                    myPawn.drafter.TakeOrderedJob(jobNew);
                };

                yield return fmoClaim;
            }
        }
        #endregion

    }
}