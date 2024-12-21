﻿using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Oktato_ora_konyvelo.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Oktato_ora_konyvelo.ViewModels;

namespace Oktato_ora_konyvelo.ViewModels
{
    public enum LessonType
    {
        A,
        Fv,
        Fo,
        Fe
    }
    public partial class MainViewModel : ViewModelBase
    {
        #region Data (ergo Model)

        #region Lessons

        [ObservableProperty] ObservableCollection<Lesson> allLessons = new();

        [ObservableProperty] ObservableCollection<Student> allStudents = new();

        [ObservableProperty] ToBeAddedLesson tempLesson;

        #endregion
        
        #region Places, Vehicles
        [ObservableProperty] ObservableCollection<string> vehicles = new(){"LWC256", "SPT867"};
        
        [ObservableProperty] ObservableCollection<Place> allPlaces = new();
        
        #endregion

        [ObservableProperty] List<string> recordLessonCbxOptions = new(){ "Alap", "Városi", "Országúti", "Éjszakai" };
        
        private KvarManager KvarManager = new();
        #endregion
        
        #region Commands
        [RelayCommand]
        public void LoginToKvar()
        {
            Task.Run(()=> KvarManager.Login());
        }
        
        #endregion

        #region Manage files
        public void ReadAllData()
        {

        }
        public void SaveAllData()
        {

        }
        #endregion
        
        public void AddTestData()
        {
            AllStudents.Add(new Student("Teszt Elek (12345678)", AllLessons));
            AllStudents.Add(new Student("Michael Myers (87654321)", AllLessons));

            AllLessons.CollectionChanged += AllLessons_CollectionChanged;

            AllLessons.Add(new Lesson(new DateOnly(2024, 08, 01),
                new TimeOnly(08, 00),
                new TimeOnly(9, 40),
                AllStudents[0],
                LessonType.A,
                "Autósiskola",
                "Autósiskola",
                18,
                false,
                AllLessons,
                true,
                522535));

            AllLessons.Add(new Lesson(new DateOnly(2024, 09, 03),
                new TimeOnly(10, 00),
                new TimeOnly(11, 40),
                AllStudents[1],
                LessonType.Fo,
                "Autósiskola",
                "Autósiskola",
                81,
                true,
                AllLessons,
                false));
            
            AllPlaces.Add(new Place("Autósiskola", true, true, "Miklós Jakab suli"));
            AllPlaces.Add(new Place("Első indulási helyszín", true, false, "123 Abc utca 321"));
            AllPlaces.Add(new Place("Első érkezési helyszín", false, true, "321 Zyx utca 123"));
        }

        #region Tanuló adatok frissítése
        private void UpdateStudentData()
        {
            foreach (Student item in AllStudents)
            {
                item.UpdateData();
            }
        }

        private void AllLessons_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateStudentData();
        }
        #endregion

        public MainViewModel()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("hu-HU");//Magyar Dátum és idő formátumokra váltás
            
            TempLesson = new ToBeAddedLesson(AllLessons);//Proxy óra az órarögzítéshez

            AddTestData();//Tesztadatok hozzáadása
        }
    }
}
