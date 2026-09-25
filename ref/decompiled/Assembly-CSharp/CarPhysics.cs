using ConVar;
using UnityEngine;

public class CarPhysics<TCar> where TCar : BaseVehicle, CarPhysics<TCar>.ICar
{
	public interface ICar
	{
		VehicleTerrainHandler.Surface OnSurface { get; }

		float GetThrottleInput();

		float GetBrakeInput();

		float GetSteerInput();

		bool GetSteerSpeedMod(float speed);

		float GetSteerMaxMult(float speed);

		float GetMaxForwardSpeed();

		float GetMaxDriveForce();

		float GetAdjustedDriveForce(float absSpeed, float topSpeed);

		float GetModifiedDrag();

		CarWheel[] GetWheels();

		float GetWheelsMidPos();
	}

	private class ServerWheelData
	{
		public CarWheel wheel;

		public Transform wheelColliderTransform;

		public WheelCollider wheelCollider;

		public bool isGrounded;

		public float downforce;

		public float forceDistance;

		public WheelHit hit;

		public Vector2 localRigForce;

		public Vector2 localVelocity;

		public float angularVelocity;

		public Vector3 origin;

		public Vector2 tyreForce;

		public Vector2 tyreSlip;

		public Vector3 velocity;

		public bool isBraking;

		public bool hasThrottleInput;

		public bool isFrontWheel;

		public bool isLeftWheel;

		public float handbrakeGripCurrent = 1f;

		public bool isHandbrakeLocked;
	}

	private readonly ServerWheelData[] wheelData;

	private readonly TCar car;

	private readonly Transform transform;

	private readonly Rigidbody rBody;

	private readonly CarSettings vehicleSettings;

	private float speedAngle;

	private bool wasSleeping = true;

	private bool hasDriver;

	private bool hadDriver;

	private float steerLerpSpeed;

	public float lastMovingTime = float.MinValue;

	private WheelFrictionCurve zeroFriction = new WheelFrictionCurve
	{
		stiffness = 0f
	};

	private Vector3 prevLocalCOM;

	private readonly float midWheelPos;

	private const bool WHEEL_HIT_CORRECTION = true;

	private const float SLEEP_SPEED = 0.25f;

	private const float SLEEP_DELAY = 10f;

	private const float AIR_DRAG = 0.25f;

	private const float DEFAULT_GROUND_GRIP = 0.75f;

	private const float ROAD_GROUND_GRIP = 1f;

	private const float ICE_GROUND_GRIP = 0.25f;

	private bool slowSpeedExitFlag;

	private const float SLOW_SPEED_EXIT_SPEED = 4f;

	public TimeSince timeSinceWaterCheck;

	public float DriveWheelVelocity { get; private set; }

	public float DriveWheelSlip { get; private set; }

	public float SteerAngle { get; private set; }

	public float TankThrottleLeft { get; private set; }

	public float TankThrottleRight { get; private set; }

	private bool InSlowSpeedExitMode
	{
		get
		{
			if (!hasDriver)
			{
				return slowSpeedExitFlag;
			}
			return false;
		}
	}

	public CarPhysics(TCar car, Transform transform, Rigidbody rBody, CarSettings vehicleSettings)
	{
		Transform transform2 = transform;
		base._002Ector();
		CarPhysics<TCar> carPhysics = this;
		this.car = car;
		this.transform = transform2;
		this.rBody = rBody;
		this.vehicleSettings = vehicleSettings;
		timeSinceWaterCheck = default(TimeSince);
		timeSinceWaterCheck = float.MaxValue;
		prevLocalCOM = rBody.centerOfMass;
		CarWheel[] wheels = car.GetWheels();
		wheelData = new ServerWheelData[wheels.Length];
		for (int i = 0; i < wheelData.Length; i++)
		{
			wheelData[i] = AddWheel(wheels[i]);
		}
		midWheelPos = car.GetWheelsMidPos();
		wheelData[0].wheel.wheelCollider.ConfigureVehicleSubsteps(1000f, 1, 1);
		lastMovingTime = UnityEngine.Time.realtimeSinceStartup;
		ServerWheelData AddWheel(CarWheel wheel)
		{
			ServerWheelData serverWheelData = new ServerWheelData();
			serverWheelData.wheelCollider = wheel.wheelCollider;
			serverWheelData.wheelColliderTransform = wheel.wheelCollider.transform;
			serverWheelData.forceDistance = GetWheelForceDistance(wheel.wheelCollider);
			serverWheelData.wheel = wheel;
			serverWheelData.wheelCollider.sidewaysFriction = zeroFriction;
			serverWheelData.wheelCollider.forwardFriction = zeroFriction;
			Vector3 vector = transform2.InverseTransformPoint(wheel.wheelCollider.transform.position);
			serverWheelData.isFrontWheel = vector.z > 0f;
			serverWheelData.isLeftWheel = vector.x < 0f;
			return serverWheelData;
		}
	}

