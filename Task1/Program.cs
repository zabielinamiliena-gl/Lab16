using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HospitalWardTraversal
{
    public static class Logger
    {
        private static int _minutesOffset = 0;

        public static void Log(string source, string message)
        {
            string timestamp = TimeSpan.FromMinutes(480 + _minutesOffset).ToString(@"hh\:mm");
            _minutesOffset += 5;
            Console.WriteLine($"[{timestamp}] [{source}] {message}");
        }
    }

    public sealed class Patient
    {
        public string FullName { get; }
        public string AttendingDoctor { get; }
        public bool IsSevere { get; }
        public string BedLabel { get; }

        public Patient(string fullName, string attendingDoctor, bool isSevere, string bedLabel)
        {
            FullName = fullName;
            AttendingDoctor = attendingDoctor;
            IsSevere = isSevere;
            BedLabel = bedLabel;
        }

        public override string ToString()
        {
            string severity = IsSevere ? "важкий стан" : "стабільний стан";
            return $"{FullName} [ліжко {BedLabel}, лікар {AttendingDoctor}, {severity}]";
        }
    }

    public sealed class Bed
    {
        public string BedNumber { get; }
        public Patient Patient { get; }

        public Bed(string bedNumber, Patient patient)
        {
            BedNumber = bedNumber;
            Patient = patient;
        }
    }

    public sealed class Ward
    {
        public int Number { get; }
        public List<Bed> Beds { get; } = new List<Bed>();

        public Ward(int number)
        {
            Number = number;
        }

        public void AddBed(string bedNumber, string patientName, string attendingDoctor, bool isSevere)
        {
            var patient = new Patient(patientName, attendingDoctor, isSevere, $"{Number}-{bedNumber}");
            Beds.Add(new Bed(bedNumber, patient));
        }
    }

    public interface IPatientIterator
    {
        string IteratorName { get; }
        bool HasNext();
        Patient Current { get; }
        Patient MoveNext();
    }

    public interface IHospitalDepartment
    {
        IPatientIterator CreateDoctorIterator(string doctorName);
        IPatientIterator CreateNurseIterator();
        IPatientIterator CreateDutyDoctorIterator();
    }

    public class HospitalDepartment : IHospitalDepartment
    {
        public string DepartmentName { get; }
        private readonly SortedDictionary<int, Ward> _wards = new SortedDictionary<int, Ward>();

        public HospitalDepartment(string departmentName)
        {
            DepartmentName = departmentName;
        }

        public Ward GetOrCreateWard(int wardNumber)
        {
            if (!_wards.ContainsKey(wardNumber))
            {
                _wards[wardNumber] = new Ward(wardNumber);
            }
            return _wards[wardNumber];
        }

        public IPatientIterator CreateDoctorIterator(string doctorName) =>
            new DoctorByWardsIterator(_wards, doctorName);

        public IPatientIterator CreateNurseIterator() =>
            new NurseAllWardsIterator(_wards);

        public IPatientIterator CreateDutyDoctorIterator() =>
            new DutyDoctorSevereFirstIterator(_wards);

        private sealed class DoctorByWardsIterator : IPatientIterator
        {
            public string IteratorName => $"Обхід лікаря ({_doctorName}) по палатах";
            private readonly string _doctorName;
            private readonly List<Patient> _patients;
            private int _index = -1;

            public DoctorByWardsIterator(SortedDictionary<int, Ward> wards, string doctorName)
            {
                _doctorName = doctorName;
                _patients = wards
                    .OrderBy(w => w.Key)
                    .SelectMany(w => w.Value.Beds)
                    .Where(b => b.Patient != null && b.Patient.AttendingDoctor == doctorName)
                    .Select(b => b.Patient)
                    .ToList();
            }

            public bool HasNext() => _index + 1 < _patients.Count;
            public Patient Current => _index >= 0 && _index < _patients.Count ? _patients[_index] : null;

            public Patient MoveNext()
            {
                if (!HasNext()) return null;
                _index++;
                return _patients[_index];
            }
        }

        private sealed class NurseAllWardsIterator : IPatientIterator
        {
            public string IteratorName => "Обхід медсестри (усі палати за номерами)";
            private readonly List<Patient> _patients;
            private int _index = -1;

            public NurseAllWardsIterator(SortedDictionary<int, Ward> wards)
            {
                _patients = wards
                    .OrderBy(w => w.Key)
                    .SelectMany(w => w.Value.Beds)
                    .Where(b => b.Patient != null)
                    .Select(b => b.Patient)
                    .ToList();
            }

            public bool HasNext() => _index + 1 < _patients.Count;
            public Patient Current => _index >= 0 && _index < _patients.Count ? _patients[_index] : null;

            public Patient MoveNext()
            {
                if (!HasNext()) return null;
                _index++;
                return _patients[_index];
            }
        }

        private sealed class DutyDoctorSevereFirstIterator : IPatientIterator
        {
            public string IteratorName => "Обхід чергового лікаря (спершу важкі)";
            private readonly List<Patient> _patients;
            private int _index = -1;

            public DutyDoctorSevereFirstIterator(SortedDictionary<int, Ward> wards)
            {
                _patients = wards
                    .OrderBy(w => w.Key)
                    .SelectMany(w => w.Value.Beds)
                    .Where(b => b.Patient != null)
                    .Select(b => b.Patient)
                    .OrderByDescending(p => p.IsSevere)
                    .ToList();
            }

            public bool HasNext() => _index + 1 < _patients.Count;
            public Patient Current => _index >= 0 && _index < _patients.Count ? _patients[_index] : null;

            public Patient MoveNext()
            {
                if (!HasNext()) return null;
                _index++;
                return _patients[_index];
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            RunTask1Scenario();
        }

        public static void RunTask1Scenario()
        {
            var department = new HospitalDepartment("Терапевтичне відділення №1");

            var ward101 = department.GetOrCreateWard(101);
            ward101.AddBed("1", "Іван Петренко", "Шевчук", false);
            ward101.AddBed("2", "Марія Коваленко", "Бондар", true);

            var ward102 = department.GetOrCreateWard(102);
            ward102.AddBed("1", "Олег Сидоренко", "Шевчук", true);
            ward102.AddBed("2", "Наталія Ткаченко", "Кравченко", false);

            var ward105 = department.GetOrCreateWard(105);
            ward105.AddBed("1", "Андрій Мороз", "Шевчук", false);
            ward105.AddBed("2", "Світлана Лисенко", "Бондар", true);

            var shevchukIter = department.CreateDoctorIterator("Шевчук");
            Logger.Log("DoctorRound", $"Старт [{shevchukIter.IteratorName}]. ДО: {shevchukIter.Current?.ToString() ?? "<не розпочато>"}");
            var shevchukList = new List<string>();
            while (shevchukIter.HasNext())
            {
                shevchukList.Add(shevchukIter.MoveNext().ToString());
            }
            Logger.Log("DoctorRound", $"Завершено [{shevchukIter.IteratorName}]. ПІСЛЯ: {shevchukIter.Current?.ToString() ?? "<кінець>"}. Пацієнти: {string.Join(" -> ", shevchukList)}");

            var dutyIter = department.CreateDutyDoctorIterator();
            Logger.Log("DutyDoctorRound", $"Старт [{dutyIter.IteratorName}]. ДО: {dutyIter.Current?.ToString() ?? "<не розпочато>"}");
            var dutyList = new List<string>();
            while (dutyIter.HasNext())
            {
                dutyList.Add(dutyIter.MoveNext().ToString());
            }
            Logger.Log("DutyDoctorRound", $"Завершено [{dutyIter.IteratorName}]. ПІСЛЯ: {dutyIter.Current?.ToString() ?? "<кінець>"}. Пацієнти: {string.Join(" -> ", dutyList)}");

            var concurrentDoctor = department.CreateDoctorIterator("Шевчук");
            var concurrentNurse = department.CreateNurseIterator();

            Logger.Log("ConcurrentRound", $"Одночасний обхід ДО -> Лікар: {concurrentDoctor.Current?.ToString() ?? "<початок>"} | Медсестра: {concurrentNurse.Current?.ToString() ?? "<початок>"}");

            concurrentDoctor.MoveNext();
            Logger.Log("ConcurrentRound", $"Лікар зробив 1 крок -> позиція лікаря: {concurrentDoctor.Current} | позиція медсестри все ще: {concurrentNurse.Current?.ToString() ?? "<початок>"}");

            concurrentNurse.MoveNext();
            concurrentNurse.MoveNext();
            Logger.Log("ConcurrentRound", $"Медсестра зробила 2 кроки -> позиція медсестри: {concurrentNurse.Current} | позиція лікаря не змінилася: {concurrentDoctor.Current}");

            concurrentDoctor.MoveNext();
            concurrentNurse.MoveNext();
            Logger.Log("ConcurrentRound", $"Одночасний обхід ПІСЛЯ кроків -> позиція лікаря: {concurrentDoctor.Current} | позиція медсестри: {concurrentNurse.Current}");
        }
    }
}