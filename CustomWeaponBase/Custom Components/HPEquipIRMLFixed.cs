using System;

public class HPEquipIRMLFixed : HPEquipIRML
{
	protected override void OnEquip()
	{
		irml = (IRMissileLauncher) ml;
		ml.parentActor = weaponManager.actor;
		if (ml.missilePrefab)
		{
			RadarCrossSection component = ml.missilePrefab.GetComponent<RadarCrossSection>();
			if (component)
			{
				perMissileRCS = component.GetAverageCrossSection();
			}
		}

		ml.OnFiredMissileIdx -= ShakeOnLaunch;
		ml.OnLoadMissile += Ml_OnLoadMissile;
		if (weaponManager.isPlayer && shakeMagnitude > 0f)
		{
			ml.OnFiredMissileIdx += ShakeOnLaunch;
		}

		if (ml.missiles != null)
		{
			foreach (Missile missile in ml.missiles)
			{
				if (missile)
				{
					missile.gameObject.name = ml.missilePrefab.name + " (" + weaponManager.actor.actorName + ")";
					if (dlz && dlz.missileDlzData == null && missile.dlzData)
					{
						dlz.missileDlzData = missile.dlzData;
					}
				}
			}
		}

		if (!trigUncageBoresightOnly)
		{
			EquipFunction equipFunction = new EquipFunction();
			EquipFunction equipFunction2 = equipFunction;
			equipFunction2.optionEvent = (EquipFunction.OptionEvent) Delegate.Combine(equipFunction2.optionEvent, new EquipFunction.OptionEvent(ToggleScanMode));
			equipFunction.optionName = s_seekMode;
			equipFunction.optionReturnLabel = seekerModeLabels[(int) seekerMode];
			EquipFunction equipFunction3 = new EquipFunction();
			EquipFunction equipFunction4 = equipFunction3;
			equipFunction4.optionEvent =
				(EquipFunction.OptionEvent) Delegate.Combine(equipFunction4.optionEvent, new EquipFunction.OptionEvent(ToggleTriggerUncage));
			equipFunction3.optionName = s_trigUncage;
			equipFunction3.optionReturnLabel = (triggerUncage ? s_ON : s_OFF);
			equipFunctions = new EquipFunction[]
			{
				equipFunction,
				equipFunction3
			};
		}
		else
		{
			while (seekerMode != HeatSeeker.SeekerModes.Uncaged)
			{
				ToggleScanMode();
			}

			if (!triggerUncage)
			{
				triggerUncage = true;
				foreach (Missile missile in ml.missiles)
				{
					if (missile)
					{
						missile.heatSeeker.manualUncage = triggerUncage;
						if (triggerUncage)
						{
							missile.heatSeeker.lockingRadar = null;
						}
						else
						{
							missile.heatSeeker.lockingRadar = weaponManager.lockingRadar;
						}
					}
				}
			}
		}

		ml.parentActor = weaponManager.actor;
		if (weaponManager.actor == FlightSceneManager.instance.playerActor)
		{
			irml.headTransform = VRHead.instance.transform;
		}

		irml.vssReferenceTransform = weaponManager.transform;
		ml.OnLoadMissile -= SetupMissile;
		ml.OnLoadMissile += SetupMissile;
		foreach (Missile m in ml.missiles)
		{
			SetupMissile(m);
		}

		if (externallyControlInternalBay)
		{
			foreach (InternalWeaponBay internalWeaponBay in weaponManager.internalWeaponBays)
			{
				if (internalWeaponBay.hardpointIdx == hardpointIdx)
				{
					iwb = internalWeaponBay;
					return;
				}
			}
		}
	}
}