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
    public class Vehicle : ThingWithComps, IThingContainerOwner
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

        public Vehicle_PathFollower pather;

        //Data of mounting
        public ThingContainer driverContainer;
        public Vehicle_InventoryTracker inventory;

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

            //this.pather.StopDead();

            driverContainer.TryAdd(pawn);
            pawn.holder = this.GetContainer();
            callDriver = false;
        }
        public virtual void Dismount()
        {
            if (!this.IsMounted)
                return;

            this.pather.StopDead();

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
            callDriver = false;
            visibleInside = false;
            pather = new Vehicle_PathFollower(this);

            driverContainer = new ThingContainer(this, true);
            driverContainer.owner = this;
            inventory = new Vehicle_InventoryTracker(this);

            //def.race.corpseDef = ThingDefOf.Steel;
            vehicleDef = def as VehicleDef;

            //Work setting
            //this.drafter = new Pawn_DraftController(this);
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

            Scribe_Deep.LookDeep<Vehicle_PathFollower>(ref pather, "pather");
            Scribe_Deep.LookDeep<Vehicle_InventoryTracker>(ref inventory, "inventory");
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
            this.UnboardAll();
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
            this.inventory.InventoryTrackerTick();
            if (IsMounted)
            {
                //driver.Position = this.Position + driverOffset.ToIntVec3().RotatedBy(this.Rotation);
                driverContainer.ThingContainerTick();
                this.pather.PatherTick();
                foreach (Parts_TurretGun turretGun in turretGuns)
                    turretGun.Tick();

                if (Driver.Downed || Driver.Dead)
                    this.Dismount();
            }

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

        #region Gizmo, FloatingOptionMenu

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
                if (vehicleDef.vehicle.boardableNum > 0)
                {
                    FloatMenuOption fmoBoard = new FloatMenuOption();

                    fmoBoard.label = "Board on " + this.LabelBase;
                    fmoBoard.priority = MenuOptionPriority.High;
                    fmoBoard.action = () =>
                    {
                        Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("Board"), this, MountPos);
                        myPawn.drafter.TakeOrderedJob(jobNew);
                    };
                    if (this.inventory.container.Count(x => x is Pawn) >= vehicleDef.vehicle.boardableNum)
                    {
                        fmoMount.label = "No space for boarding";
                        fmoMount.Disabled = true;
                    }

                    yield return fmoBoard;
                }
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

        public int TicksPerMove(bool diagonal)
        {
            float statValue = StatExtension.GetStatValue((Thing)this, StatDefOf.MoveSpeed, true);
            float f = 1f / (statValue / 60f);
            if (!Find.RoofGrid.Roofed(this.Position))
                f /= Find.WeatherManager.CurMoveSpeedMultiplier;
            if (diagonal)
                f *= 1.41421f;
            return Mathf.Clamp(Mathf.RoundToInt(f), 1, 450);
        }
    }

    //This code source is DLL decompiler
    public class Vehicle_InventoryTracker : IExposable, IThingContainerOwner
    {
        public ThingWithComps thing;
        public ThingContainer container;

        public Vehicle_InventoryTracker(ThingWithComps thing)
        {
            this.thing = thing;
            this.container = new ThingContainer((IThingContainerOwner) this, false);
        }

        public void ExposeData()
        {
            Scribe_Deep.LookDeep<ThingContainer>(ref container, "container");
        }

        public void InventoryTrackerTick()
        {
            this.container.ThingContainerTick();
        }

        public void DropAllNearThing(IntVec3 pos, bool forbid = false)
        {
            while (this.container.Count > 0)
            {
                Thing lastResultingThing;
                this.container.TryDrop(this.container[0], pos, ThingPlaceMode.Near, out lastResultingThing);
                if (forbid && lastResultingThing != null)
                    ForbidUtility.SetForbiddenIfOutsideHomeArea(lastResultingThing);
            }
        }

        public ThingContainer GetContainer()
        {
            return this.container;
        }

        public IntVec3 GetPosition()
        {
            return this.thing.Position;
        }
    }

    //This code source is DLL decompiler
    public class Vehicle_PathFollower : IExposable
    {
        private const int MaxMoveTicks = 450;
        private const int MaxCheckAheadNodes = 20;
        private const float SnowReductionFromWalking = 0.005f;
        private const int ClamorCellsInterval = 10;
        private const int MinTicksWalk = 50;
        private const int MinTicksAmble = 60;
        protected ThingWithComps thing;
        private bool moving;
        public IntVec3 nextCell;
        private IntVec3 lastCell;
        public int ticksUntilMove;
        public int totalMoveDuration;
        private int cellsUntilClamor;
        private TargetInfo destination;
        private PathEndMode peMode;
        public PawnPath curPath;
        public IntVec3 lastPathedTargetPosition;

        public TargetInfo Destination
        {
            get
            {
            return this.destination;
            }
        }

        public bool Moving
        {
            get
            {
            return this.moving;
            }
        }

        public Vehicle_PathFollower(ThingWithComps thing)
        {
            this.totalMoveDuration = 1;
            this.thing = thing;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<bool>(ref this.moving, "moving", true, false);
            Scribe_Values.LookValue<IntVec3>(ref this.nextCell, "nextCell", new IntVec3(), false);
            Scribe_Values.LookValue<int>(ref this.ticksUntilMove, "ticksUntilMove", 0, false);
            Scribe_Values.LookValue<int>(ref this.totalMoveDuration, "totalMoveDuration", 0, false);
            Scribe_Values.LookValue<PathEndMode>(ref this.peMode, "peMode", PathEndMode.None, false);
            if (this.moving)
            {
                if (this.destination.Thing != null && this.destination.Thing.Destroyed)
                {
                    Log.Error("Saved while " + this.thing + " was moving towards destroyed thing " + this.destination.Thing + " with job ");
                }
                Scribe_TargetInfo.LookTargetInfo(ref this.destination, "destination");
            }
            if (Scribe.mode != LoadSaveMode.PostLoadInit || Game.Mode == GameMode.Entry)
                return;
            if (this.moving)
                this.StartPath(this.destination, this.peMode);
            //Find.PawnDestinationManager.ReserveDestinationFor(this.thing, this.destination.Cell);
        }

        public void Notify_Teleported_Int()
        {
            this.StopDead();
            this.ResetToCurrentPosition();
        }

        public void ResetToCurrentPosition()
        {
            this.nextCell = this.thing.Position;
        }

        private bool PawnCanOccupy(IntVec3 c)
        {
            if (!GenGrid.Walkable(c))
            return false;
            Building edifice = GridsUtility.GetEdifice(c);
            if (edifice != null)
            {
            Building_Door buildingDoor = edifice as Building_Door;
            if (buildingDoor != null && !MachinesLike(buildingDoor.Faction, this.thing) && !buildingDoor.Open)
                return false;
            }
            return true;
        }

        public void StartPath(TargetInfo dest, PathEndMode peMode)
        {
            GenPath.ResolvePathMode(ref dest, ref peMode);
            if (!this.PawnCanOccupy(this.thing.Position) && !this.TryRecoverFromUnwalkablePosition(dest) || this.moving && this.curPath != null && (this.destination == dest && this.peMode == peMode))
                return;
            if (!Reachability.CanReach(this.thing.Position, dest, peMode, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
            {
                Log.Warning(this.thing.ThingID + "(" + this.thing.LabelCap + ") at " + this.thing.Position + " tried to path to unreachable dest " + dest
                     + ". This should have been checked before trying to path. (peMode=" + peMode + ")");
                this.PatherArrived();
            }
            else
            {
            if (!GenGrid.Walkable(this.nextCell))
                this.nextCell = this.thing.Position;
            this.peMode = peMode;
            this.destination = dest;
            //if (!this.destination.HasThing && Find.PawnDestinationManager.DestinationReservedFor(this.thing) != this.destination.Cell)
            //    Find.PawnDestinationManager.UnreserveAllFor(this.thing);
            if (this.AtDestinationPosition())
                this.PatherArrived();
            else
            {
                if (this.curPath != null)
                this.curPath.ReleaseToPool();
                this.curPath = (PawnPath) null;
                this.moving = true;
            }
            }
        }

        public void StopDead()
        {
            if (this.curPath != null)
            this.curPath.ReleaseToPool();
            this.curPath = (PawnPath) null;
            this.moving = false;
            this.nextCell = this.thing.Position;
        }

        public void PatherTick()
        {
            if (this.ticksUntilMove > 0)
            {
            --this.ticksUntilMove;
            }
            else
            {
            if (!this.moving)
                return;
            this.TryEnterNextPathCell();
            }
        }

        public Thing BuildingBlockingNextPathCell()
        {
            Thing thing = (Thing) GridsUtility.GetEdifice(this.nextCell);
            if (thing != null && ((((Building_Door)thing).Open)? false : !MachinesLike(thing.Faction, this.thing)))
            return thing;
            return (Thing) null;
        }

        public Building_Door NextCellDoorToManuallyOpen()
        {
            Building_Door buildingDoor = Find.ThingGrid.ThingAt<Building_Door>(this.nextCell);
            if (buildingDoor != null && buildingDoor.SlowsPawns && (!buildingDoor.Open && MachinesLike(buildingDoor.Faction, this.thing)))
                return buildingDoor;
            return (Building_Door) null;
        }

        public void PatherDraw()
        {
            if (!DebugViewSettings.drawPaths || this.curPath == null || !Find.Selector.IsSelected((object)this.thing))
                return;
            if (!this.curPath.Found)
                return;
            float num = Altitudes.AltitudeFor(AltitudeLayer.Item);
            if (this.curPath.NodesLeftCount <= 0)
                return;
            for (int nodesAhead = 0; nodesAhead < this.curPath.NodesLeftCount - 1; ++nodesAhead)
            {
                Vector3 A = this.curPath.Peek(nodesAhead).ToVector3Shifted();
                A.y = num;
                Vector3 B = this.curPath.Peek(nodesAhead + 1).ToVector3Shifted();
                B.y = num;
                GenDraw.DrawLineBetween(A, B);
            }
            if (this.thing == null)
                return;
            Vector3 drawPos = this.thing.DrawPos;
            drawPos.y = num;
            Vector3 B1 = this.curPath.Peek(0).ToVector3Shifted();
            B1.y = num;
            if ((double) (drawPos - B1).sqrMagnitude <= 0.00999999977648258)
                return;
            GenDraw.DrawLineBetween(drawPos, B1);
        }

        private bool TryRecoverFromUnwalkablePosition(TargetInfo originalDest)
        {
            bool flag = false;
            for (int index1 = 0; index1 < GenRadial.RadialPattern.Length; ++index1)
            {
                IntVec3 c = this.thing.Position + GenRadial.RadialPattern[index1];
                if (this.PawnCanOccupy(c))
                {
                    Log.Warning(this.thing + " on unwalkable cell " + this.thing.Position + ". Teleporting to " + c);
                    this.thing.Position = c;
                    this.moving = false;
                    this.nextCell = this.thing.Position;
                    this.StartPath(originalDest, this.peMode);
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Log.Error(this.thing + " on unwalkable cell " + this.thing.Position + ". Could not find walkable position nearby. Destroyed.");
            }
            return flag;
        }

        private void PatherArrived()
        {
            this.StopDead();
            //if (this.thing.jobs.curJob == null)
            //    return;
            //this.thing.jobs.curDriver.Notify_PatherArrived();
        }

        private void PatherFailed()
        {
            this.StopDead();
            //this.thing.jobs.curDriver.Notify_PatherFailed();
        }

        private void TryEnterNextPathCell()
        {
            Thing b = this.BuildingBlockingNextPathCell();
            if (b != null)
            {
                Building_Door buildingDoor = b as Building_Door;
                if (buildingDoor == null || !buildingDoor.FreePassage)
                {
                    /*if (this.thing.CurJob != null && this.thing.CurJob.canBash || GenHostility.HostileTo((Thing)this.thing, b))
                    {
                        Job newJob = new Job(JobDefOf.AttackMelee, (TargetInfo) b);
                        newJob.expiryInterval = 1100;
                        this.thing.jobs.StartJob(newJob, JobCondition.Incompletable, (ThinkNode)null, false, true);
                        return;
                    }*/
                    this.PatherFailed();
                    return;
                }
            }
            Building_Door buildingDoor1 = this.NextCellDoorToManuallyOpen();
            if (buildingDoor1 != null)
            {
                Stance_Cooldown stanceCooldown = new Stance_Cooldown(buildingDoor1.TicksToOpenNow, (TargetInfo) ((Thing) buildingDoor1));
                stanceCooldown.neverAimWeapon = true;
                //this.thing.stances.SetStance((Stance)stanceCooldown);
                buildingDoor1.StartManualOpenBy((Pawn)this.thing);
            }
            else
            {
                this.lastCell = this.thing.Position;
                this.thing.Position = this.nextCell;
                /*if (this.thing.RaceProps.Humanlike)
                {
                    --this.cellsUntilClamor;
                    if (this.cellsUntilClamor <= 0)
                    {
                    GenClamor.DoClamor(this.thing.Position, 7f);
                    this.cellsUntilClamor = 10;
                    }
                }
                this.thing.filth.Notify_EnteredNewCell();
                if ((double)this.thing.BodySize > 0.899999976158142)
                    Find.SnowGrid.AddDepth(this.thing.Position, -0.005f);*/
                Building_Door buildingDoor2 = Find.ThingGrid.ThingAt<Building_Door>(this.lastCell);
                if (buildingDoor2 != null && !buildingDoor2.CloseBlocked && !GenHostility.HostileTo((Thing)this.thing, (Thing)buildingDoor2))
                {
                    buildingDoor2.FriendlyTouched();
                    if (!buildingDoor2.HoldOpen && buildingDoor2.SlowsPawns)
                    {
                        buildingDoor2.StartManualCloseBy((Pawn)this.thing);
                        return;
                    }
                }
                if (this.NeedNewPath() && !this.TrySetNewPath())
                    return;
                if (this.AtDestinationPosition())
                    this.PatherArrived();
                else if (this.curPath.NodesLeftCount == 0)
                {
                    Log.Error((string)(object)this.thing + (object)" ran out of path nodes. Force-arriving.");
                    this.PatherArrived();
                }
                else
                    this.SetupMoveIntoNextCell();
            }
        }

        private void SetupMoveIntoNextCell()
        {
            if (this.curPath.NodesLeftCount < 2)
            {
                Log.Error(this.thing + " at " + this.thing.Position + " ran out of path nodes while pathing to " + this.destination + ".");
                this.PatherFailed();
            }
            else
            {
                this.nextCell = this.curPath.ConsumeNextNode();
                if (!GenGrid.Walkable(this.nextCell))
                {
                    Log.Error(this.thing + " entering " + this.nextCell + " which is unwalkable.");
                }
                Building_Door buildingDoor = Find.ThingGrid.ThingAt<Building_Door>(this.nextCell);
                if (buildingDoor != null)
                    buildingDoor.Notify_PawnApproaching((Pawn)this.thing);
                int num = this.TicksToMoveIntoCell(this.nextCell);
                this.totalMoveDuration = num;
                this.ticksUntilMove = num;
            }
        }

        private int TicksToMoveIntoCell(IntVec3 c)
        {
            int num = (c.x == this.thing.Position.x || c.z == this.thing.Position.z ? ((Vehicle)this.thing).TicksPerMove(false) : ((Vehicle)this.thing).TicksPerMove(true)) + PathGrid.CalculatedCostAt(c, false);
            Building edifice = GridsUtility.GetEdifice(c);
            if (edifice != null)
                num += (int)0;
            if (num > 450)
                num = 450;
            /*if (this.thing.jobs.curJob != null)
            {
                switch (this.thing.jobs.curJob.locomotionUrgency)
                {
                    case LocomotionUrgency.Amble:
                    num *= 3;
                    if (num < 60)
                    {
                        num = 60;
                        break;
                    }
                    break;
                    case LocomotionUrgency.Walk:
                    num *= 2;
                    if (num < 50)
                    {
                        num = 50;
                        break;
                    }
                    break;
                    case LocomotionUrgency.Jog:
                    num *= 1;
                    break;
                    case LocomotionUrgency.Sprint:
                    num = Mathf.RoundToInt((float) num * 0.75f);
                    break;
                }
            }*/
            return num;
        }

        private bool TrySetNewPath()
        {
            PawnPath pawnPath = this.GenerateNewPath();
            if (!pawnPath.Found)
            {
            this.PatherFailed();
            return false;
            }
            if (this.curPath != null)
            this.curPath.ReleaseToPool();
            this.curPath = pawnPath;
            return true;
        }

        private PawnPath GenerateNewPath()
        {
            this.lastPathedTargetPosition = this.destination.Cell;
            return PathFinder.FindPath(this.thing.Position, this.destination, TraverseParms.For(TraverseMode.PassAnything, Danger.Deadly), this.peMode);
        }

        private bool AtDestinationPosition()
        {
            if (this.thing.Position == this.destination.Cell)
            return true;
            if (this.peMode == PathEndMode.Touch)
            {
            if (!this.destination.HasThing)
            {
                if (GenAdj.AdjacentTo8WayOrInside(this.thing.Position, this.destination.Cell))
                return true;
            }
            else if (GenAdj.AdjacentTo8WayOrInside(this.thing.Position, this.destination.Thing))
                return true;
            }
            return false;
        }

        private bool NeedNewPath()
        {
            if (this.curPath == null || !this.curPath.Found || this.curPath.NodesLeftCount == 0)
            return true;
            if (this.lastPathedTargetPosition != this.destination.Cell)
            {
                float horizontalSquared = (this.thing.Position - this.destination.Cell).LengthHorizontalSquared;
                float num = (double) horizontalSquared <= 900.0 ? ((double) horizontalSquared <= 289.0 ? ((double) horizontalSquared <= 100.0 ? ((double) horizontalSquared <= 49.0 ? 0.5f : 2f) : 3f) : 5f) : 10f;
                if ((double) (this.lastPathedTargetPosition - this.destination.Cell).LengthHorizontalSquared > (double) num * (double) num)
                    return true;
            }
            for (int nodesAhead = 0; nodesAhead < 20 && nodesAhead < this.curPath.NodesLeftCount; ++nodesAhead)
            {
                IntVec3 c = this.curPath.Peek(nodesAhead);
                if (!GenGrid.Walkable(c))
                    return true;
                Building_Door buildingDoor = GridsUtility.GetEdifice(c) as Building_Door;
                if (buildingDoor != null && (!((!buildingDoor.FreePassage)? MachinesLike(buildingDoor.Faction, this.thing) : true) && !GenHostility.HostileTo((Thing)this.thing, (Thing)buildingDoor) || ForbidUtility.IsForbidden((Thing)buildingDoor, this.thing.Faction)))
                    return true;
            }
            return false;
        }

        private bool MachinesLike(Faction machineFaction, Thing thing)
        {
            return thing.Faction != null && !FactionUtility.HostileTo(thing.Faction, machineFaction);
        }


    }
}