	public void FixedUpdate(float dt, float speed)
	{
		using (TimeWarning.New("CarPhysics.FixedUpdate"))
		{
			if (rBody.centerOfMass != prevLocalCOM)
			{
				COMChanged();
			}
			float num = Mathf.Abs(speed);
			hasDriver = car.HasDriver();
			if (!hasDriver && hadDriver)
			{
				if (num <= 4f)
				{
					slowSpeedExitFlag = true;
				}
			}
			else if (hasDriver && !hadDriver)
			{
				slowSpeedExitFlag = false;
			}
			if ((hasDriver || !vehicleSettings.canSleep) && rBody.IsSleeping())
			{
				rBody.WakeUp();
				if (vehicleSettings.kinematicWhileAsleep)
				{
					rBody.isKinematic = false;
				}
			}
			if (!rBody.IsSleeping())
			{
				if ((wasSleeping && !rBody.isKinematic) || num > 0.25f || Mathf.Abs(rBody.angularVelocity.magnitude) > 0.25f)
				{
					lastMovingTime = UnityEngine.Time.time;
				}
				bool flag = vehicleSettings.canSleep && !hasDriver && UnityEngine.Time.time > lastMovingTime + 10f;
				if (flag && (car.GetParentEntity() as BaseVehicle).IsValid())
				{
					flag = false;
				}
				if (flag)
				{
					for (int i = 0; i < wheelData.Length; i++)
					{
						ServerWheelData serverWheelData = wheelData[i];
						serverWheelData.wheelCollider.motorTorque = 0f;
						serverWheelData.wheelCollider.brakeTorque = 0f;
						serverWheelData.wheelCollider.steerAngle = 0f;
						if (vehicle.disable_wheels_when_sleeping)
						{
							serverWheelData.wheelCollider.enabled = false;
						}
					}
					rBody.Sleep();
					if (vehicleSettings.kinematicWhileAsleep)
					{
						rBody.isKinematic = true;
					}
				}
				else
				{
					speedAngle = Vector3.Angle(rBody.linearVelocity, transform.forward) * Mathf.Sign(Vector3.Dot(rBody.linearVelocity, transform.right));
					float maxDriveForce = car.GetMaxDriveForce();
					float maxForwardSpeed = car.GetMaxForwardSpeed();
					float num2 = (car.IsOn() ? car.GetThrottleInput() : 0f);
					float steerInput = car.GetSteerInput();
					float brakeInput = (InSlowSpeedExitMode ? 1f : car.GetBrakeInput());
					bool flag2 = !vehicleSettings.disableHandbrakes && car is IHandbrakeCar handbrakeCar && handbrakeCar.GetHandbrakeInput();
					float num3 = 1f;
					if (!flag2)
					{
						if (num < 3f)
						{
							num3 = 2.75f;
						}
						else if (num < 9f)
						{
							float t = Mathf.InverseLerp(9f, 3f, num);
							num3 = Mathf.Lerp(1f, 2.75f, t);
						}
					}
					maxDriveForce *= num3;
					ComputeSteerAngle(num2, steerInput, dt, speed);
					if ((float)timeSinceWaterCheck > 0.25f)
					{
						float a = car.WaterFactor();
						float b = 0f;
						if (car.FindTrigger<TriggerVehicleDrag>(out var result))
						{
							b = result.vehicleDrag;
						}
						float a2 = ((num2 != 0f) ? 0f : 0.25f);
						float a3 = Mathf.Max(a, b);
						a3 = Mathf.Max(a3, car.GetModifiedDrag());
						rBody.linearDamping = Mathf.Max(a2, a3);
						rBody.angularDamping = Mathf.Max(vehicleSettings.baseAngularDamping, a3 * 0.5f);
						timeSinceWaterCheck = 0f;
					}
					int num4 = 0;
					float num5 = 0f;
					bool flag3 = !vehicleSettings.disableHandbrakes && !hasDriver && rBody.linearVelocity.magnitude < 2.5f && (float)car.timeSinceLastPush > 2f && car.OnSurface != VehicleTerrainHandler.Surface.Frictionless;
					bool flag4 = !vehicleSettings.disableHandbrakes && !flag3 && num2 == 0f && num < 0.2f && (float)car.timeSinceLastPush > 2f && car.OnSurface != VehicleTerrainHandler.Surface.Frictionless;
					for (int j = 0; j < wheelData.Length; j++)
					{
						ServerWheelData serverWheelData2 = wheelData[j];
						if (!serverWheelData2.wheelCollider.enabled)
						{
							serverWheelData2.wheelCollider.enabled = true;
							serverWheelData2.wheelCollider.ConfigureVehicleSubsteps(1000f, 1, 1);
						}
						serverWheelData2.wheelCollider.motorTorque = 1E-05f;
						if (flag3)
						{
							serverWheelData2.wheelCollider.brakeTorque = 10000f;
						}
						else if (flag4)
						{
							serverWheelData2.wheelCollider.brakeTorque = 1000f;
						}
						else
						{
							serverWheelData2.wheelCollider.brakeTorque = 0f;
						}
						if (serverWheelData2.wheel.steerWheel)
						{
							serverWheelData2.wheel.wheelCollider.steerAngle = (serverWheelData2.isFrontWheel ? SteerAngle : (vehicleSettings.rearWheelSteer * (0f - SteerAngle)));
						}
						UpdateSuspension(serverWheelData2);
						if (serverWheelData2.isGrounded)
						{
							num4++;
							num5 += wheelData[j].downforce;
						}
					}
					AdjustHitForces(num4, num5 / (float)num4);
					for (int k = 0; k < wheelData.Length; k++)
					{
						ServerWheelData wd = wheelData[k];
						UpdateLocalFrame(wd, dt);
						ComputeTyreForces(wd, speed, maxDriveForce, maxForwardSpeed, num2, brakeInput, num3, flag2, dt);
						ApplyTyreForces(wd);
					}
					ComputeOverallForces();
				}
				wasSleeping = false;
			}
			else
			{
				wasSleeping = true;
			}
			hadDriver = hasDriver;
		}
	}

