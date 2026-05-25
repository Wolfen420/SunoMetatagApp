using CommunityToolkit.Mvvm.ComponentModel;

namespace SunoMetatagApp.Models;

public sealed partial class Section : ObservableObject
{
    [ObservableProperty] private string _lyrics = "";
}
