using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HospitalTreatmentPlan
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

    public sealed class TreatmentPlanState
    {
        public string Medication { get; }
        public string Dosage { get; }
        public List<string> Procedures { get; }

        public TreatmentPlanState(string medication, string dosage, IEnumerable<string> procedures)
        {
            Medication = medication;
            Dosage = dosage;
            Procedures = new List<string>(procedures);
        }

        public TreatmentPlanState Clone() => new TreatmentPlanState(Medication, Dosage, Procedures);

        public override string ToString()
        {
            string procList = Procedures.Count == 0 ? "немає" : string.Join(", ", Procedures);
            return $"препарат: '{Medication}', доза: '{Dosage}', процедури: [{procList}]";
        }
    }

    public sealed class TreatmentPlanMemento
    {
        public string StepName { get; }
        private readonly TreatmentPlanState _stateCopy;

        internal TreatmentPlanMemento(string stepName, TreatmentPlanState state)
        {
            StepName = stepName;
            _stateCopy = state.Clone();
        }

        internal TreatmentPlanState GetStateCopy() => _stateCopy.Clone();
    }

    public class TreatmentPlan
    {
        public string PatientName { get; }
        private TreatmentPlanState _state;

        public TreatmentPlan(string patientName, string medication, string dosage, IEnumerable<string> procedures = null)
        {
            PatientName = patientName;
            _state = new TreatmentPlanState(medication, dosage, procedures ?? Enumerable.Empty<string>());
            Logger.Log("TreatmentPlan", $"Створено план для пацієнта '{PatientName}'. Поточний стан: {_state}");
        }

        public void ChangeMedicationAndDosage(string newMedication, string newDosage)
        {
            string before = _state.ToString();
            _state = new TreatmentPlanState(newMedication, newDosage, _state.Procedures);
            Logger.Log("TreatmentPlan", $"Змінено препарат і дозу для '{PatientName}'. ДО: [{before}] -> ПІСЛЯ: [{_state}]");
        }

        public void AddProcedure(string procedure)
        {
            string before = _state.ToString();
            var updatedProcedures = new List<string>(_state.Procedures) { procedure };
            _state = new TreatmentPlanState(_state.Medication, _state.Dosage, updatedProcedures);
            Logger.Log("TreatmentPlan", $"Додано процедуру '{procedure}' для '{PatientName}'. ДО: [{before}] -> ПІСЛЯ: [{_state}]");
        }

        public TreatmentPlanMemento Save(string stepName)
        {
            Logger.Log("TreatmentPlan", $"Збережено стан '{stepName}' для '{PatientName}'. Стан: [{_state}]");
            return new TreatmentPlanMemento(stepName, _state);
        }

        public void Restore(TreatmentPlanMemento memento)
        {
            string before = _state.ToString();
            _state = memento.GetStateCopy();
            Logger.Log("TreatmentPlan", $"Відновлено стан '{memento.StepName}' для '{PatientName}'. ДО: [{before}] -> ПІСЛЯ: [{_state}]");
        }

        public string DescribePlan() => $"{PatientName} -> {_state}";
    }

    public class TreatmentPlanHistory
    {
        private readonly List<TreatmentPlanMemento> _history = new List<TreatmentPlanMemento>();

        public void Push(TreatmentPlanMemento memento)
        {
            _history.Add(memento);
            Logger.Log("TreatmentPlanHistory", $"Записано стан '{memento.StepName}' в історію (всього станів: {_history.Count}).");
        }

        public TreatmentPlanMemento StepBack()
        {
            if (_history.Count < 2)
                throw new InvalidOperationException("Недостатньо збережених станів для відкату назад.");

            int targetIndex = _history.Count - 2;
            var target = _history[targetIndex];
            _history.RemoveAt(_history.Count - 1);
            Logger.Log("TreatmentPlanHistory", $"Відкат на крок назад до стану '{target.StepName}' (залишилось станів: {_history.Count}).");
            return target;
        }

        public TreatmentPlanMemento GetFirstState()
        {
            if (_history.Count == 0)
                throw new InvalidOperationException("Історія станів порожня.");

            var first = _history[0];
            Logger.Log("TreatmentPlanHistory", $"Запитано найперший стан '{first.StepName}'.");
            return first;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            RunTask2Scenario();
        }

        public static void RunTask2Scenario()
        {
            Logger.Log("Runner", "=== Початок Завдання 2: Редагування плану лікування (Memento) ===");

            var history = new TreatmentPlanHistory();
            var plan = new TreatmentPlan("Олександр Савченко", "Цефтріаксон", "1000 мг/добу", new[] { "Загальний аналіз крові" });

            history.Push(plan.Save("Стан 1 (базовий)"));

            plan.ChangeMedicationAndDosage("Амоксиклав", "875 мг/добу");
            history.Push(plan.Save("Стан 2 (змінено препарат і дозу)"));

            plan.AddProcedure("Фізіотерапія");
            Logger.Log("Consilium", $"Поточний стан плану після додавання процедури: {plan.DescribePlan()}");

            plan.Restore(history.StepBack());
            Logger.Log("Consilium", $"План після відкату на крок назад: {plan.DescribePlan()}");

            plan.Restore(history.GetFirstState());
            Logger.Log("Consilium", $"План після відкату до найпершого стану: {plan.DescribePlan()}");

            Logger.Log("Runner", "=== Кінець Завдання 2 ===");
        }
    }
}