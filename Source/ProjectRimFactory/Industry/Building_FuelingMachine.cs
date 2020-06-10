﻿using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ProjectRimFactory.Industry
{
    public class Building_FuelingMachine : Building
    {
        public IntVec3 FuelableCell => Rotation.FacingCell + Position;
        public override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(10) && GetComp<CompPowerTrader>().PowerOn)
            {
                // Get what we are supposed to refuel:
                //   (only refuel one thing - if you need to adjust this to fuel more
                //    than one thing, make the loop here and put some breaking logic
                //    instead of all the "return;"s below)
                CompRefuelable refuelableComp=null;
                foreach (Thing tmpThing in Map.thingGrid.ThingsListAt(FuelableCell)) {
                    if (tmpThing is Building) refuelableComp=(tmpThing as Building).GetComp<CompRefuelable>();
                    if (refuelableComp != null) break;
                }
                if (refuelableComp != null) {
                    if (refuelableComp.Fuel >= refuelableComp.TargetFuelLevel) return; // fully fueled
                    foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(this))
                    {
                        List<Thing> l = Map.thingGrid.ThingsListAt(cell);
                        foreach (Thing item in Map.thingGrid.ThingsListAt(cell)) {
                            if (refuelableComp.Props.fuelFilter.Allows(item))
                            {
                                int num = Mathf.Min(item.stackCount, Mathf.CeilToInt(refuelableComp.TargetFuelLevel - refuelableComp.Fuel));
                                if (num > 0)
                                {
                                    refuelableComp.Refuel(num);
                                    item.SplitOff(num).Destroy();
                                }
                                if (refuelableComp.Fuel >= refuelableComp.TargetFuelLevel) return; // fully fueled
                            }
                        }
                    }
                }
            }
        }
        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(new List<IntVec3>(GenAdj.CellsAdjacent8Way(this)));
            GenDraw.DrawFieldEdges(new List<IntVec3>() { FuelableCell }, Color.yellow);
        }
    }
}
