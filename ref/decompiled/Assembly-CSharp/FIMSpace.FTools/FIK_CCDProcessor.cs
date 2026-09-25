using System;
using UnityEngine;

namespace FIMSpace.FTools;

[Serializable]
public class FIK_CCDProcessor : FIK_ProcessorBase
{
	[Serializable]
	public class CCDIKBone : FIK_IKBoneBase
	{
		[Range(0f, 180f)]
		public float AngleLimit = 45f;

		[Range(0f, 180f)]
		public float TwistAngleLimit = 5f;

		public Vector3 ForwardOrientation;

		public float FrameWorldLength = 1f;

		public Vector2 HingeLimits = Vector2.zero;

		public Quaternion PreviousHingeRotation;

		public float PreviousHingeAngle;

		public Vector3 LastIKLocPosition;

		public Quaternion LastIKLocRotation;

		public CCDIKBone IKParent { get; private set; }

		public CCDIKBone IKChild { get; private set; }

		public CCDIKBone(Transform t)
			: base(t)
		{
		}

		public void Init(CCDIKBone child, CCDIKBone parent)
		{
			LastIKLocPosition = base.transform.localPosition;
			IKParent = parent;
			if (child != null)
			{
				SetChild(child);
			}
			IKChild = child;
		}

		public override void SetChild(FIK_IKBoneBase child)
		{
			base.SetChild(child);
		}

		public void AngleLimiting()
		{
			Quaternion quaternion = Quaternion.Inverse(LastKeyLocalRotation) * base.transform.localRotation;
			Quaternion quaternion2 = quaternion;
			if (FEngineering.VIsZero(HingeLimits))
			{
				if (AngleLimit < 180f)
				{
					quaternion2 = LimitSpherical(quaternion2);
				}
				if (TwistAngleLimit < 180f)
				{
					quaternion2 = LimitZ(quaternion2);
				}
			}
			else
			{
				quaternion2 = LimitHinge(quaternion2);
			}
			if (!FEngineering.QIsSame(quaternion2, quaternion))
			{
				base.transform.localRotation = LastKeyLocalRotation * quaternion2;
			}
		}

		private Quaternion LimitSpherical(Quaternion rotation)
		{
			if (FEngineering.QIsZero(rotation))
			{
				return rotation;
			}
			Vector3 vector = rotation * ForwardOrientation;
			Quaternion quaternion = Quaternion.RotateTowards(Quaternion.identity, Quaternion.FromToRotation(ForwardOrientation, vector), AngleLimit);
			return Quaternion.FromToRotation(vector, quaternion * ForwardOrientation) * rotation;
		}

		private Quaternion LimitZ(Quaternion currentRotation)
		{
			Vector3 vector = new Vector3(ForwardOrientation.y, ForwardOrientation.z, ForwardOrientation.x);
			Vector3 normal = currentRotation * ForwardOrientation;
			Vector3 tangent = vector;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			vector = currentRotation * vector;
			Vector3.OrthoNormalize(ref normal, ref vector);
			Quaternion quaternion = Quaternion.FromToRotation(vector, tangent) * currentRotation;
			if (TwistAngleLimit <= 0f)
			{
				return quaternion;
			}
			return Quaternion.RotateTowards(quaternion, currentRotation, TwistAngleLimit);
		}

		private Quaternion LimitHinge(Quaternion rotation)
		{
			Quaternion quaternion = Quaternion.FromToRotation(rotation * ForwardOrientation, ForwardOrientation) * rotation * Quaternion.Inverse(PreviousHingeRotation);
			float num = Quaternion.Angle(Quaternion.identity, quaternion);
			Vector3 vector = new Vector3(ForwardOrientation.z, ForwardOrientation.x, ForwardOrientation.y);
			Vector3 rhs = Vector3.Cross(vector, ForwardOrientation);
			if (Vector3.Dot(quaternion * vector, rhs) > 0f)
			{
				num = 0f - num;
			}
			PreviousHingeAngle = Mathf.Clamp(PreviousHingeAngle + num, HingeLimits.x, HingeLimits.y);
			PreviousHingeRotation = Quaternion.AngleAxis(PreviousHingeAngle, ForwardOrientation);
			return PreviousHingeRotation;
		}
	}

	public CCDIKBone[] IKBones;

	public bool ContinousSolving = true;

	[Range(0f, 1f)]
	public float SyncWithAnimator = 1f;

