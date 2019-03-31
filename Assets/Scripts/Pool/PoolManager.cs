using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoWithCachedTransform, IManager
{
	private Dictionary<int, Queue<GameObject>> _poolsByPrototypeID = new Dictionary<int, Queue<GameObject>>();
	private Queue<GameObject> _workingQueue;

	private Dictionary<int, int> _prototypeIDsByInstance = new Dictionary<int, int>();

	public void Cleanup()
	{
		_poolsByPrototypeID.Clear();
		_workingQueue?.Clear();
		_prototypeIDsByInstance.Clear();
	}

	private void Awake()
	{
		ManagerLocator.TryRegister<PoolManager>(this);
	}

	public T Spawn<T>(T prototype, Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour
	{
		var prototypeID = prototype.gameObject.GetInstanceID();

		if (_poolsByPrototypeID.TryGetValue(prototypeID, out _workingQueue) && _workingQueue.Count > 0)
		{
			var pooledInstance = _workingQueue.Dequeue();
					   
			_prototypeIDsByInstance[pooledInstance.gameObject.GetInstanceID()] = prototypeID;
			pooledInstance.transform.SetParent(parent);
			pooledInstance.transform.SetPositionAndRotation(position, rotation);
			pooledInstance.gameObject.SetActive(true);

			return pooledInstance.GetComponent<T>();
		}

		var newInstance = Instantiate<T>(prototype, position, rotation, parent);
		_prototypeIDsByInstance[newInstance.gameObject.GetInstanceID()] = prototypeID;

		if (_workingQueue == null)
		{
			_poolsByPrototypeID[prototypeID] = new Queue<GameObject>();
		}

		return newInstance;
	}

	public void Despawn<T>(T instance) where T : MonoBehaviour
	{
		int prototypeID;
		if (_prototypeIDsByInstance.TryGetValue(instance.gameObject.GetInstanceID(), out prototypeID))
		{
			instance.gameObject.SetActive(false);
			instance.transform.SetParent(CachedTransform);
			_poolsByPrototypeID[prototypeID].Enqueue(instance.gameObject);
		}
		else
		{
			Destroy(instance.gameObject);
		}
	}
}
