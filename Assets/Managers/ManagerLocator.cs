using System.Collections.Generic;

public class ManagerLocator
{
	private static ManagerLocator _instance;

	public static T TryGet<T>() where T : class, IManager
	{
		return Instance.TryGetManager<T>();
	}

	public static bool TryRegister<T>(T manager) where T : class, IManager
	{
		return Instance.TryRegisterManager<T>(manager);
	}

	public static void Cleanup()
	{
		_instance._managers.Clear();
		_instance = null;
	}

	private static ManagerLocator Instance
	{
		get
		{
			return _instance ?? (_instance = new ManagerLocator());
		}
	}

	private Dictionary<System.Type, IManager> _managers = new Dictionary<System.Type, IManager>();

	private ManagerLocator()
	{
		CreateDefaultManagers();
	}

	private void CreateDefaultManagers()
	{
		TryRegisterManager<HitManager>(new HitManager());
	}

	private T TryGetManager<T>() where T : class, IManager
	{
		IManager manager;

		if (_managers.TryGetValue(typeof(T), out manager))
		{
			return (T)manager;
		}

		return null;
	}

	private bool TryRegisterManager<T>(T manager) where T : class, IManager
	{
		var key = typeof(T);
		
		if (_managers.ContainsKey(key))
		{
			return false;
		}

		_managers.Add(key, manager);
		return true;
	}
}

public interface IManager
{

}
