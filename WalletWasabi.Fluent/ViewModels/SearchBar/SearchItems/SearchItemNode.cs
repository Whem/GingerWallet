using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveUI;
using WalletWasabi.Fluent.ViewModels.SearchBar.Patterns;
using WalletWasabi.Fluent.ViewModels.SearchBar.Settings;
using WalletWasabi.Fluent.ViewModels.SearchBar.Sources;

namespace WalletWasabi.Fluent.ViewModels.SearchBar.SearchItems;

public class SearchItemNode<TObject, TProperty> : ReactiveObject, IContentSearchItem
{
	private readonly IEditableSearchSource _editableSearchSource;
	private readonly NestedItemConfiguration<TProperty>[] _nestedItems;
	private readonly Setting<TObject, TProperty> _setting;

	public SearchItemNode(IEditableSearchSource editableSearchSource, Setting<TObject, TProperty> setting, string name, string category, IEnumerable<string> keywords, string? icon, bool isDefault, params NestedItemConfiguration<TProperty>[] nestedItems)
	{
		_editableSearchSource = editableSearchSource;
		_setting = setting;
		_nestedItems = nestedItems;
		Name = name;
		Content = setting;
		Category = category;
		Keywords = keywords;
		Icon = icon;
		IsDefault = isDefault;
		this.WhenAnyValue(item => item._setting.Value)
			.Do(HandleNested)
			.Subscribe();
	}

	public object Content { get; }
	public string Name { get; }
	public ComposedKey Key => new(Name);
	public string Description => "";
	public string? Icon { get; set; }
	public string Category { get; }
	public IEnumerable<string> Keywords { get; }
	public bool IsDefault { get; }
	public int Priority { get; set; }

	private void HandleNested(TProperty property)
	{
		foreach (var nestedItem in _nestedItems)
		{
			var isVisible = nestedItem.IsVisibleSelector(property);
			if (isVisible)
			{
				_editableSearchSource.Add(nestedItem.Item);
			}
			else
			{
				_editableSearchSource.Remove(nestedItem.Item);
			}
		}
	}
}