	public bool IsGrounded()
	{
		int num = 0;
		for (int i = 0; i < wheelData.Length; i++)
		{
			if (wheelData[i].isGrounded)
			{
				num++;
			}
			if (num >= Mathf.FloorToInt((float)wheelData.Length * 0.5f))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWheelGrounded(int index)
	{
		if (index < 0 || index >= wheelData.Length)
		{
			return false;
		}
		return wheelData[index].isGrounded;
	}

	public bool HasHandbrake()
	{
		for (int i = 0; i < wheelData.Length; i++)
		{
			if (wheelData[i].wheelCollider.brakeTorque != 10000f)
			{
				return false;
			}
		}
		return true;
	}

	private void COMChanged()
	{
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			serverWheelData.forceDistance = GetWheelForceDistance(serverWheelData.wheel.wheelCollider);
		}
		prevLocalCOM = rBody.centerOfMass;
	}

	private void ComputeSteerAngle(float throttleInput, float steerInput, float dt, float speed)
	{
		if (vehicleSettings.tankSteering)
		{
			SteerAngle = 0f;
			ComputeTankSteeringThrottle(throttleInput, steerInput, speed);
			return;
		}
		float num = vehicleSettings.maxSteerAngle * steerInput;
		float num2 = Mathf.InverseLerp(0f, vehicleSettings.minSteerLimitSpeed, speed);
		if (vehicleSettings.steeringLimit)
		{
			float a = vehicleSettings.maxSteerAngle * car.GetSteerMaxMult(speed);
			float b = vehicleSettings.minSteerLimitAngle * car.GetSteerMaxMult(speed);
			float num3 = Mathf.Lerp(a, b, num2);
			num = Mathf.Clamp(num, 0f - num3, num3);
		}
		float num4 = 0f;
		if (vehicleSettings.steeringAssist)
		{
			float num5 = Mathf.InverseLerp(0.1f, 3f, speed);
			num4 = speedAngle * vehicleSettings.steeringAssistRatio * num5 * Mathf.InverseLerp(2f, 3f, Mathf.Abs(speedAngle));
		}
		float num6 = Mathf.Clamp(num + num4, 0f - vehicleSettings.maxSteerAngle, vehicleSettings.maxSteerAngle);
		if (SteerAngle == num6)
		{
			steerLerpSpeed = 0f;
			return;
		}
		float num7 = Mathf.Abs(SteerAngle / num6);
		float num8 = 1f - num2 * vehicleSettings.steerLimitLerpPenalty;
		bool steerSpeedMod = car.GetSteerSpeedMod(speed);
		if ((SteerAngle == 0f || Mathf.Sign(num6) == Mathf.Sign(SteerAngle)) && Mathf.Abs(num6) > Mathf.Abs(SteerAngle))
		{
			float num9 = SteerAngle / vehicleSettings.maxSteerAngle;
			float num10 = vehicleSettings.steerMinLerpSpeed;
			if (steerSpeedMod)
			{
				num10 *= 1.8f;
			}
			float num11 = Mathf.Lerp(num10 * num8, vehicleSettings.steerMaxLerpSpeed * num8, num9 * num9);
			if (Mathf.Abs(num6) > Mathf.Abs(SteerAngle) && num7 > 0.85f)
			{
				num11 = Mathf.Lerp(num11, 0f, num7);
			}
			if (!vehicleSettings.retainLerpSpeed || num11 > steerLerpSpeed)
			{
				steerLerpSpeed = num11;
			}
		}
		else
		{
			float num12 = vehicleSettings.steerReturnLerpSpeed;
			if (num6 != 0f && Mathf.Sign(num6) != Mathf.Sign(SteerAngle))
			{
				num12 *= 1.33f;
			}
			if (steerSpeedMod)
			{
				num12 *= 1.5f;
			}
			steerLerpSpeed = num12 * num8;
		}
		if (steerSpeedMod)
		{
			steerLerpSpeed *= 1.2f;
		}
		SteerAngle = Mathf.MoveTowards(SteerAngle, num6, dt * steerLerpSpeed);
	}

	private float GetWheelForceDistance(WheelCollider col)
	{
		return rBody.centerOfMass.y - transform.InverseTransformPoint(col.transform.position).y + col.radius + (1f - col.suspensionSpring.targetPosition) * col.suspensionDistance;
	}

	private void UpdateSuspension(ServerWheelData wd)
	{
		wd.isGrounded = wd.wheelCollider.GetGroundHit(out wd.hit);
		wd.origin = wd.wheelColliderTransform.TransformPoint(wd.wheelCollider.center);
		if (wd.isGrounded && GamePhysics.Trace(new Ray(wd.origin, -wd.wheelColliderTransform.up), 0f, out var hitInfo, wd.wheelCollider.suspensionDistance + wd.wheelCollider.radius, 1235321089, QueryTriggerInteraction.Ignore))
		{
			wd.hit.point = hitInfo.point;
			wd.hit.normal = hitInfo.normal;
		}
		if (wd.isGrounded)
		{
			if (wd.hit.force < 0f)
			{
				wd.hit.force = 0f;
			}
			wd.downforce = wd.hit.force;
		}
		else
		{
			wd.downforce = 0f;
		}
	}

	private void AdjustHitForces(int groundedWheels, float neutralForcePerWheel)
	{
		float num = neutralForcePerWheel * 0.25f;
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			if (!serverWheelData.isGrounded || !(serverWheelData.downforce < num))
			{
				continue;
			}
			if (groundedWheels == 1)
			{
				serverWheelData.downforce = num;
				continue;
			}
			float a = (num - serverWheelData.downforce) / (float)(groundedWheels - 1);
			serverWheelData.downforce = num;
			for (int j = 0; j < wheelData.Length; j++)
			{
				ServerWheelData serverWheelData2 = wheelData[j];
				if (serverWheelData2.isGrounded && serverWheelData2.downforce > num)
				{
					float num2 = Mathf.Min(a, serverWheelData2.downforce - num);
					serverWheelData2.downforce -= num2;
				}
			}
		}
	}

