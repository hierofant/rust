using UnityEngine;

namespace ConVar;

[Factory("heli")]
public class PatrolHelicopter : ConsoleSystem
{
	private const string path = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";

	[ServerVar(Help = "(Generated) How many minutes the patrol helicopter stays airborne before self-destructing; default is 30 minutes")]
	public static float lifetimeMinutes = 30f;

	[ServerVar(Help = "(Generated) Number of gun hardpoints active on the patrol helicopter; set to 0 to disable its guns without despawning it")]
	public static int guns = 1;

	[ServerVar(Help = "(Generated) Multiplier applied to all bullet damage dealt by the patrol helicopter; 1.0 = normal, 2.0 = double damage")]
	public static float bulletDamageScale = 1f;

	[ServerVar(Help = "(Generated) Cone angle in degrees of the patrol helicopter gun spread; higher values make the helicopter less accurate")]
	public static float bulletAccuracy = 2f;

	[ServerVar(Help = "(Generated) Spawns a patrol helicopter at the calling admin position, bypassing the normal random spawn logic; primarily used for testing")]
	public static void drop(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			Debug.Log("heli called to : " + basePlayer.transform.position);
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.GetComponent<PatrolHelicopterAI>().SetInitialDestination(basePlayer.transform.position + new Vector3(0f, 10f, 0f), 0f);
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to fly to the calling admin position and attack there")]
	public static void calltome(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			Debug.Log("heli called to : " + basePlayer.transform.position);
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.GetComponent<PatrolHelicopterAI>().SetInitialDestination(basePlayer.transform.position + new Vector3(0f, 10f, 0f));
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Spawns and sends a patrol helicopter to the specified player or position, using the same logic as calltome but targeting another player")]
	public static void call(Arg arg)
	{
		if ((bool)ArgEx.Player(arg))
		{
			Debug.Log("Helicopter inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to perform a strafing run on the calling admin current position")]
	public static void strafe(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.strafe_target = basePlayer;
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Strafe_Enter(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to orbit around the calling admin current position")]
	public static void orbit(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Orbit_Enter(70f);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to orbit and strafe the calling admin position simultaneously")]
	public static void orbitstrafe(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.strafe_target = basePlayer;
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_OrbitStrafe_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Moves the active patrol helicopter to the calling admin current position without entering combat mode")]
	public static void move(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Move_Enter(basePlayer.transform.position);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to immediately flee to a random distant location and disengage")]
	public static void flee(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Flee_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to resume normal patrol mode, following its randomised waypoint path across the map")]
	public static void patrol(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Patrol_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Forces the active patrol helicopter to die immediately, triggering its death explosion and crash sequence")]
	public static void death(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if (heliInstance == null)
			{
				Debug.Log("no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = basePlayer.transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Death_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Triggers the helicopter puzzle sequence (approach, puzzle activation, reward) for testing the helicopter monument puzzle")]
	public static void testpuzzle(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			_ = basePlayer.IsDeveloper;
		}
	}
}