	[Range(1f, 12f)]
	public int ReactionQuality = 2;

	[Range(0f, 1f)]
	public float Smoothing;

	[Range(0f, 1.5f)]
	public float StretchToTarget;

	public AnimationCurve StretchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public bool Use2D;

	public bool Invert;

	public CCDIKBone StartIKBone => IKBones[0];

	public CCDIKBone EndIKBone => IKBones[IKBones.Length - 1];

	public float ActiveLength { get; private set; }

	public FIK_CCDProcessor(Transform[] bonesChain)
	{
		IKBones = new CCDIKBone[bonesChain.Length];
		FIK_IKBoneBase[] bones = new CCDIKBone[IKBones.Length];
		base.Bones = bones;
		for (int i = 0; i < bonesChain.Length; i++)
		{
			IKBones[i] = new CCDIKBone(bonesChain[i]);
			base.Bones[i] = IKBones[i];
		}
		IKTargetPosition = base.EndBone.transform.position;
		IKTargetRotation = base.EndBone.transform.rotation;
	}

	public override void Init(Transform root)
	{
		if (base.Initialized)
		{
			return;
		}
		base.fullLength = 0f;
		for (int i = 0; i < base.Bones.Length; i++)
		{
			CCDIKBone cCDIKBone = IKBones[i];
			CCDIKBone child = null;
			CCDIKBone parent = null;
			if (i > 0)
			{
				parent = IKBones[i - 1];
			}
			if (i < base.Bones.Length - 1)
			{
				child = IKBones[i + 1];
			}
			if (i < base.Bones.Length - 1)
			{
				IKBones[i].Init(child, parent);
				base.fullLength += cCDIKBone.BoneLength;
				cCDIKBone.ForwardOrientation = Quaternion.Inverse(cCDIKBone.transform.rotation) * (IKBones[i + 1].transform.position - cCDIKBone.transform.position);
			}
			else
			{
				IKBones[i].Init(child, parent);
				cCDIKBone.ForwardOrientation = Quaternion.Inverse(cCDIKBone.transform.rotation) * (IKBones[IKBones.Length - 1].transform.position - IKBones[0].transform.position);
			}
		}
		base.Initialized = true;
	}

