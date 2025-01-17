﻿using System;
using KontrolSystem.TO2.Binding;
using KontrolSystem.TO2.Runtime;
using KSP.Api;
using KSP.Sim.impl;
using KSP.Sim.State;
using UnityEngine;

namespace KontrolSystem.KSP.Runtime.KSPControl {
    public partial class KSPControlModule {
        [KSClass("RCSTranslateManager")]
        public class RCSTranslateManager {
            private readonly IKSPContext context;
            private readonly VesselComponent vessel;
            private Func<double, Vector3d> translateProvider;
            private bool suspended;

            public RCSTranslateManager(IKSPContext context, VesselComponent vessel, Func<double, Vector3d> translateProvider) {
                this.context = context;
                this.vessel = vessel;
                this.translateProvider = translateProvider;

                this.context.HookAutopilot(this.vessel, UpdateAutopilot);
            }

            [KSField]
            public Vector3d Translate {
                get => translateProvider(0);
                set => translateProvider = _ => value;
            }

            [KSMethod]
            public void SetTranslateProvider(Func<double, Vector3d> newTranslateProvider) =>
                translateProvider = newTranslateProvider;

            [KSMethod]
            public Future<object> Release() {
                suspended = true;
                context.NextYield = new WaitForFixedUpdate();
                context.OnNextYieldOnce = () => {
                    context.UnhookAutopilot(vessel, UpdateAutopilot);
                };
                return new Future.Success<object>(null);
            }

            [KSMethod]
            public void Resume() {
                suspended = false;
                context.HookAutopilot(vessel, UpdateAutopilot);
            }

            public void UpdateAutopilot(ref FlightCtrlState c, float deltaT) {
                Vector3d translate = suspended ? Vector3d.zero : translateProvider(deltaT);
                c.X = (float)DirectBindingMath.Clamp(translate.x, -1, 1);
                c.Y = (float)DirectBindingMath.Clamp(translate.y, -1, 1);
                c.Z = (float)DirectBindingMath.Clamp(translate.z, -1, 1);
            }
        }
    }
}
