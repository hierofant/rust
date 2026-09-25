using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public struct PlayerAnimationHandle : IEquatable<PlayerAnimationHandle>
{
	public AnimationClipPlayable Playable { get; private set; }

	public AnimatorControllerPlayable Controller { get; private set; }

	public AnimationLayerMixerPlayable LayerMixer { get; private set; }

	public PlayableGraph Graph { get; private set; }

	public float Length { get; private set; }

	public readonly bool Valid
	{
		get
		{
			if (!Playable.IsValid())
			{
				return Controller.IsValid();
			}
			return true;
		}
	}

	public int InputPort { get; private set; }

	public AvatarMask CurrentMask { get; private set; }

	public static PlayerAnimationHandle InvalidHandle => default(PlayerAnimationHandle);

	public bool Equals(PlayerAnimationHandle other)
	{
		if (Playable.Equals(other.Playable) && LayerMixer.Equals(other.LayerMixer) && Graph.Equals(other.Graph) && Length.Equals(other.Length))
		{
			return InputPort == other.InputPort;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlayerAnimationHandle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Playable, LayerMixer, Graph, Length, InputPort);
	}

	public readonly void SetProgress(float progress)
	{
		if (Valid && Playable.IsValid() && Graph.IsValid())
		{
			progress = Mathf.Clamp01(progress);
			Playable.SetTime(progress * Length);
		}
	}

	public readonly void SetApplyFootIK(bool apply)
	{
		if (Playable.IsValid())
		{
			Playable.SetApplyFootIK(apply);
		}
	}

	public readonly void SetTime(float time)
	{
		if (Valid && Playable.IsValid())
		{
			time = Mathf.Clamp(time, 0f, Length);
			Playable.SetTime(time);
		}
	}

	public readonly void Play()
	{
		if (Valid && Playable.IsValid())
		{
			Playable.Play();
		}
	}

	public readonly void PlayFromStart()
	{
		if (Playable.IsValid())
		{
			Playable.SetTime(0.0);
			Playable.Play();
		}
	}

	public readonly void SetWeight(float weight)
	{
		if (Valid && LayerMixer.IsValid())
		{
			LayerMixer.SetInputWeight(InputPort, weight);
		}
	}

	public float GetWeight()
	{
		if (!Valid)
		{
			return 0f;
		}
		return LayerMixer.GetInputWeight(InputPort);
	}

	public void SetMask(AvatarMask newMask)
	{
		if (!(newMask == null) && Valid && LayerMixer.IsValid())
		{
			LayerMixer.SetLayerMaskFromAvatarMask((uint)InputPort, newMask);
			CurrentMask = newMask;
		}
	}

	public readonly void Pause()
	{
		if (Valid && Playable.IsValid())
		{
			Playable.Pause();
		}
	}

	public readonly bool IsPlaying()
	{
		if (!Valid)
		{
			return false;
		}
		return Playable.GetPlayState() == PlayState.Playing;
	}

	public void Dispose()
	{
		if (Valid && Graph.IsValid())
		{
			LayerMixer.DisconnectInput(InputPort);
			if (Playable.IsValid())
			{
				Graph.DestroyPlayable(Playable);
			}
			if (Controller.IsValid())
			{
				Graph.DestroyPlayable(Controller);
			}
		}
	}

	public readonly float GetNormalizedTime()
	{
		if (!Valid || !Playable.IsValid() || !Graph.IsValid())
		{
			return 0f;
		}
		return (float)(Playable.GetTime() / (double)Length);
	}

	public static PlayerAnimationHandle Create(AnimationClip clip, PlayableGraph playableGraph, AnimationLayerMixerPlayable layerMixer, int inputPort, AvatarMask mask, bool additive, bool autoPlay = true, float initialWeight = 1f)
	{
		PlayerAnimationHandle result = default(PlayerAnimationHandle);
		layerMixer.DisconnectInput(inputPort);
		result.Length = clip.length;
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Playable = AnimationClipPlayable.Create(playableGraph, clip);
		result.CurrentMask = mask;
		layerMixer.ConnectInput(inputPort, result.Playable, 0);
		layerMixer.SetInputWeight(inputPort, initialWeight);
		layerMixer.SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		layerMixer.SetLayerAdditive((uint)inputPort, additive);
		if (!autoPlay)
		{
			result.Playable.Pause();
		}
		else
		{
			result.Playable.Play();
		}
		return result;
	}

	public static PlayerAnimationHandle Create(RuntimeAnimatorController controller, PlayableGraph playableGraph, AnimationLayerMixerPlayable layerMixer, int inputPort, AvatarMask mask, bool additive, float initialWeight = 1f)
	{
		PlayerAnimationHandle result = default(PlayerAnimationHandle);
		layerMixer.DisconnectInput(inputPort);
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Controller = AnimatorControllerPlayable.Create(playableGraph, controller);
		result.CurrentMask = mask;
		layerMixer.ConnectInput(inputPort, result.Controller, 0);
		layerMixer.SetInputWeight(inputPort, initialWeight);
		layerMixer.SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		layerMixer.SetLayerAdditive((uint)inputPort, additive);
		return result;
	}
}