	private void UpdateLocalFrame(ServerWheelData wd, float dt)
	{
		if (!wd.isGrounded)
		{
			wd.hit.point = wd.origin - wd.wheelColliderTransform.up * (wd.wheelCollider.suspensionDistance + wd.wheelCollider.radius);
			wd.hit.normal = wd.wheelColliderTransform.up;
			wd.hit.collider = null;
		}
		Vector3 pointVelocity = rBody.GetPointVelocity(wd.hit.point);
		wd.velocity = pointVelocity - Vector3.Project(pointVelocity, wd.hit.normal);
		wd.localVelocity.y = Vector3.Dot(wd.hit.forwardDir, wd.velocity);
		wd.localVelocity.x = Vector3.Dot(wd.hit.sidewaysDir, wd.velocity);
		if (!wd.isGrounded)
		{
			wd.localRigForce = Vector2.zero;
			return;
		}
		float num = Mathf.InverseLerp(1f, 0.25f, wd.velocity.sqrMagnitude);
		Vector2 zero = default(Vector2);
		if (num > 0f)
		{
			float num2 = Vector3.Dot(Vector3.up, wd.hit.normal);
			Vector3 rhs;
			if (num2 > 1E-06f)
			{
				Vector3 vector = Vector3.up * wd.downforce / num2;
				rhs = vector - Vector3.Project(vector, wd.hit.normal);
			}
			else
			{
				rhs = Vector3.up * 100000f;
			}
			zero.y = Vector3.Dot(wd.hit.forwardDir, rhs);
			zero.x = Vector3.Dot(wd.hit.sidewaysDir, rhs);
			zero *= num;
		}
		else
		{
			zero = Vector2.zero;
		}
		Vector2 vector2 = (0f - Mathf.Clamp(wd.downforce / (0f - UnityEngine.Physics.gravity.y), 0f, wd.wheelCollider.sprungMass) * 0.5f) * wd.localVelocity / dt;
		wd.localRigForce = vector2 + zero;
	}

