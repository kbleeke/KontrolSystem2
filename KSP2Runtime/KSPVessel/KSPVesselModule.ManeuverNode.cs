﻿using System.Collections.Generic;
using KontrolSystem.KSP.Runtime.KSPOrbit;
using KontrolSystem.TO2.Binding;
using KontrolSystem.TO2.Runtime;
using KSP.Sim.Maneuver;

namespace KontrolSystem.KSP.Runtime.KSPVessel {
    public partial class KSPVesselModule {
        [KSClass("ManeuverNode")]
        public class ManeuverNodeAdapter {
            private readonly VesselAdapter vesselAdapter;
            private readonly ManeuverNodeData maneuverNode;

            public ManeuverNodeAdapter(VesselAdapter vesselAdapter, ManeuverNodeData maneuverNode) {
                this.vesselAdapter = vesselAdapter;
                this.maneuverNode = maneuverNode;
            }
            
            [KSField]
            public double Time {
                get => maneuverNode.Time;
                set =>  UpdateNode(maneuverNode.BurnVector.x, maneuverNode.BurnVector.y, maneuverNode.BurnVector.z, value);
            }

            [KSField]
            public double Prograde {
                get => maneuverNode.BurnVector.z;
                set =>  UpdateNode(maneuverNode.BurnVector.x, maneuverNode.BurnVector.y, value, maneuverNode.Time);
            }

            [KSField]
            public double Normal {
                get => maneuverNode.BurnVector.y;
                set => UpdateNode(maneuverNode.BurnVector.x, value, maneuverNode.BurnVector.z, maneuverNode.Time);
            }

            [KSField]
            public double RadialOut {
                get => maneuverNode.BurnVector.x;
                set => UpdateNode(value, maneuverNode.BurnVector.y, maneuverNode.BurnVector.z, maneuverNode.Time);
            }

            [KSField("ETA")]
            public double Eta {
                get => maneuverNode.Time - vesselAdapter.context.UniversalTime;
                set => UpdateNode(maneuverNode.BurnVector.x, maneuverNode.BurnVector.y, maneuverNode.BurnVector.z,
                    value + vesselAdapter.context.UniversalTime);
            }

            [KSField]
            public Vector3d BurnVector {
                get {
                    KSPOrbitModule.IOrbit orbit = new OrbitWrapper(vesselAdapter.context, vesselAdapter.vessel.Orbit);
                    return orbit.RadialPlus(maneuverNode.Time) * maneuverNode.BurnVector.x +
                           orbit.NormalPlus(maneuverNode.Time) * maneuverNode.BurnVector.y +
                           orbit.Prograde(maneuverNode.Time) * maneuverNode.BurnVector.z;
                }
                set {
                    KSPOrbitModule.IOrbit orbit = new OrbitWrapper(vesselAdapter.context, vesselAdapter.vessel.Orbit);
                    UpdateNode(
                        Vector3d.Dot(orbit.RadialPlus(maneuverNode.Time), value),
                        Vector3d.Dot(orbit.NormalPlus(maneuverNode.Time), value),
                        Vector3d.Dot(orbit.Prograde(maneuverNode.Time), value),
                        maneuverNode.Time);
                }
            }

            [KSField] public double BurnDuration => maneuverNode.BurnDuration;

            [KSField]
            public Option<KSPOrbitModule.IOrbit> ExpectedOrbit {
                get {
                    foreach (var patchedOrbit in vesselAdapter.vessel.Orbiter.PatchedConicSolver.CurrentTrajectory) {
                        if (patchedOrbit.StartUT > maneuverNode.Time) {
                            return new Option<KSPOrbitModule.IOrbit>(new OrbitWrapper(vesselAdapter.context, patchedOrbit));
                        }                        
                    }
                    return new Option<KSPOrbitModule.IOrbit>();
                }
            }
            
            [KSMethod]
            public void Remove() {
                vesselAdapter.vessel.Game.SpaceSimulation.Maneuvers.RemoveNodesFromVessel(vesselAdapter.vessel.GlobalId, new List<ManeuverNodeData>() { maneuverNode });
//                vesselAdapter.vessel.SimulationObject.ManeuverPlan.RemoveNode(maneuverNode, false);
            }

            private void UpdateNode(double radialOut, double normal, double prograde, double ut) {
                maneuverNode.Time = ut;
                maneuverNode.BurnVector = new Vector3d(radialOut, normal, prograde);
                vesselAdapter.vessel.SimulationObject.ManeuverPlan.UpdateNodeDetails(maneuverNode);
            }
        }
    }
}
