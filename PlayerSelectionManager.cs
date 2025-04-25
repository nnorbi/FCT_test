using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSelectionManager<T> where T : IPlayerSelectable
{
	private HashSet<T> _Selection = new HashSet<T>();

	public readonly SafeEvent<IReadOnlyCollection<T>> Changed = new SafeEvent<IReadOnlyCollection<T>>();

	public IReadOnlyCollection<T> Selection => _Selection;

	public int Count => _Selection.Count;

	public void Clear()
	{
		if (_Selection.Count != 0)
		{
			_Selection.Clear();
			Changed.Invoke(_Selection);
		}
	}

	public void Deselect(IEnumerable<T> entries)
	{
		int countBefore = _Selection.Count;
		_Selection.ExceptWith(entries);
		if (_Selection.Count != countBefore)
		{
			Changed.Invoke(_Selection);
		}
	}

	public void Select(IReadOnlyCollection<T> entities)
	{
		if (entities.Count == 0)
		{
			Debug.LogWarning("Called Select() with empty entity list");
			return;
		}
		if (entities.Any((T entity) => !entity.Selectable))
		{
			Debug.LogError("Tried to Select() non-selectable entity");
		}
		_Selection.UnionWith(entities);
		Changed.Invoke(_Selection);
	}

	public void ChangeSelection(HashSet<T> add, IEnumerable<T> remove)
	{
		if (add.Any((T entity) => !entity.Selectable))
		{
			Debug.LogError("Tried to call ChangeSelection() with non-selectable entity in add");
		}
		_Selection.UnionWith(add);
		_Selection.ExceptWith(remove);
		Changed.Invoke(_Selection);
	}

	public void ChangeTo(IEnumerable<T> newSelection)
	{
		HashSet<T> selectionSet = newSelection.ToHashSet();
		if (selectionSet.Any((T entity) => !entity.Selectable))
		{
			Debug.LogError("Tried to call ChangeSelection() with non-selectable entity in add");
		}
		_Selection = selectionSet;
		Changed.Invoke(_Selection);
	}
}
