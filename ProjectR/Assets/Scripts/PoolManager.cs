using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GOPoolManager : SingletonBehaviour<GOPoolManager>
{
	private class GameObjectPool
	{
		private class PoolObject : MonoBehaviour
		{
			private GameObjectPool pool;

			public void Init(GameObjectPool pool)
			{
				this.pool = pool;
			}

			protected void OnDisable()
			{
				Instance.StartCoroutine(After1FrameOnDisable());
			}

			protected void OnDestroy()
			{
				pool.Destroy(this);
			}

			private IEnumerator After1FrameOnDisable()
			{
				yield return null;
				pool.Push(this);
			}
		}

		private GameObject original;
		private List<PoolObject> deactiveObjects = new List<PoolObject>();

		public void Init(GameObject original)
		{
			this.original = original;
			original.transform.SetParent(Instance.transform);
			original.SetActive(false);
		}

		private void Push(PoolObject poolObj)
		{
			deactiveObjects.Add(poolObj);
			poolObj.transform.SetParent(Instance.transform);
		}

		public GameObject Pop()
		{
			PoolObject newObj = null;
			if (deactiveObjects.Count > 0)
			{
				newObj = deactiveObjects[deactiveObjects.Count - 1];
				deactiveObjects.RemoveAt(deactiveObjects.Count - 1);
			}
			else
			{
				GameObject obj = Object.Instantiate(original);
				newObj = obj.AddComponent<PoolObject>();
				newObj.Init(this);
			}

			newObj.gameObject.SetActive(true);
			newObj.transform.SetParent(null);
			SceneManager.MoveGameObjectToScene(newObj.gameObject, SceneManager.GetActiveScene());

			return newObj.gameObject;
		}

		private void Destroy(PoolObject poolObj)
		{
			deactiveObjects.Remove(poolObj);
		}
	}

	private SmartDictionary<string, GameObjectPool> poolDict = new SmartDictionary<string, GameObjectPool>();

	public bool Init(string poolId, GameObject original)
	{
		if (poolDict.ContainsKey(poolId) == true)
			return false;

		GameObjectPool pool = new GameObjectPool();
		pool.Init(original);

		poolDict.Add(poolId, pool);

		return true;
	}

	public GameObject Pop(string poolId)
	{
		return poolDict[poolId]?.Pop();
	}
}