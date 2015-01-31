using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditorInternal;

namespace UnityEditor
{
	[CustomEditor (typeof (SortingGroup))]
	internal class SortingGroupEditor : Editor
	{
		ReorderableList list;
		SerializedProperty sortingLayerID;
		SerializedProperty useIsometricSorting;

		protected static MethodInfo meth_SortingLayerField;

		static Vector2 scrollArea;

		void OnEnable()
		{
			var sortingGroup = target as SortingGroup;
			sortingGroup.Refresh ();

			list = new ReorderableList(serializedObject,
				serializedObject.FindProperty("rendererInfos"),
				true, false, false, false);

			sortingLayerID = serializedObject.FindProperty("sortingLayerID");
			useIsometricSorting = serializedObject.FindProperty("useIsometricSorting");

			// Get SortingLayerField method
			var editorTypes = typeof(Editor).Assembly.GetTypes();		
			var type = editorTypes.FirstOrDefault(t => t.Name == "EditorGUILayout");
			meth_SortingLayerField = type.GetMethod("SortingLayerField",(BindingFlags.Static | BindingFlags.NonPublic));
		}

		public override void OnInspectorGUI()
		{
			var sortingGroup = target as SortingGroup;

			serializedObject.Update();

			//scrollArea = GUILayout.BeginScrollView (scrollArea);
			list.DoLayoutList();
			//GUILayout.EndScrollView();

			EditorGUI.BeginChangeCheck();
			SortingLayerField (new GUIContent("Sorting Layer"), sortingLayerID, EditorStyles.popup);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (var renderer in sortingGroup.GetComponentsInChildren<Renderer>())
				{
					var so = new SerializedObject(renderer);
					so.FindProperty("m_SortingLayerID").intValue = sortingLayerID.intValue;
					so.ApplyModifiedProperties();
				}
			}
			EditorGUILayout.PropertyField(useIsometricSorting, new GUIContent("Isometric Sorting"));
			serializedObject.ApplyModifiedProperties();
		}

		static void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style)
		{
			var parameters = new object[]{label,layerID,style};
			meth_SortingLayerField.Invoke(null,parameters);
		}
	}
}