﻿//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace M4u
{
	/// <summary>
	/// M4uInputFieldBinding. Bind InputField
	/// </summary>
	[AddComponentMenu("M4u/InputFieldBinding")]
	public class M4uInputFieldBinding : M4uBindingSingle
	{
		public string Format = "";

		private InputField ui;

		public override void Start()
		{
			base.Start();

			ui = GetComponent<InputField>();
			OnChange();
		}

		public override void OnChange()
		{
			base.OnChange();

			ui.text = string.Format(Format, Values[0]);
		}

		public override string ToString()
		{
			return "InputField.text=" + string.Format(Format, GetBindStr(Path));
		}
	}
}