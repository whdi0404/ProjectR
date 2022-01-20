using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Table.Editor
{
	[CustomEditor(typeof(GoogleSheetImporter), true)]
	public class GoogleSheetImporterEditor : OdinEditor
	{
		private GoogleSheetImporter t;

		protected override void OnEnable()
		{
			base.OnEnable();
			t = target as GoogleSheetImporter;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (t.SpreadSheetInfoList == null)
				return;

			if (GUILayout.Button("Get All Info") == true)
				t.SpreadSheetInfoList.ForEach(sheetInfo => sheetInfo.GetInfo());

			if (GUILayout.Button("Update All") == true)
				UpdateSheet();
		}

		public void UpdateSheet()
		{
			foreach (var sheetInfo in t.SpreadSheetInfoList)
				sheetInfo.Update();
		}
	}
}