	public override void Update()
	{
		if (!base.Initialized || IKWeight <= 0f)
		{
			return;
		}
		CCDIKBone cCDIKBone = IKBones[0];
		if (ContinousSolving)
		{
			while (cCDIKBone != null)
			{
				cCDIKBone.LastKeyLocalRotation = cCDIKBone.transform.localRotation;
				cCDIKBone.transform.localPosition = cCDIKBone.LastIKLocPosition;
				cCDIKBone.transform.localRotation = cCDIKBone.LastIKLocRotation;
				cCDIKBone = cCDIKBone.IKChild;
			}
		}
		else if (SyncWithAnimator > 0f)
		{
			while (cCDIKBone != null)
			{
				cCDIKBone.LastKeyLocalRotation = cCDIKBone.transform.localRotation;
				cCDIKBone = cCDIKBone.IKChild;
			}
		}
		if (ReactionQuality < 0)
		{
			ReactionQuality = 1;
		}
		Vector3 vector = Vector3.zero;
		if (ReactionQuality > 1)
		{
			vector = GetGoalPivotOffset();
		}
		for (int i = 0; i < ReactionQuality && (i < 1 || vector.sqrMagnitude != 0f || !(Smoothing > 0f) || !(GetVelocityDifference() < Smoothing * Smoothing)); i++)
		{
			LastLocalDirection = RefreshLocalDirection();
			Vector3 vector2 = IKTargetPosition + vector;
			cCDIKBone = IKBones[IKBones.Length - 2];
			if (!Use2D)
			{
				if (!Invert)
				{
					while (cCDIKBone != null)
					{
						float num = cCDIKBone.MotionWeight * IKWeight;
						if (num > 0f)
						{
							Quaternion quaternion = Quaternion.FromToRotation(base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position, vector2 - cCDIKBone.transform.position) * cCDIKBone.transform.rotation;
							if (num < 1f)
							{
								cCDIKBone.transform.rotation = Quaternion.Lerp(cCDIKBone.transform.rotation, quaternion, num);
							}
							else
							{
								cCDIKBone.transform.rotation = quaternion;
							}
						}
						cCDIKBone.AngleLimiting();
						cCDIKBone = cCDIKBone.IKParent;
					}
					continue;
				}
				while (cCDIKBone != null)
				{
					cCDIKBone.AngleLimiting();
					cCDIKBone = cCDIKBone.IKParent;
				}
				for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
				{
					float num2 = cCDIKBone.MotionWeight * IKWeight;
					if (num2 > 0f)
					{
						Quaternion quaternion2 = Quaternion.FromToRotation(base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position, vector2 - cCDIKBone.transform.position) * cCDIKBone.transform.rotation;
						if (num2 < 1f)
						{
							cCDIKBone.transform.rotation = Quaternion.Lerp(cCDIKBone.transform.rotation, quaternion2, num2);
						}
						else
						{
							cCDIKBone.transform.rotation = quaternion2;
						}
					}
				}
				continue;
			}
			if (!Invert)
			{
				while (cCDIKBone != null)
				{
					float num3 = cCDIKBone.MotionWeight * IKWeight;
					if (num3 > 0f)
					{
						Vector3 vector3 = base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position;
						Vector3 vector4 = vector2 - cCDIKBone.transform.position;
						cCDIKBone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(Mathf.Atan2(vector3.x, vector3.y) * 57.29578f, Mathf.Atan2(vector4.x, vector4.y) * 57.29578f) * num3, Vector3.back) * cCDIKBone.transform.rotation;
					}
					cCDIKBone.AngleLimiting();
					cCDIKBone = cCDIKBone.IKParent;
				}
				continue;
			}
			while (cCDIKBone != null)
			{
				cCDIKBone.AngleLimiting();
				cCDIKBone = cCDIKBone.IKParent;
			}
			for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
			{
				float num4 = cCDIKBone.MotionWeight * IKWeight;
				if (num4 > 0f)
				{
					Vector3 vector5 = base.Bones[base.Bones.Length - 1].transform.position - cCDIKBone.transform.position;
					Vector3 vector6 = vector2 - cCDIKBone.transform.position;
					cCDIKBone.transform.rotation = Quaternion.AngleAxis(Mathf.DeltaAngle(Mathf.Atan2(vector5.x, vector5.y) * 57.29578f, Mathf.Atan2(vector6.x, vector6.y) * 57.29578f) * num4, Vector3.back) * cCDIKBone.transform.rotation;
				}
			}
		}
		LastLocalDirection = RefreshLocalDirection();
		if (StretchToTarget > 0f)
		{
			float num5 = (IKTargetPosition - EndIKBone.transform.position).magnitude;
			ActiveLength = Mathf.Epsilon;
			cCDIKBone = IKBones[0];
			int num6 = 0;
			float num7 = Mathf.Max(1f, StretchToTarget);
			while (cCDIKBone.IKChild != null && !(num5 <= 0f))
			{
				Vector3 normalized = (IKTargetPosition - cCDIKBone.transform.position).normalized;
				Vector3 position = cCDIKBone.transform.position;
				Vector3 position2 = cCDIKBone.IKChild.transform.position;
				Vector3 normalized2 = (position2 - position).normalized;
				float num8 = Vector3.Dot(normalized2, normalized);
				if (num8 > 0f)
				{
					float num9 = cCDIKBone.BoneLength * num7 * num8;
					if (num9 > num5)
					{
						num9 = num5;
					}
					Vector3 b = position2 + normalized2 * num9;
					cCDIKBone.IKChild.transform.position = Vector3.Lerp(position2, b, StretchToTarget);
					cCDIKBone.transform.rotation = cCDIKBone.transform.rotation * Quaternion.FromToRotation(position2 - position, cCDIKBone.Child.transform.position - cCDIKBone.transform.position);
					num5 -= Vector3.Distance(position2, b);
				}
				cCDIKBone = cCDIKBone.IKChild;
				num6++;
			}
		}
		for (cCDIKBone = IKBones[0]; cCDIKBone != null; cCDIKBone = cCDIKBone.IKChild)
		{
			cCDIKBone.LastIKLocRotation = cCDIKBone.transform.localRotation;
			cCDIKBone.LastIKLocPosition = cCDIKBone.transform.localPosition;
			Quaternion quaternion3 = cCDIKBone.LastIKLocRotation * Quaternion.Inverse(cCDIKBone.InitialLocalRotation);
			cCDIKBone.transform.localRotation = Quaternion.Lerp(cCDIKBone.LastIKLocRotation, quaternion3 * cCDIKBone.LastKeyLocalRotation, SyncWithAnimator);
			if (IKWeight < 1f)
			{
				cCDIKBone.transform.localRotation = Quaternion.Lerp(cCDIKBone.LastKeyLocalRotation, cCDIKBone.transform.localRotation, IKWeight);
			}
		}
	}

	protected Vector3 GetGoalPivotOffset()
	{
		if (!GoalPivotOffsetDetected())
		{
			return Vector3.zero;
		}
		Vector3 normalized = (IKTargetPosition - IKBones[0].transform.position).normalized;
		Vector3 rhs = new Vector3(normalized.y, normalized.z, normalized.x);
		if (IKBones[IKBones.Length - 2].AngleLimit < 180f || IKBones[IKBones.Length - 2].TwistAngleLimit < 180f)
		{
			rhs = IKBones[IKBones.Length - 2].transform.rotation * IKBones[IKBones.Length - 2].ForwardOrientation;
		}
		return Vector3.Cross(normalized, rhs) * IKBones[IKBones.Length - 2].BoneLength * 0.5f;
	}

	private bool GoalPivotOffsetDetected()
	{
		if (!base.Initialized)
		{
			return false;
		}
		Vector3 vector = base.Bones[base.Bones.Length - 1].transform.position - base.Bones[0].transform.position;
		Vector3 vector2 = IKTargetPosition - base.Bones[0].transform.position;
		float magnitude = vector.magnitude;
		float magnitude2 = vector2.magnitude;
		if (magnitude2 == 0f)
		{
			return false;
		}
		if (magnitude == 0f)
		{
			return false;
		}
		if (magnitude < magnitude2)
		{
			return false;
		}
		if (magnitude < base.fullLength - base.Bones[base.Bones.Length - 2].BoneLength * 0.1f)
		{
			return false;
		}
		if (magnitude2 > magnitude)
		{
			return false;
		}
		if (Vector3.Dot(vector / magnitude, vector2 / magnitude2) < 0.999f)
		{
			return false;
		}
		return true;
	}

	private Vector3 RefreshLocalDirection()
	{
		LocalDirection = base.Bones[0].transform.InverseTransformDirection(base.Bones[base.Bones.Length - 1].transform.position - base.Bones[0].transform.position);
		return LocalDirection;
	}

	private float GetVelocityDifference()
	{
		return Vector3.SqrMagnitude(LocalDirection - LastLocalDirection);
	}

	public void AutoLimitAngle(float angleLimit = 60f, float twistAngleLimit = 50f)
	{
		if (IKBones == null)
		{
			return;
		}
		float num = 1f / (float)IKBones.Length;
		if (Invert)
		{
			for (int i = 0; i < IKBones.Length; i++)
			{
				IKBones[i].AngleLimit = angleLimit * Mathf.Min(1f, (1f - (float)(i + 1) * num) * 3f);
				IKBones[i].TwistAngleLimit = twistAngleLimit * Mathf.Min(1f, (1f - (float)(i + 1) * num) * 4.5f);
			}
		}
		else
		{
			for (int j = 0; j < IKBones.Length; j++)
			{
				IKBones[j].AngleLimit = angleLimit * Mathf.Min(1f, (float)(j + 1) * num * 3f);
				IKBones[j].TwistAngleLimit = twistAngleLimit * Mathf.Min(1f, (float)(j + 1) * num * 4.5f);
			}
		}
	}

	public void AutoWeightBones(float baseValue = 1f)
	{
		float num = baseValue / ((float)base.Bones.Length * 1.3f);
		if (Invert)
		{
			for (int i = 0; i < base.Bones.Length; i++)
			{
				base.Bones[i].MotionWeight = 1f - (baseValue - num * (float)i);
			}
		}
		else
		{
			for (int j = 0; j < base.Bones.Length; j++)
			{
				base.Bones[j].MotionWeight = baseValue - num * (float)j;
			}
		}
	}

	public void AutoWeightBones(AnimationCurve weightCurve)
	{
		if (Invert)
		{
			for (int i = 0; i < base.Bones.Length; i++)
			{
				base.Bones[i].MotionWeight = Mathf.Clamp(1f - weightCurve.Evaluate((float)i / (float)base.Bones.Length), 0f, 1f);
			}
		}
		else
		{
			for (int j = 0; j < base.Bones.Length; j++)
			{
				base.Bones[j].MotionWeight = Mathf.Clamp(weightCurve.Evaluate((float)j / (float)base.Bones.Length), 0f, 1f);
			}
		}
	}
}
