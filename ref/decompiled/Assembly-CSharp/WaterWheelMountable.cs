using UnityEngine;

public class WaterWheelMountable : BaseMountable
{
	public static readonly Translate.Phrase NeedSustenance = new Translate.Phrase("waterwheel.needSustenance", "Too hungry to use...");

	public const Flags PlayerRunningInside = Flags.Reserved11;

	public float caloriesRequired = 10f;

	public float calorieDrainPerMinute = 10f;

	public float hydrationDrainPerMinute = 10f;

	private ElectricWaterWheel _waterWheel;

	private static readonly int PushingWaterwheel = Animator.StringToHash("pushingWaterWheel");

	private TimeSince lastToastWarning;

	private ElectricWaterWheel waterWheel
	{
		get
		{
			if (_waterWheel == null)
			{
				_waterWheel = GetParentEntity() as ElectricWaterWheel;
			}
			return _waterWheel;
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		bool flag = inputState.IsDown(BUTTON.FORWARD);
		bool flag2 = player.metabolism.calories.value < caloriesRequired;
		if (flag2 && (float)lastToastWarning > 2f)
		{
			player.ShowToast(GameTip.Styles.Red_Normal, NeedSustenance, false);
			lastToastWarning = 0f;
		}
		if (flag && !flag2)
		{
			player.metabolism.calories.value -= calorieDrainPerMinute / 60f * Time.deltaTime;
			player.metabolism.hydration.value -= hydrationDrainPerMinute / 60f * Time.deltaTime;
			player.metabolism.SendChanges();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, AnyMounted() && !flag2 && flag);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, b: false);
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (IsMountingFromOpenSide(player.transform.position))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public bool IsMountingFromOpenSide(Vector3 position)
	{
		return Vector3.Dot((position - mountAnchor.position).normalized, base.transform.forward) > 0.2f;
	}
}
