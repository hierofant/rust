using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class ElevatorStatic : Elevator
{
	public enum ElevatorFloorNumber
	{
		None,
		Basement,
		Lobby,
		Floor2,
		Floor3,
		Floor4,
		Floor5,
		Floor6,
		Floor7,
		Floor8,
		Penthouse,
		Floor0,
		Floor1,
		FloorMinus1
	}

	public bool StaticTop;

	public const Flags LiftRecentlyArrived = Flags.Reserved3;

	public List<EntityRef<ElevatorStatic>> clientFloorPositions = new List<EntityRef<ElevatorStatic>>();

	public List<ElevatorStatic> floorPositions = new List<ElevatorStatic>();

	public ElevatorStatic ownerElevator;

	public ElevatorFloorNumber FloorNumberDisplay;

	public static Translate.Phrase Phrase_Basement = new Translate.Phrase("elevator_floor_option_basement", "Basement");

	public static Translate.Phrase Phrase_Elevator = new Translate.Phrase("elevator_floor_option_elevator", "Elevator");

	public static Translate.Phrase Phrase_Lobby = new Translate.Phrase("elevator_floor_option_lobby", "Lobby");

	public static Translate.Phrase Phrase_Penthouse = new Translate.Phrase("elevator_floor_option_penthouse", "Penthouse");

	public static Translate.Phrase Phrase_GroundFloor = new Translate.Phrase("elevator_floor_option_ground", "Ground Floor");

	public static Translate.Phrase Phrase_Floor1 = new Translate.Phrase("elevator_floor_option_1", "Floor 1");

	public static Translate.Phrase Phrase_Floor2 = new Translate.Phrase("elevator_floor_option_2", "Floor 2");

	public static Translate.Phrase Phrase_Floor3 = new Translate.Phrase("elevator_floor_option_3", "Floor 3");

	public static Translate.Phrase Phrase_Floor4 = new Translate.Phrase("elevator_floor_option_4", "Floor 4");

	public static Translate.Phrase Phrase_Floor5 = new Translate.Phrase("elevator_floor_option_5", "Floor 5");

	public static Translate.Phrase Phrase_Floor6 = new Translate.Phrase("elevator_floor_option_6", "Floor 6");

	public static Translate.Phrase Phrase_Floor7 = new Translate.Phrase("elevator_floor_option_7", "Floor 7");

	public static Translate.Phrase Phrase_Floor8 = new Translate.Phrase("elevator_floor_option_8", "Floor 8");

	public static Dictionary<ElevatorFloorNumber, (string icon, Translate.Phrase phrase)> IconLookup = new Dictionary<ElevatorFloorNumber, (string, Translate.Phrase)>
	{
		{
			ElevatorFloorNumber.None,
			("elevator", Phrase_Elevator)
		},
		{
			ElevatorFloorNumber.Basement,
			("elevator_b", Phrase_Basement)
		},
		{
			ElevatorFloorNumber.Lobby,
			("elevator_l", Phrase_Lobby)
		},
		{
			ElevatorFloorNumber.Penthouse,
			("elevator_p", Phrase_Penthouse)
		},
		{
			ElevatorFloorNumber.Floor2,
			("elevator_2", Phrase_Floor2)
		},
		{
			ElevatorFloorNumber.Floor3,
			("elevator_3", Phrase_Floor3)
		},
		{
			ElevatorFloorNumber.Floor4,
			("elevator_4", Phrase_Floor4)
		},
		{
			ElevatorFloorNumber.Floor5,
			("elevator_5", Phrase_Floor5)
		},
		{
			ElevatorFloorNumber.Floor6,
			("elevator_6", Phrase_Floor6)
		},
		{
			ElevatorFloorNumber.Floor7,
			("elevator_7", Phrase_Floor7)
		},
		{
			ElevatorFloorNumber.Floor8,
			("elevator_8", Phrase_Floor8)
		},
		{
			ElevatorFloorNumber.Floor1,
			("elevator_1", Phrase_Floor1)
		},
		{
			ElevatorFloorNumber.Floor0,
			("elevator_0", Phrase_Lobby)
		},
		{
			ElevatorFloorNumber.FloorMinus1,
			("elevator_minus1", Phrase_Basement)
		}
	};

	public override bool IsStatic => true;

	public IReadOnlyList<ElevatorStatic> FloorPositions => floorPositions;

	public ElevatorStatic TopElevator
	{
		get
		{
			if (!StaticTop)
			{
				return ownerElevator;
			}
			return this;
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
			flagsUpdateScope.Set(Flags.Reserved1, StaticTop);
		}
		if (!Rust.Application.isLoadingSave)
		{
			UpdateFloorPositions();
		}
	}

	private void UpdateFloorPositions()
	{
		if (!base.IsTop)
		{
			return;
		}
		floorPositions.Clear();
		List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(base.transform.position, -Vector3.up), 0f, obj, 200f, 262144, QueryTriggerInteraction.Collide);
		foreach (RaycastHit item in obj)
		{
			if (item.transform.parent != null)
			{
				ElevatorStatic component = item.transform.parent.GetComponent<ElevatorStatic>();
				if (!(component == null) && !(component == this) && !component.isClient && !component.IsDestroyed)
				{
					floorPositions.Add(component);
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		floorPositions.Reverse();
		base.Floor = floorPositions.Count;
		for (int i = 0; i < floorPositions.Count; i++)
		{
			floorPositions[i].SetFloorDetails(i, this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(UpdateFloorPositions, 1f);
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		UpdateChildEntities(base.IsTop);
	}

	public override bool IsValidFloor(int targetFloor)
	{
		if (targetFloor >= 0)
		{
			return targetFloor <= base.Floor;
		}
		return false;
	}

	public override Vector3 GetWorldSpaceFloorPosition(int targetFloor)
	{
		if (targetFloor == base.Floor)
		{
			return base.transform.position + Vector3.up * 1f;
		}
		Vector3 position = base.transform.position;
		position.y = floorPositions[targetFloor].transform.position.y + 1f;
		return position;
	}

	public void SetFloorDetails(int floor, ElevatorStatic owner)
	{
		ownerElevator = owner;
		base.Floor = floor;
	}

	public override void CallElevator()
	{
		if (ownerElevator != null)
		{
			ownerElevator.RequestMoveLiftTo(base.Floor, out var _, this);
		}
		else if (base.IsTop)
		{
			RequestMoveLiftTo(base.Floor, out var _, this);
		}
	}

	public ElevatorStatic ElevatorAtFloor(int floor)
	{
		if (floor == base.Floor)
		{
			return this;
		}
		if (floor >= 0 && floor < floorPositions.Count)
		{
			return floorPositions[floor];
		}
		return null;
	}

	protected override void OpenDoorsAtFloor(int floor)
	{
		base.OpenDoorsAtFloor(floor);
		if (floor == floorPositions.Count)
		{
			OpenLiftDoors();
		}
		else
		{
			floorPositions[floor].OpenLiftDoors();
		}
	}

	public override void OnMoveBegin()
	{
		base.OnMoveBegin();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if (elevatorStatic != null)
		{
			elevatorStatic.OnLiftLeavingFloor();
		}
		NotifyLiftEntityDoorsOpen(state: false);
	}

	public void OnLiftLeavingFloor()
	{
		ClearPowerOutput();
		if (IsInvoking(ClearPowerOutput))
		{
			CancelInvoke(ClearPowerOutput);
		}
	}

	public override void ClearBusy()
	{
		base.ClearBusy();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if (elevatorStatic != null)
		{
			elevatorStatic.OnLiftArrivedAtFloor();
		}
		NotifyLiftEntityDoorsOpen(state: true);
	}

	protected override void OpenLiftDoors()
	{
		base.OpenLiftDoors();
		OnLiftArrivedAtFloor();
	}

	public void OnLiftArrivedAtFloor()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		MarkDirty();
		Invoke(ClearPowerOutput, 10f);
	}

	public void ClearPowerOutput()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		MarkDirty();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved3))
		{
			return 0;
		}
		return 1;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		FloorNumberDisplay = (ElevatorFloorNumber)info.msg.elevator.floorDisplay;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.IsTop)
		{
			info.msg.elevator.floorList = Pool.Get<List<NetworkableId>>();
			foreach (ElevatorStatic floorPosition in floorPositions)
			{
				info.msg.elevator.floorList.Add(floorPosition.net.ID);
			}
		}
		info.msg.elevator.floorDisplay = (int)FloorNumberDisplay;
	}
}
