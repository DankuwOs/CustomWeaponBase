/*using System;
using Harmony;

[HarmonyPatch(typeof(HPEquipIRML), "OnEquip")]
    public class Patch_HPEquipIRML
    {
        public static bool Prefix(HPEquipIRML __instance)
        {
            __instance.irml = (IRMissileLauncher) __instance.ml;
            __instance.ml.parentActor = __instance.weaponManager.actor;
		    if (__instance.ml.missilePrefab)
		    {
			    RadarCrossSection component = __instance.ml.missilePrefab.GetComponent<RadarCrossSection>();
			    if (component)
			    {
				    __instance.perMissileRCS = component.GetAverageCrossSection();
			    }
		    }

		    __instance.ml.OnFiredMissileIdx -= __instance.ShakeOnLaunch;
		    __instance.ml.OnLoadMissile += __instance.Ml_OnLoadMissile;
		    if (__instance.weaponManager.isPlayer && __instance.shakeMagnitude > 0f)
		    {
			    __instance.ml.OnFiredMissileIdx += __instance.ShakeOnLaunch;
		    }

		    if (__instance.ml.missiles != null)
		    {
			    foreach (Missile missile in __instance.ml.missiles)
			    {
				    if (missile)
				    {
					    missile.gameObject.name = __instance.ml.missilePrefab.name + " (" + __instance.weaponManager.actor.actorName + ")";
					    if (__instance.dlz && __instance.dlz.missileDlzData == null && missile.dlzData)
					    {
						    __instance.dlz.missileDlzData = missile.dlzData;
					    }
				    }
			    }
		    }

		    if (!__instance.trigUncageBoresightOnly)
		    {
			    var traverse = Traverse.Create(__instance);
			    HPEquippable.EquipFunction equipFunction = new HPEquippable.EquipFunction();
			    HPEquippable.EquipFunction equipFunction2 = equipFunction;
			    equipFunction2.optionEvent = (HPEquippable.EquipFunction.OptionEvent) Delegate.Combine(equipFunction2.optionEvent, new HPEquippable.EquipFunction.OptionEvent(__instance.ToggleScanMode));
			    equipFunction.optionName = traverse.Field("s_seekMode").GetValue() as string;
			    equipFunction.optionReturnLabel = traverse.Field("seekerModeLabels").GetValue((int) traverse.Field("seekerMode").GetValue<int>()) as string;
			    HPEquippable.EquipFunction equipFunction3 = new HPEquippable.EquipFunction();
			    HPEquippable.EquipFunction equipFunction4 = equipFunction3;
			    equipFunction4.optionEvent =
				    (HPEquippable.EquipFunction.OptionEvent) Delegate.Combine(equipFunction4.optionEvent, new HPEquippable.EquipFunction.OptionEvent(__instance.ToggleTriggerUncage));
			    equipFunction3.optionName = traverse.Field("s_trigUncage").GetValue() as string;
			    equipFunction3.optionReturnLabel = (__instance.triggerUncage ? traverse.Field("s_ON").GetValue() : traverse.Field("s_OFF").GetValue()) as string;
			    __instance.equipFunctions = new HPEquippable.EquipFunction[]
			    {
				    equipFunction,
				    equipFunction3
			    };
		    }
		    else
		    {
			    while (__instance.seekerMode != HeatSeeker.SeekerModes.Uncaged)
			    {
				    this.ToggleScanMode();
			    }

			    if (!this.triggerUncage)
			    {
				    this.triggerUncage = true;
				    foreach (Missile missile in this.ml.missiles)
				    {
					    if (missile)
					    {
						    missile.heatSeeker.manualUncage = this.triggerUncage;
						    if (this.triggerUncage)
						    {
							    missile.heatSeeker.lockingRadar = null;
						    }
						    else
						    {
							    missile.heatSeeker.lockingRadar = base.weaponManager.lockingRadar;
						    }
					    }
				    }
			    }
		    }

		    this.ml.parentActor = base.weaponManager.actor;
		    if (base.weaponManager.actor == FlightSceneManager.instance.playerActor)
		    {
			    this.irml.headTransform = VRHead.instance.transform;
		    }

		    this.irml.vssReferenceTransform = base.weaponManager.transform;
		    this.ml.OnLoadMissile -= this.SetupMissile;
		    this.ml.OnLoadMissile += this.SetupMissile;
		    foreach (Missile m in this.ml.missiles)
		    {
			    this.SetupMissile(m);
		    }

		    if (this.externallyControlInternalBay)
		    {
			    foreach (InternalWeaponBay internalWeaponBay in base.weaponManager.internalWeaponBays)
			    {
				    if (internalWeaponBay.hardpointIdx == this.hardpointIdx)
				    {
					    this.iwb = internalWeaponBay;
					    return;
				    }
			    }
		    }
        }
    }*/