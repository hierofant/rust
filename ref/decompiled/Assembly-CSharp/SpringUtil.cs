using System;
using UnityEngine;

public class SpringUtil
{
	public static void SimpleDampedSpring(ref Vector3 val, ref Vector3 velocity, Vector3 targetValue, float dampening, float frequency, float delta)
	{
		SimpleDampedSpring(ref val.x, ref velocity.x, targetValue.x, dampening, frequency, delta);
		SimpleDampedSpring(ref val.y, ref velocity.y, targetValue.y, dampening, frequency, delta);
		SimpleDampedSpring(ref val.z, ref velocity.z, targetValue.z, dampening, frequency, delta);
	}

	public static void SimpleDampedSpring(ref float val, ref float velocity, float targetValue, float dampening, float frequency, float delta)
	{
		float num = 1f + 2f * delta * dampening * frequency;
		float num2 = frequency * frequency;
		float num3 = delta * num2;
		float num4 = delta * num3;
		float num5 = 1f / (num + num4);
		float num6 = num * val + delta * velocity + num4 * targetValue;
		float num7 = velocity + num3 * (targetValue - val);
		val = num6 * num5;
		velocity = num7 * num5;
	}

	public static void DampedSpring(ref float val, ref float velocity, float targetValue, float reductionAmount, float reductionDuration, float frequency, float delta)
	{
		float num = FrequencyInHertz(frequency);
		float dampening = DampeningInDuration(reductionAmount, reductionDuration, num);
		SimpleDampedSpring(ref val, ref velocity, targetValue, dampening, num, delta);
	}

	public static void DampedSpring(ref Vector3 val, ref Vector3 velocity, Vector3 targetValue, float reductionAmount, float reductionDuration, float frequency, float delta)
	{
		float num = FrequencyInHertz(frequency);
		float dampening = DampeningInDuration(reductionAmount, reductionDuration, num);
		SimpleDampedSpring(ref val.x, ref velocity.x, targetValue.x, dampening, num, delta);
		SimpleDampedSpring(ref val.y, ref velocity.y, targetValue.y, dampening, num, delta);
		SimpleDampedSpring(ref val.z, ref velocity.z, targetValue.z, dampening, num, delta);
	}

	private static float FrequencyInHertz(float rawFrequency)
	{
		return MathF.PI * 2f * rawFrequency;
	}

	private static float DampeningInDuration(float reductionRate, float duration, float freqHertz)
	{
		float num = Mathf.Log(reductionRate);
		float num2 = freqHertz * (0f - duration);
		return num / num2;
	}
}
