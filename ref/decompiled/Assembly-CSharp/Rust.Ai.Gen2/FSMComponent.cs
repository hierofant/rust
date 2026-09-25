using System.Collections.Generic;
using System.Text;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(BlackboardComponent), typeof(NPCEncounterTimer))]
[SoftRequireComponent(typeof(RustNavMeshAgent), typeof(RootMotionPlayer), typeof(SenseComponent))]
public class FSMComponent : EntityComponent<BaseEntity>
{
	public class TickFSMWorkQueue : PersistentObjectWorkQueue<FSMComponent>
	{
		protected override void RunJob(FSMComponent component)
		{
			if (ShouldAdd(component) && component.isRunning)
			{
				component.Senses.Tick();
				if (component.TryGetComponent<NPCEncounterTimer>(out var component2))
				{
					component2.Tick();
				}
				component.Tick();
				if (component.TryGetComponent<NpcBarkComponent>(out var component3))
				{
					component3.Tick();
				}
				if (component.TryGetComponent<NPCNetworking>(out var component4))
				{
					component4.Tick();
				}
			}
		}

		protected override bool ShouldAdd(FSMComponent component)
		{
			if (base.ShouldAdd(component))
			{
				return component.baseEntity.IsValid();
			}
			return false;
		}
	}

	private bool isRunning;

	private SenseComponent _senses;

	public const float minRefreshIntervalSeconds = 0f;

	public const float maxRefreshIntervalSeconds = 0.5f;

	private double? _lastTickTime;

	private double nextRefreshTime;

	private const int maxStateChangesPerTick = 3;

	private List<FSMStateBase> sameFrameStateChangesHistory = new List<FSMStateBase>();

	private FSMStateBase pendingStateChange;

	private FSMPayload pendingStateChangePayload;

	public static TickFSMWorkQueue workQueue = new TickFSMWorkQueue();

	public const float frameBudgetMs = 1f;

	public FSMStateBase CurrentState { get; private set; }

	private SenseComponent Senses => _senses ?? (_senses = base.baseEntity.GetComponent<SenseComponent>());

	private float RefreshInterval
	{
		get
		{
			if (!Senses.ShouldRefreshFast)
			{
				return 0.5f;
			}
			return 0f;
		}
	}

