using Facepunch;
using Facepunch.Extend;
using ProtoBuf;
using UnityEngine;

public class HarborCraneStatic : HarborCrane
{
	public float StartingDepth;

	public float StartingHeight;

	public float StartingAngle;

	public Transform HangingLadder;

	private TransformHandle craneGrabHandle;

	private TransformHandle armRootHandle;

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		SetArmPos(StartingAngle, StartingHeight, StartingDepth);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		craneGrabHandle = CraneGrab.transformHandle;
		armRootHandle = ArmRoot.transformHandle;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.harborCrane = Pool.Get<ProtoBuf.HarborCrane>();
		Vector3 vector;
		Quaternion quaternion;
		if (BaseNetworkable.UseParallelSaves)
		{
			vector = Facepunch.Extend.TransformEx.Unsafe.GetLocalPosMT(in craneGrabHandle);
			quaternion = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in armRootHandle);
		}
		else
		{
			vector = CraneGrab.localPosition;
			quaternion = ArmRoot.localRotation;
		}
		info.msg.harborCrane.depth = vector.x;
		info.msg.harborCrane.height = vector.y;
		info.msg.harborCrane.yaw = quaternion.eulerAngles.z;
	}

	private void SetArmPos(float angle, float height, float depth)
	{
		ArmRoot.localEulerAngles = new Vector3(0f, 0f, angle);
		CraneGrab.localPosition = new Vector3(depth, height, 0f);
		HangingLadder.rotation = Quaternion.LookRotation(base.transform.right, Vector3.up);
		UpdateArmSupports(base.transform.forward);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.harborCrane != null)
		{
			SetArmPos(info.msg.harborCrane.yaw, info.msg.harborCrane.height, info.msg.harborCrane.depth);
		}
	}
}
