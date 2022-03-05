using M4u;
using System;
using System.Collections.Generic;
using UnityEngine;

//Singleton���� ���� ��, Canvas�� �߰� ����
[Singleton(CreateInstance = true, DontDestroyOnLoad = true, PrefabPath = "")]
public class UIManager : SingletonBehaviour<UIManager>
{
	[SerializeField]
	private Transform uiRoot;

	public Transform UIRoot { get => uiRoot; }

	private SmartDictionary<Type, GameObject> cache = new SmartDictionary<Type, GameObject>();

	public UIRootContext RootContext { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		RootContext = new UIRootContext();
		GetComponent<M4uContextRoot>().Context = RootContext;
	}
}