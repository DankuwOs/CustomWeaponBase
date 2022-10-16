using System;

public class HPEquipIRMLFixed : HPEquipIRML
{
	protected override void OnEquip()
	{
		this.irml = (IRMissileLauncher) this.ml;
		this.ml.parentActor = base.weaponManager.actor;
		if (this.ml.missilePrefab)
		{
			RadarCrossSection component = this.ml.missilePrefab.GetComponent<RadarCrossSection>();
			if (component)
			{
				this.perMissileRCS = component.GetAverageCrossSection();
			}
		}

		this.ml.OnFiredMissileIdx -= this.ShakeOnLaunch;
		this.ml.OnLoadMissile += this.Ml_OnLoadMissile;
		if (base.weaponManager.isPlayer && this.shakeMagnitude > 0f)
		{
			this.ml.OnFiredMissileIdx += this.ShakeOnLaunch;
		}

		if (this.ml.missiles != null)
		{
			foreach (Missile missile in this.ml.missiles)
			{
				if (missile)
				{
					missile.gameObject.name = this.ml.missilePrefab.name + " (" + base.weaponManager.actor.actorName + ")";
					if (base.dlz && base.dlz.missileDlzData == null && missile.dlzData)
					{
						base.dlz.missileDlzData = missile.dlzData;
					}
				}
			}
		}

		if (!this.trigUncageBoresightOnly)
		{
			HPEquippable.EquipFunction equipFunction = new HPEquippable.EquipFunction();
			HPEquippable.EquipFunction equipFunction2 = equipFunction;
			equipFunction2.optionEvent = (HPEquippable.EquipFunction.OptionEvent) Delegate.Combine(equipFunction2.optionEvent, new HPEquippable.EquipFunction.OptionEvent(this.ToggleScanMode));
			equipFunction.optionName = this.s_seekMode;
			equipFunction.optionReturnLabel = this.seekerModeLabels[(int) this.seekerMode];
			HPEquippable.EquipFunction equipFunction3 = new HPEquippable.EquipFunction();
			HPEquippable.EquipFunction equipFunction4 = equipFunction3;
			equipFunction4.optionEvent =
				(HPEquippable.EquipFunction.OptionEvent) Delegate.Combine(equipFunction4.optionEvent, new HPEquippable.EquipFunction.OptionEvent(this.ToggleTriggerUncage));
			equipFunction3.optionName = this.s_trigUncage;
			equipFunction3.optionReturnLabel = (this.triggerUncage ? this.s_ON : this.s_OFF);
			this.equipFunctions = new HPEquippable.EquipFunction[]
			{
				equipFunction,
				equipFunction3
			};
		}
		else
		{
			while (this.seekerMode != HeatSeeker.SeekerModes.Uncaged)
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
}