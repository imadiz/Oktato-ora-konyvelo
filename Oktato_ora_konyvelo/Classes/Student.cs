using CommunityToolkit.Mvvm.ComponentModel;
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

            foreach (Lesson studentlesson in AllLessons.Where(x=>x.Student.Id.Equals(Id)).OrderBy(x => x.Date).OrderBy(x => x.StartTime))
            {
                StudentLessons.Add(studentlesson);
            }

            DrivenTime = new TimeSpan(0, StudentLessons.Sum(x => x.DrivenMinutes), 0);
            DrivenDistance = StudentLessons.Sum(x => x.DrivenKm);
            AllLessonCount = StudentLessons.Count;

            ALessonCount = StudentLessons.Count(x => x.LessonType.Equals("A"));
            FvLessonCount = StudentLessons.Count(x => x.LessonType.Equals("F/v"));
            FoLessonCount = StudentLessons.Count(x => x.LessonType.Equals("F/o"));
            FeLessonCount = StudentLessons.Count(x => x.LessonType.Equals("F/é"));

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
        public Student(string nameAndId, ObservableCollection<Lesson> alllessons)
        {
            NameAndId = nameAndId;

            DisplayName = nameAndId[..nameAndId.IndexOf(" (")].Trim();
            Id = new string(nameAndId[nameAndId.IndexOf(" (")..].Where(char.IsNumber).ToArray());
            AllLessons = alllessons;

            UpdateData();
        }
    }
}