using CommunityToolkit.Mvvm.ComponentModel;

namespace Oktato_ora_konyvelo.Classes;

public partial class Place : ObservableObject
{
    [ObservableProperty] private string placeName;
    [ObservableProperty] private bool isStartPlace;
    [ObservableProperty] private bool isEndPlace;
    [ObservableProperty] private string address;

    public Place(string placeName, bool isStartPlace, bool isEndPlace, string address)
    {
        PlaceName = placeName;
        IsStartPlace = isStartPlace;
        IsEndPlace = isEndPlace;
        Address = address;
    }
}