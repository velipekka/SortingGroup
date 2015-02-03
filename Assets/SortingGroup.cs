using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
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

	public enum SortingMode { Manual, Hierarchy, Isometric }
	public SortingMode sortingMode;

	public List<RendererInfo> rendererInfos = new List<RendererInfo> ();
	public int sortingLayerID;

	public bool useIsometricSorting;
	public Vector3 isometricScale;

	void Reset()
	{
		Refresh();
		var rootGroup = GetClosestToRootComponent<SortingGroup> (transform) as SortingGroup;

		if (rootGroup != this)
			return;

		SortHierarchy (rootGroup);
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

	T GetClosestToRootComponent <T>(Transform transform) where T : Component
	{
		T result = null;
		if (transform.parent)
			result = GetClosestToRootComponent<T>(transform.parent);

		if (result == null)
			result = transform.GetComponent<T> ();

		return result;
	}

	void Update()
	{
		var rootGroup = GetClosestToRootComponent<SortingGroup> (transform) as SortingGroup;

		if (rootGroup != this)
			return;
		
		int orderIndex = GetRendererCount(rootGroup);
		SetRenderingOrder (rootGroup, ref orderIndex);
	}

	int GetRendererCount(SortingGroup sortingGroup)
	{
		int count = sortingGroup.rendererInfos.Count;

		foreach (var rendererInfo in sortingGroup.rendererInfos)
		{
			if (rendererInfo.sortingGroup != null)
				count += GetRendererCount(rendererInfo.sortingGroup);
		}

		return count;
	}

	void SetRenderingOrder(SortingGroup sortingGroup, ref int orderIndex)
	{
		switch (sortingGroup.sortingMode)
		{
			case SortingMode.Isometric:
				SortIsometric (sortingGroup);
				break;

			case SortingMode.Hierarchy:
				SortHierarchy (sortingGroup);
				break;
		}

		foreach (var rendererInfo in sortingGroup.rendererInfos)
		{
			if (rendererInfo.renderer)
				rendererInfo.renderer.sortingOrder = orderIndex--;

			if (rendererInfo.sortingGroup != null)
				SetRenderingOrder(rendererInfo.sortingGroup, ref orderIndex);
		}
	}

	void SortHierarchy(SortingGroup sortingGroup)
	{
		sortingGroup.rendererInfos.Sort(
			delegate(RendererInfo a, RendererInfo b)
			{
				var x = a.renderer ? a.renderer.transform : a.sortingGroup.transform;
				var y = b.renderer ? b.renderer.transform : b.sortingGroup.transform;

				return HierarchyCompare (x, y);
			}
			);
	}

	int HierarchyCompare(Transform a, Transform b)
	{
		return GetHierarhyIndex(a) < GetHierarhyIndex(b) ? 1 : -1;
	}

	int GetHierarhyIndex (Transform transform, int orderIndex = 0)
	{
		if (transform == this.transform)
			return orderIndex;

		orderIndex += transform.GetSiblingIndex ();
		return GetHierarhyIndex(transform.parent, orderIndex);
	}

	static void SortIsometric (SortingGroup sortingGroup)
	{
		sortingGroup.rendererInfos.Sort(
			delegate(RendererInfo a, RendererInfo b)
			{
				var x = a.renderer ? a.renderer.transform : a.sortingGroup.transform;
				var y = b.renderer ? b.renderer.transform : b.sortingGroup.transform;

				return x.position.y.CompareTo(y.position.y);
			}
			);
	}
}