	private void ComputeTyreForces(ServerWheelData wd, float speed, float maxDriveForce, float maxSpeed, float throttleInput, float brakeInput, float driveForceMultiplier, bool handbrakeInput, float dt)
	{
		float num = Mathf.Abs(speed);
		bool flag = (wd.isHandbrakeLocked = handbrakeInput && !wd.isFrontWheel);
		if (flag)
		{
			brakeInput = 1f;
		}
		float target = (flag ? vehicleSettings.handbrakeGripMultiplier : 1f);
		float num2;
		if (flag)
		{
			num2 = vehicleSettings.driftLerpSpeed;
		}
		else
		{
			float t = Mathf.InverseLerp(0f, 10f, num);
			num2 = Mathf.Lerp(vehicleSettings.driftRecoverySpeed, vehicleSettings.driftRecoverySpeed * 0.15f, t);
		}
		wd.handbrakeGripCurrent = Mathf.MoveTowards(wd.handbrakeGripCurrent, target, dt * num2);
		if (vehicleSettings.tankSteering && brakeInput == 0f)
		{
			throttleInput = ((!wd.isLeftWheel) ? TankThrottleRight : TankThrottleLeft);
		}
		float num3 = (wd.wheel.powerWheel ? throttleInput : 0f);
		wd.hasThrottleInput = num3 != 0f;
		float num4 = vehicleSettings.maxDriveSlip;
		if (Mathf.Sign(num3) != Mathf.Sign(wd.localVelocity.y))
		{
			num4 -= wd.localVelocity.y * Mathf.Sign(num3);
		}
		float num5 = Mathf.Abs(num3);
		float num6 = 0f - vehicleSettings.rollingResistance + num5 * (1f + vehicleSettings.rollingResistance) - brakeInput * (1f - vehicleSettings.rollingResistance);
		if (InSlowSpeedExitMode || num6 < 0f || maxDriveForce == 0f)
		{
			num6 *= -1f;
			wd.isBraking = true;
		}
		else
		{
			num6 *= Mathf.Sign(num3);
			wd.isBraking = false;
		}
		float num8;
		if (wd.isBraking)
		{
			float num7 = Mathf.Clamp(car.GetMaxForwardSpeed() * vehicleSettings.brakeForceMultiplier, 10f * vehicleSettings.brakeForceMultiplier, 50f * vehicleSettings.brakeForceMultiplier);
			num7 += rBody.mass * 1.5f;
			if (flag)
			{
				float t2 = Mathf.InverseLerp(20f, 60f, Mathf.Abs(speedAngle));
				num7 *= Mathf.Lerp(1f, 0.4f, t2);
			}
			num8 = num6 * num7;
		}
		else
		{
			num8 = ComputeDriveForce(speed, num, num6 * maxDriveForce, maxDriveForce, maxSpeed, driveForceMultiplier);
		}
		if (wd.isGrounded)
		{
			wd.tyreSlip.x = wd.localVelocity.x;
			wd.tyreSlip.y = wd.localVelocity.y - wd.angularVelocity * wd.wheelCollider.radius;
			float num9 = car.OnSurface switch
			{
				VehicleTerrainHandler.Surface.Road => 1f, 
				VehicleTerrainHandler.Surface.Ice => 0.25f, 
				VehicleTerrainHandler.Surface.Frictionless => 0f, 
				_ => 0.75f, 
			};
			float num10 = wd.wheel.tyreFriction * wd.downforce * num9;
			if (num > 1.5f && car is IHandbrakeCar)
			{
				float b;
				if (wd.isFrontWheel)
				{
					float t3 = Mathf.InverseLerp(10f, 16f, num);
					b = Mathf.Lerp(vehicleSettings.frontTraction, 0.35f, t3);
				}
				else
				{
					b = vehicleSettings.rearTraction;
				}
				float t4 = Mathf.InverseLerp(65f, 160f, Mathf.Abs(speedAngle));
				num10 *= Mathf.Lerp(1f, b, t4);
			}
			float num11 = 0f;
			if (!wd.isBraking)
			{
				num11 = Mathf.Min(Mathf.Abs(num8 * wd.tyreSlip.x) / num10, num4);
				if (num8 != 0f && num11 < 0.1f)
				{
					num11 = 0.1f;
				}
			}
			if (Mathf.Abs(wd.tyreSlip.y) < num11)
			{
				wd.tyreSlip.y = num11 * Mathf.Sign(wd.tyreSlip.y);
			}
			Vector2 vector = (0f - num10) * wd.tyreSlip.normalized;
			vector.x = Mathf.Abs(vector.x) * 1.5f;
			vector.y = Mathf.Abs(vector.y);
			vector.x *= wd.handbrakeGripCurrent;
			wd.tyreForce.x = Mathf.Clamp(wd.localRigForce.x, 0f - vector.x, vector.x);
			if (wd.isBraking)
			{
				float num12 = Mathf.Min(vector.y, num8);
				wd.tyreForce.y = Mathf.Clamp(wd.localRigForce.y, 0f - num12, num12);
			}
			else
			{
				wd.tyreForce.y = Mathf.Clamp(num8, 0f - vector.y, vector.y);
			}
		}
		else
		{
			wd.tyreSlip = Vector2.zero;
			wd.tyreForce = Vector2.zero;
		}
		if (wd.isGrounded)
		{
			float num13;
			if (flag)
			{
				num13 = 0f - wd.localVelocity.y;
			}
			else if (wd.isBraking)
			{
				num13 = 0f;
			}
			else
			{
				float driveForceToMaxSlip = vehicleSettings.driveForceToMaxSlip;
				num13 = Mathf.Clamp01((Mathf.Abs(num8) - Mathf.Abs(wd.tyreForce.y)) / driveForceToMaxSlip) * num4 * Mathf.Sign(num8);
			}
			wd.angularVelocity = (wd.localVelocity.y + num13) / wd.wheelCollider.radius;
		}
		else
		{
			float num14 = 50f;
			float num15 = 10f;
			if (num3 > 0f)
			{
				wd.angularVelocity += num14 * num3;
			}
			else
			{
				wd.angularVelocity -= num15;
			}
			wd.angularVelocity -= num14 * brakeInput;
			wd.angularVelocity = Mathf.Clamp(wd.angularVelocity, 0f, maxSpeed / wd.wheelCollider.radius);
		}
	}

