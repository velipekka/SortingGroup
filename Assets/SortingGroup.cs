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
	}
	public List<RendererInfo> rendererInfos = new List<RendererInfo> ();
	public int sortingLayerID;
	public int groupOrder;

	void Reset()
	{
		Refresh();
	}

	bool Contains(Renderer renderer)
	{
		foreach (var rendererInfo in rendererInfos)
		{
			if (rendererInfo.renderer == null)
			{
				rendererInfos.Remove(rendererInfo);
				continue;
			}

			if (rendererInfo.renderer == renderer)
				return true;
		}
		return false;
	}

	public void Refresh ()
	{
		var renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());

		// Newly added renderers
		foreach (var renderer in renderers)
			if (!Contains(renderer))
				rendererInfos.Add (new RendererInfo () { name = renderer.name, renderer = renderer });
	}

	void Update()
	{
		int index = rendererInfos.Count;
		foreach (var renderer in rendererInfos)
		{
			renderer.renderer.sortingOrder = index--;
		}
	}
}
