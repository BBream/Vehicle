using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;


namespace Vehicle
{
    public class VehicleDef : ThingDef
    {
        public VehiclProperties vehicle;
    }

    public class VehiclProperties
    {
        //Data of mounting
        public IntVec3           mountPosOffset;
        public BodyPartDef       mountedPart;
        public int               boardableNum;

        //Data of Parts
        public List<Parts_TurretGunDef> turretGunDefs;
        //public List<Parts_Component> component;

        //draw setting
        public Vector3           driverOffset;
        public PawnVisible     driverVisible;
        public List<Vector3>     crewsOffset;
        public List<PawnVisible> crewsVisible;
        public List<ExtraGraphicDef> extraGraphicDefs;

        //vehicle work setting
        public List<JobGiverDef> jobGiverDefs;
    }

    public enum PawnVisible
    {
        Always = 2,
        IfSelected = 1,
        Never = 0
    }

    public class Parts_TurretGunDef
    {
        public ExtraGraphicDef turretTopExtraGraphicDef;
        public Vector3 partsOffset;
        public ThingDef turretGunDef;
        public ThingDef turretShellDef;
        public int turretBurstWarmupTicks;
        public int turretBurstCooldownTicks;
    }

    public class ExtraGraphicDef
    {
        public string         graphicPath;
        public Vector3        drawingOffset;
        public bool           InvisibleWhenSelected;
    }

    public class JobGiverDef
    {
        public Type giverClass;
        public int priorityInOrder;

        private ThinkNode_JobGiver jobGiverInt;

        public ThinkNode_JobGiver JobGiver
        {
            get
            {
                if (jobGiverInt == null && this.giverClass != null)
                    jobGiverInt = (ThinkNode_JobGiver)Activator.CreateInstance(this.giverClass);
                return jobGiverInt;
            }
        }
    }
}
