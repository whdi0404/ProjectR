using UnityEngine;

public class SingletonBehaviour : MonoBehaviour
{
	protected static GameObject DontDestroyRoot;
}

public class SingletonBehaviour<T> : SingletonBehaviour where T : SingletonBehaviour<T>
{
	protected static bool isApplicationQuitting = false;
	private static T instance;

	public static bool Exist
	{
		get
		{
			return instance != null;
		}
	}

	public static T Instance
	{
		get
		{
			if (instance == null)
				instance = FindObjectOfType<T>();

			if (instance == null)
				instance = InstantiatePrefab(false);

			if (instance == null)
				instance = CreateInstance();

			return instance;
		}
	}

	public void Create()
	{
	}

	public static T OpenPrefab()
	{
		return InstantiatePrefab(true);
	}

	public static T OpenPrefab(Transform parent)
	{
		return InstantiatePrefab(true, parent);
	}

	private static T InstantiatePrefab(bool forceCreate, Transform parent = null)
	{
		var singletonAttribute = GetSingletonAttribute();
		if (singletonAttribute == null)
			return null;

		if (string.IsNullOrEmpty(singletonAttribute.PrefabPath))
			return null;

		if (!singletonAttribute.CreateInstance && !forceCreate)
			return null;

		GameObject prefab = Resources.Load<GameObject>(singletonAttribute.PrefabPath);
		if (prefab == null)
		{
			Debug.LogWarningFormat("{0} singleton prefab not found: {1}", typeof(T).Name, singletonAttribute.PrefabPath);
			return null;
		}

		if (isApplicationQuitting)
		{
			Debug.LogWarning("Could not create singleton instance : Application is already quited");
			return null;
		}

		if (parent != null)
			return GameObject.Instantiate(prefab, parent).GetComponent<T>();
		else
			return GameObject.Instantiate(prefab).GetComponent<T>();
	}

	private static T CreateInstance()
	{
		var singletonAttribute = GetSingletonAttribute();
		if (singletonAttribute == null)
			return null;

		if (!singletonAttribute.CreateInstance)
			return null;

		if (isApplicationQuitting)
		{
			Debug.LogWarning("Could not create singleton instance : Application is already quited");
			return null;
		}

		return new GameObject(typeof(T).Name).AddComponent<T>();
	}

	private static SingletonAttribute GetSingletonAttribute()
	{
		var attributes = typeof(T).GetCustomAttributes(typeof(SingletonAttribute), true);

		if (attributes.Length > 0)
			return attributes[0] as SingletonAttribute;
		else
			return null;
	}

	protected virtual void Awake()
	{
		if (instance == null)
			instance = (T)this;

		if (this != instance)
		{
			Debug.LogWarningFormat("Destroying duplicated singleton instance {0}", this);
			Destroy(this);
		}
		else
		{
			var singletonAttribute = GetSingletonAttribute();
			if ( singletonAttribute != null && singletonAttribute.DontDestroyOnLoad )
			{
				if ( DontDestroyRoot == null )
				{
					DontDestroyRoot = GameObject.Find("DontDestroyOnLoad" );
				}

				if ( DontDestroyRoot == null )
				{
					DontDestroyRoot = new GameObject( "DontDestroyOnLoad" );
					DontDestroyOnLoad( DontDestroyRoot );
				}
				transform.SetParent( DontDestroyRoot.transform );
			}
		}
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		if (this == instance)
			instance = null;
	}

	protected void OnApplicationQuit()
	{
		isApplicationQuitting = true;
	}
}