using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Oktato_ora_konyvelo.Classes;

public partial class Vehicle : ObservableObject
{
    public enum VehicleCategory
    {
        B
    }

    [ObservableProperty] private string licensePlate;
    [ObservableProperty] private VehicleCategory category;
    [ObservableProperty] private bool canTow;

    public Vehicle(string licensePlate, VehicleCategory category, bool canTow)
    {
        LicensePlate = licensePlate;
        Category = category;
        CanTow = canTow;
    }
}