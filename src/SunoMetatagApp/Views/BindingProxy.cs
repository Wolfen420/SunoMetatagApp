using System.Windows;

namespace SunoMetatagApp.Views;

// v1.20 (B-028) — Lead absorption #2: bind commands across the ComboBox Popup
// boundary. ComboBox dropdown items live in a separate visual/logical tree
// (Popup hosts a PopupRoot), so RelativeSource={AncestorType=Window} from
// inside an ItemTemplate fails. A Freezable proxy held in Window.Resources is
// captured by both trees (Freezable inheritance context propagates through
// resource lookups), so a Binding can hop through it to reach the Window's
// DataContext (the MainViewModel).
//
// Usage:
//   <Window.Resources>
//     <views:BindingProxy x:Key="VmProxy" Data="{Binding}" />
//   </Window.Resources>
//   <!-- inside a ComboBox.ItemTemplate -->
//   <Button Command="{Binding Data.DeleteUserTemplateCommand, Source={StaticResource VmProxy}}"
//           CommandParameter="{Binding}" />
public sealed class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new UIPropertyMetadata(null));
}