	private double LastTickTime
	{
		get
		{
			double valueOrDefault = _lastTickTime.GetValueOrDefault();
			if (!_lastTickTime.HasValue)
			{
				valueOrDefault = UnityEngine.Time.timeAsDouble;
				_lastTickTime = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
		set
		{
			_lastTickTime = value;
		}
	}

	public void SetFsmActive(bool newActive)
	{
		if (newActive != isRunning)
		{
			isRunning = newActive;
			if (isRunning)
			{
				_lastTickTime = null;
				workQueue.Add(this);
			}
			else
			{
				workQueue.Remove(this);
			}
		}
	}

	public override void DestroyShared()
	{
		if (base.baseEntity.isServer)
		{
			SetFsmActive(newActive: false);
			base.DestroyShared();
		}
	}

	public static void ShowDebugInfoAroundLocation(BasePlayer player, float radius = 100f)
	{
		if (!player.IsValid())
		{
			return;
		}
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		BaseEntity.Query.Server.GetBrainsInSphere(player.transform.position, radius, pooledList);
		foreach (BaseEntity item in pooledList)
		{
			FSMComponent component = item.GetComponent<FSMComponent>();
			if (!(component == null) && component.CurrentState != null && component.isRunning)
			{
				player.ClientRPC(RpcTarget.Player("CL_ShowStateDebugInfo", player), component.baseEntity.transform.position, component.CurrentState.Name);
			}
		}
	}

	protected void ForceTickOnTheNextUpdate()
	{
		nextRefreshTime = 0.0;
	}

	public void Tick()
	{
		using (TimeWarning.New("FSMComponent.Tick"))
		{
			if (UnityEngine.Time.timeAsDouble < nextRefreshTime)
			{
				return;
			}
			nextRefreshTime = UnityEngine.Time.timeAsDouble + (double)RefreshInterval;
			float deltaTime = (float)(UnityEngine.Time.timeAsDouble - LastTickTime);
			LastTickTime = UnityEngine.Time.timeAsDouble;
			sameFrameStateChangesHistory.Clear();
			if (pendingStateChange != null)
			{
				SetState(pendingStateChange, pendingStateChangePayload);
			}
			else
			{
				if (CurrentState == null)
				{
					return;
				}
				FSMPayload payload = default(FSMPayload);
				using (TimeWarning.New("NormalTransitions"))
				{
					using PooledList<FSMStateBase> pooledList = Facepunch.Pool.Get<PooledList<FSMStateBase>>();
					CurrentState.FindAncestry(pooledList);
					foreach (FSMStateBase item in pooledList)
					{
						foreach (var (fSMTransitionBase, fSMStateBase) in item.transitions)
						{
							if (fSMTransitionBase.Owner == null)
							{
								fSMTransitionBase.Init(base.baseEntity);
							}
							if (fSMTransitionBase.Evaluate(ref payload))
							{
								fSMStateBase.Owner = base.baseEntity;
								fSMTransitionBase.OnTransitionTaken(CurrentState, fSMStateBase);
								SetState(fSMStateBase, payload);
								return;
							}
						}
					}
				}
				EFSMStateStatus currentStateStatus = EFSMStateStatus.None;
				using (TimeWarning.New("StateTick"))
				{
					using (TimeWarning.New(CurrentState.Name))
					{
						currentStateStatus = CurrentState.OnStateUpdate(deltaTime);
					}
				}
				EvaluateEndTransitions(currentStateStatus);
			}
		}
	}

	private void EvaluateEndTransitions(EFSMStateStatus currentStateStatus)
	{
		using (TimeWarning.New("EndTransitions"))
		{
			if (currentStateStatus == EFSMStateStatus.None)
			{
				return;
			}
			FSMPayload payload = default(FSMPayload);
			using PooledList<FSMStateBase> pooledList = Facepunch.Pool.Get<PooledList<FSMStateBase>>();
			CurrentState.FindAncestry(pooledList);
			foreach (FSMStateBase item in pooledList)
			{
				foreach (var (fSMTransitionBase, fSMStateBase, eFSMStateStatus) in item.endTransitions)
				{
					if (eFSMStateStatus != (EFSMStateStatus.Success | EFSMStateStatus.Failure) && eFSMStateStatus != currentStateStatus)
					{
						continue;
					}
					bool flag = true;
					if (fSMTransitionBase != null)
					{
						if (fSMTransitionBase.Owner == null)
						{
							fSMTransitionBase.Init(base.baseEntity);
						}
						flag = fSMTransitionBase.Evaluate(ref payload);
					}
					if (flag)
					{
						fSMStateBase.Owner = base.baseEntity;
						fSMTransitionBase?.OnTransitionTaken(CurrentState, fSMStateBase);
						SetState(fSMStateBase, payload);
						ForceTickOnTheNextUpdate();
						return;
					}
				}
			}
		}
	}

	public void SetState(FSMStateBase newState, FSMPayload payload = default(FSMPayload))
	{
		using (TimeWarning.New("SetState"))
		{
			newState.Owner = base.baseEntity;
			pendingStateChange = null;
			pendingStateChangePayload = default(FSMPayload);
			sameFrameStateChangesHistory.Add(newState);
			if (sameFrameStateChangesHistory.Count > 3)
			{
				if (!AI.logIssues)
				{
					return;
				}
				StringBuilder obj = Facepunch.Pool.Get<StringBuilder>();
				obj.AppendFormat("[FSM] Possible endless recursion detected from {0} to {1} on {2}\n", CurrentState?.Name, newState.Name, base.baseEntity);
				foreach (FSMStateBase item5 in sameFrameStateChangesHistory)
				{
					obj.AppendFormat("{0} -> ", item5.Name);
				}
				Debug.LogWarning(obj);
				pendingStateChange = newState;
				pendingStateChangePayload = payload;
				Facepunch.Pool.FreeUnmanaged(ref obj);
				return;
			}
			if (CurrentState != null)
			{
				using (TimeWarning.New("Transitions OnStateExit"))
				{
					using PooledList<FSMStateBase> pooledList = Facepunch.Pool.Get<PooledList<FSMStateBase>>();
					CurrentState.FindAncestry(pooledList);
					foreach (FSMStateBase item6 in pooledList)
					{
						foreach (var endTransition in item6.endTransitions)
						{
							FSMTransitionBase item = endTransition.transition;
							if (item != null && item.Owner == null)
							{
								item.Init(base.baseEntity);
							}
							item?.OnStateExit();
						}
						foreach (var transition in item6.transitions)
						{
							FSMTransitionBase item2 = transition.transition;
							if (item2 != null && item2.Owner == null)
							{
								item2.Init(base.baseEntity);
							}
							item2.OnStateExit();
						}
					}
				}
				using (TimeWarning.New("OnStateExit"))
				{
					using (TimeWarning.New(CurrentState.Name))
					{
						CurrentState.OnStateExit();
					}
				}
			}
			CurrentState = newState;
			using (TimeWarning.New("Transitions OnStateEnter"))
			{
				using PooledList<FSMStateBase> pooledList2 = Facepunch.Pool.Get<PooledList<FSMStateBase>>();
				CurrentState.FindAncestry(pooledList2);
				foreach (FSMStateBase item7 in pooledList2)
				{
					foreach (var endTransition2 in item7.endTransitions)
					{
						FSMTransitionBase item3 = endTransition2.transition;
						if (item3 != null && item3.Owner == null)
						{
							item3.Init(base.baseEntity);
						}
						item3?.OnStateEnter();
					}
					foreach (var transition2 in item7.transitions)
					{
						FSMTransitionBase item4 = transition2.transition;
						if (item4 != null && item4.Owner == null)
						{
							item4.Init(base.baseEntity);
						}
						item4.OnStateEnter();
					}
				}
			}
			using (TimeWarning.New("OnStateEnter"))
			{
				using (TimeWarning.New(CurrentState.Name))
				{
					EFSMStateStatus currentStateStatus = CurrentState.OnStateEnter(payload);
					payload.Dispose();
					EvaluateEndTransitions(currentStateStatus);
				}
			}
		}
	}
}
