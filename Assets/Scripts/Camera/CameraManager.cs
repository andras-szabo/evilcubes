﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour, IManager, IShakeable
{
	[Range(0f, 1f)] public float shakeIntensityFactor;
	private Dictionary<int, CameraManagerClient> _camerasByID = new Dictionary<int, CameraManagerClient>();

	int _shakingCameraCount;
	bool _shakeCancelToken;

	public void Cleanup()
	{
		if (this != null)
		{
			StopAllCoroutines();
			_camerasByID.Clear();
		}
	}

	private void Awake()
	{
		ManagerLocator.TryRegister<CameraManager>(this);
	}

	private void Start()
	{
		ManagerLocator.TryGet<GameController>().OnGameOver += HandleGameOver;
	}

	private void OnDestroy()
	{
		_camerasByID.Clear();
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver -= HandleGameOver;
		}
	}

	private void HandleGameOver(GameController.GameResult hasPlayerWon)
	{
		_shakeCancelToken = true;
	}

	public void Register(int id, CameraManagerClient camera)
	{
		if (!_camerasByID.ContainsKey(id))
		{
			_camerasByID.Add(id, camera);
		}
	}

	public Camera TryGetCamera(int camID)
	{
		CameraManagerClient cam;

		if (_camerasByID.TryGetValue(camID, out cam))
		{
			return cam.Camera;
		}

		return null;
	}

	public void Shake(float intensity)
	{
		if (_shakingCameraCount == 0 && shakeIntensityFactor > 0f)
		{
			_shakeCancelToken = false;

			foreach (var cam in _camerasByID.Values)
			{
				StartCoroutine(ShakeRoutine(cam, intensity));
				_shakingCameraCount++;
			}
		}
	}

	private IEnumerator ShakeRoutine(CameraManagerClient cam, float intensity)
	{
		var startPos = cam.Position;

		var elapsed = 0f;
		var duration = intensity * 0.02f;
		intensity *= shakeIntensityFactor;

		while (!_shakeCancelToken && cam.CanShake && elapsed < duration)
		{
			elapsed += Time.deltaTime;

			var deltaX = Random.Range(-intensity, intensity);
			var deltaY = Random.Range(-intensity, intensity);

			cam.Position = startPos + new Vector3(deltaX, deltaY);

			yield return null;
		}

		cam.Position = startPos;
		_shakingCameraCount--;
	}
}
