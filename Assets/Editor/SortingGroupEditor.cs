using System;
using System.Linq;
using System.Reflection;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.Events;

namespace UnityEditor
{
	[CustomEditor (typeof (SortingGroup))]
	internal class SortingGroupEditor : Editor
	{
		ReorderableList list;
		SerializedProperty sortingLayerID;
		SerializedProperty sortingMode;
		AnimatedValues.AnimBool showManualMode;

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
			sortingMode = serializedObject.FindProperty("sortingMode");

			showManualMode = new AnimBool(sortingMode.intValue == 0);
			showManualMode.valueChanged = new UnityEvent ();
			showManualMode.valueChanged.AddListener(Repaint);

			// Get SortingLayerField method
			var editorTypes = typeof(Editor).Assembly.GetTypes();		
			var type = editorTypes.FirstOrDefault(t => t.Name == "EditorGUILayout");
			meth_SortingLayerField = type.GetMethod("SortingLayerField",(BindingFlags.Static | BindingFlags.NonPublic), null, new Type[] {typeof(GUIContent), typeof(SerializedProperty) , typeof(GUIStyle)}, null);
		}

		public override void OnInspectorGUI()
		{
			var sortingGroup = target as SortingGroup;

			serializedObject.Update();

			EditorGUILayout.PropertyField (sortingMode);

			var mode = (SortingGroup.SortingMode)sortingMode.intValue;
			showManualMode.target = mode == SortingGroup.SortingMode.Manual;

			if (EditorGUILayout.BeginFadeGroup (showManualMode.faded))
				list.DoLayoutList();
			EditorGUILayout.EndFadeGroup ();

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
			serializedObject.ApplyModifiedProperties();
		}

		static void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style)
		{
			var parameters = new object[]{label,layerID,style};
			meth_SortingLayerField.Invoke(null,parameters);
		}
	}
}