	private void ComputeTankSteeringThrottle(float throttleInput, float steerInput, float speed)
	{
		TankThrottleLeft = throttleInput;
		TankThrottleRight = throttleInput;
		float tankSteerInvert = GetTankSteerInvert(throttleInput, speed);
		if (throttleInput == 0f)
		{
			TankThrottleLeft = 0f - steerInput;
			TankThrottleRight = steerInput;
		}
		else if (steerInput > 0f)
		{
			TankThrottleLeft = Mathf.Lerp(throttleInput, -1f * tankSteerInvert, steerInput);
			TankThrottleRight = Mathf.Lerp(throttleInput, 1f * tankSteerInvert, steerInput);
		}
		else if (steerInput < 0f)
		{
			TankThrottleLeft = Mathf.Lerp(throttleInput, 1f * tankSteerInvert, 0f - steerInput);
			TankThrottleRight = Mathf.Lerp(throttleInput, -1f * tankSteerInvert, 0f - steerInput);
		}
	}

	private float ComputeDriveForce(float speed, float absSpeed, float demandedForce, float maxForce, float maxForwardSpeed, float driveForceMultiplier)
	{
		float num = ((speed >= 0f) ? maxForwardSpeed : (maxForwardSpeed * vehicleSettings.reversePercentSpeed));
		if (absSpeed < num)
		{
			if ((speed >= 0f || demandedForce <= 0f) && (speed <= 0f || demandedForce >= 0f))
			{
				maxForce = car.GetAdjustedDriveForce(absSpeed, maxForwardSpeed) * driveForceMultiplier;
			}
			return Mathf.Clamp(demandedForce, 0f - maxForce, maxForce);
		}
		float num2 = maxForce * Mathf.Max(1f - absSpeed / num, -1f) * Mathf.Sign(speed);
		if ((speed < 0f && demandedForce > 0f) || (speed > 0f && demandedForce < 0f))
		{
			num2 = Mathf.Clamp(num2 + demandedForce, 0f - maxForce, maxForce);
		}
		return num2;
	}

