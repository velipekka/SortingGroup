using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SortingGroup : MonoBehaviour
{
	[System.Serializable]
	public struct RendererInfo
	{
		public string name;
		public Renderer renderer;
		public SortingGroup sortingGroup;
	}
	public List<RendererInfo> rendererInfos = new List<RendererInfo> ();
	public int sortingLayerID;
	public int groupOrder;

	void Reset()
	{
		Refresh();
	}

	bool Contains(Renderer renderer, SortingGroup sortingGroup)
	{
		foreach (var rendererInfo in rendererInfos)
		{
			if (rendererInfo.renderer == null && rendererInfo.sortingGroup == null)
			{
				rendererInfos.Remove(rendererInfo);
				continue;
			}

			if (renderer != null && rendererInfo.renderer == renderer)
				return true;

			if (sortingGroup != null && rendererInfo.sortingGroup == sortingGroup)
				return true;
		}
		return false;
	}

	public void Refresh ()
	{
		var renderers = GetRenderersInChildren();

		// Newly added renderers
		foreach (var renderer in renderers)
			if (!Contains(renderer, null))
				rendererInfos.Add(new RendererInfo() {name = renderer.name, renderer = renderer});

		var sortingGroups = GetSortingGroups ();

		foreach (var sortingGroup in sortingGroups)
			if (!Contains (null, sortingGroup))
				rendererInfos.Add (new RendererInfo () { name = sortingGroup.name, sortingGroup = sortingGroup });
	}

	List<Renderer> GetRenderersInChildren()
	{
		var renderers = GetComponentsInChildren<Renderer> ();
		List<Renderer> result = new List<Renderer>();

		foreach (var renderer in renderers)
			if (GetClosestComponentFromParent<SortingGroup> (renderer.transform, true) == this)
				result.Add(renderer);

		return result;
	}

	List<SortingGroup> GetSortingGroups()
	{
		List<SortingGroup> result = new List<SortingGroup>();

		foreach (var sortingGroup in GetComponentsInChildren<SortingGroup> ())
			if (GetClosestComponentFromParent<SortingGroup> (sortingGroup.transform, false) == this)
				if (sortingGroup != this)
					result.Add (sortingGroup);

		return result;
	}

	T GetClosestComponentFromParent<T>(Transform transform, bool includeSelf) where T : Component
	{
		if (includeSelf)
		{
			var component = transform.GetComponent<T>();
			if (component != null)
				return component;
		}

		if (transform.parent == null)
			return null;

		if (!includeSelf)
		{
			var component = transform.parent.GetComponent<T> ();
			if (component != null)
				return component;
		}

		return GetClosestComponentFromParent<T>(transform.parent, includeSelf);
	}

	void Update()
	{
		int index = rendererInfos.Count;
		foreach (var renderer in rendererInfos)
		{
			if (renderer.renderer)
				renderer.renderer.sortingOrder = index--;

			if (renderer.sortingGroup)
				renderer.sortingGroup.groupOrder = index--;
		}
	}
}
