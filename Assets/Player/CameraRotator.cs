﻿using System.Collections;
using UnityEngine;

public class CameraRotator : MonoWithCachedTransform
{
	public enum ViewPosition
	{
		FirstPerson,
		ThirdPerson
	};

	[Range(1f, 500f)] public float vertSensitivity = 100f;
	[Range(1f, 500f)] public float horiSensitivity = 100f;
	[Range(0f, 2f)] public float perspectiveSwitchDurationSeconds = 1f;
	[Range(0.1f, 1f)] public float turnAroundSeconds = 0.25f;
	public ViewPosition startingViewPosition;

	public bool invertMouseVertical = true;

	public Transform body;
	public Transform head;

	public Transform firstPersonCamPositionMarker;
	public Transform thirdPersonCamPositionMarker;

	private Coroutine _perspectiveSwitchRoutine;
	private ViewPosition _currentViewPos;
	private Coroutine _turnaroundRoutine;

	private void Awake()
	{
		SetPerspective(startingViewPosition, animate: false);
	}

	public void DoQuickTurnaround()
	{
		if (_turnaroundRoutine != null)
		{
			StopCoroutine(_turnaroundRoutine);
		}

		_turnaroundRoutine = StartCoroutine(QuickTurnAroundRoutine());
	}

	public void TogglePerspective()
	{
		SetPerspective(_currentViewPos == ViewPosition.FirstPerson ? ViewPosition.ThirdPerson : ViewPosition.FirstPerson);
	}

	public void SetPerspective(ViewPosition newPosition, bool animate = true)
	{
		if (_perspectiveSwitchRoutine != null)
		{
			StopCoroutine(_perspectiveSwitchRoutine);
		}

		_perspectiveSwitchRoutine = StartCoroutine(SwitchPerspectiveRoutine(newPosition, animate));
	}

	public IEnumerator SwitchPerspectiveRoutine(ViewPosition newPosition, bool animate)
	{
		var target = newPosition == ViewPosition.FirstPerson ? firstPersonCamPositionMarker : thirdPersonCamPositionMarker;

		var targetPosition = target.localPosition;
		var startPosition = CachedTransform.localPosition;
		var elapsedSeconds = 0f;
		var duration = animate ? perspectiveSwitchDurationSeconds : 0f;

		_currentViewPos = newPosition;

		while (elapsedSeconds < duration && duration > 0f)
		{
			elapsedSeconds += Time.deltaTime;
			CachedTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedSeconds / duration);
			yield return null;
		}

		CachedTransform.localPosition = targetPosition;

		_perspectiveSwitchRoutine = null;
	}

	public void ApplyInput(float inputV, float inputH)
	{
		if (_turnaroundRoutine == null)
		{
			RotateBody(inputH);
		}

		RotateHead(invertMouseVertical ? inputV : -inputV);
	}

	private void RotateBody(float yaw)
	{
		var deltaEulerAngles = new Vector3(0f, yaw, 0f) * horiSensitivity * Time.deltaTime;
		body.Rotate(deltaEulerAngles);
	}

	private void RotateHead(float pitch)
	{
		var deltaEulerAngles = new Vector3(pitch, 0f, 0f) * vertSensitivity * Time.deltaTime;
		head.Rotate(deltaEulerAngles);
	}

	private IEnumerator QuickTurnAroundRoutine()
	{
		var forward = CachedTransform.forward;
		var bodyForward = new Vector3(forward.x, 0f, forward.z).normalized;
		var startBodyRotation = Quaternion.LookRotation(bodyForward, Vector3.up);
		var targetBodyRotation = Quaternion.LookRotation(-bodyForward, Vector3.up);

		var elapsed = 0f;
		var duration = turnAroundSeconds;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			var current = Quaternion.Slerp(startBodyRotation, targetBodyRotation, elapsed / duration);
			body.rotation = current;
			yield return null;
		}

		body.rotation = targetBodyRotation;
		
		_turnaroundRoutine = null;
	}
}
