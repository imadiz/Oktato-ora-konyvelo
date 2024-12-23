﻿using CommunityToolkit.Mvvm.ComponentModel;
using Oktato_ora_konyvelo.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oktato_ora_konyvelo.Classes
{
    public partial class Student : ObservableObject
    {
        [ObservableProperty]
        private string nameAndId;

        [ObservableProperty]
        private string displayName;

        [ObservableProperty]
        private string id;

        [ObservableProperty]
        private TimeSpan drivenTime;

        [ObservableProperty]
        private int drivenDistance;

        [ObservableProperty]
        private int allLessonCount;

        [ObservableProperty]
        private int aLessonCount;

        [ObservableProperty]
        private int fvLessonCount;

        [ObservableProperty]
        private int foLessonCount;

        [ObservableProperty]
        private int feLessonCount;

        [ObservableProperty]
        private string? kVARStatus;

        [ObservableProperty]
        private ObservableCollection<Lesson> studentLessons = new();

        private readonly ObservableCollection<Lesson> AllLessons;

        public void UpdateData()
        {
            StudentLessons.Clear();

            foreach (Lesson studentlesson in AllLessons.Where(x=>x.Student.Id.Equals(Id)).OrderBy(x => x.Date).ThenBy(x => x.StartTime))
            {
                StudentLessons.Add(studentlesson);
            }

            DrivenTime = new TimeSpan(0, StudentLessons.Sum(x => x.DrivenMinutes), 0);
            DrivenDistance = StudentLessons.Sum(x => x.DrivenKm);
            AllLessonCount = StudentLessons.Count;

            ALessonCount = StudentLessons.Count(x => x.Type.Equals(LessonType.A));
            FvLessonCount = StudentLessons.Count(x => x.Type.Equals(LessonType.Fv));
            FoLessonCount = StudentLessons.Count(x => x.Type.Equals(LessonType.Fo));
            FeLessonCount = StudentLessons.Count(x => x.Type.Equals(LessonType.Fe));

            if (StudentLessons.All(x=>x.IsRecordedInKVAR.Equals(true)))//Ha az összes óra rögzítve van KVAR-ban
            {
                KVARStatus = "Rögzítve";
            }
            else if (StudentLessons.Any(x=>x.IsRecordedInKVAR.Equals(true)))//Ha legalább egy óra rögzítve van KVAR-ban
            {
                KVARStatus = "Részleges";
            }
            else
            {
                KVARStatus = "Nincs rögzítve";
            }
        }
        public Student(string name/*Név*/, string id/*Karton azon.*/, ObservableCollection<Lesson> alllessons)
        {
            NameAndId = $"{name} ({id})";

            DisplayName = name;
            Id = id;
            AllLessons = alllessons;
        }
    }
}