	private void ComputeOverallForces()
	{
		DriveWheelVelocity = 0f;
		DriveWheelSlip = 0f;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < wheelData.Length; i++)
		{
			ServerWheelData serverWheelData = wheelData[i];
			if (serverWheelData.wheel.powerWheel)
			{
				if (!serverWheelData.isHandbrakeLocked)
				{
					DriveWheelVelocity += serverWheelData.angularVelocity;
					num++;
				}
				if (serverWheelData.isGrounded)
				{
					float num3 = ComputeCombinedSlip(serverWheelData.localVelocity, serverWheelData.tyreSlip);
					DriveWheelSlip += num3;
				}
				num2++;
			}
		}
		if (num > 0)
		{
			DriveWheelVelocity /= num;
		}
		if (num2 > 0)
		{
			DriveWheelSlip /= num2;
		}
	}

	private static float ComputeCombinedSlip(Vector2 localVelocity, Vector2 tyreSlip)
	{
		float magnitude = localVelocity.magnitude;
		if (magnitude > 0.01f)
		{
			float num = tyreSlip.x * localVelocity.x / magnitude;
			float y = tyreSlip.y;
			return Mathf.Sqrt(num * num + y * y);
		}
		return tyreSlip.magnitude;
	}

	private void ApplyTyreForces(ServerWheelData wd)
	{
		if (wd.isGrounded)
		{
			Vector3 force = wd.hit.forwardDir * wd.tyreForce.y;
			Vector3 force2 = wd.hit.sidewaysDir * wd.tyreForce.x;
			Vector3 sidewaysForceAppPoint = GetSidewaysForceAppPoint(wd, wd.hit.point);
			rBody.AddForceAtPosition(force, wd.hit.point, ForceMode.Force);
			rBody.AddForceAtPosition(force2, sidewaysForceAppPoint, ForceMode.Force);
		}
	}

	private Vector3 GetSidewaysForceAppPoint(ServerWheelData wd, Vector3 contactPoint)
	{
		Vector3 result = contactPoint + wd.wheelColliderTransform.up * vehicleSettings.antiRoll * wd.forceDistance;
		float num = (wd.wheel.steerWheel ? SteerAngle : 0f);
		if (num != 0f && Mathf.Sign(num) != Mathf.Sign(wd.tyreSlip.x))
		{
			result += wd.wheelColliderTransform.forward * midWheelPos * (vehicleSettings.handlingBias - 0.5f);
		}
		return result;
	}

	private float GetTankSteerInvert(float throttleInput, float speed)
	{
		float result = 1f;
		if (throttleInput < 0f && speed < 1.75f)
		{
			result = -1f;
		}
		else if (throttleInput == 0f && speed < -1f)
		{
			result = -1f;
		}
		else if (speed < -1f)
		{
			result = -1f;
		}
		return result;
	}
}
