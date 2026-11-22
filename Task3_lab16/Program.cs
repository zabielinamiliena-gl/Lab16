using System;
using System.Collections.Generic;

namespace BukovynaAirport
{
    class Journal
    {
        private int minutes;

        public Journal(int startHour, int startMinute)
        {
            minutes = startHour * 60 + startMinute;
        }

        public void Write(string source, string message)
        {
            int hour = minutes / 60;
            int minute = minutes % 60;
            Console.WriteLine("[" + hour.ToString("00") + ":" + minute.ToString("00") + "] [" + source + "] " + message);
            minutes += 1;
        }
    }

    class Passenger
    {
        public string Name { get; private set; }
        public string Seat { get; private set; }
        public string Mark { get; private set; }
        public bool Infant { get; private set; }

        public Passenger(string name, string seat, string mark, bool infant)
        {
            Name = name;
            Seat = seat;
            Mark = mark;
            Infant = infant;
        }

        public string Text()
        {
            if (Infant)
            {
                return Name + ", немовля без місця";
            }

            if (Mark.Length > 0)
            {
                return Name + ", місце " + Seat + ", позначка: " + Mark;
            }

            return Name + ", місце " + Seat;
        }
    }

    interface IPassengerIterator
    {
        string Name { get; }
        string Position { get; }
        bool HasNext();
        Passenger Next();
    }

    class Cabin
    {
        private readonly List<List<Passenger>> rows = new List<List<Passenger>>();
        private readonly List<Passenger> infants = new List<Passenger>();

        public void AddSeat(int row, string letter, string name, string mark)
        {
            while (rows.Count < row)
            {
                rows.Add(new List<Passenger>());
            }

            rows[row - 1].Add(new Passenger(name, row + letter, mark, false));
        }

        public void AddInfant(string name)
        {
            infants.Add(new Passenger(name, "", "", true));
        }

        public void PrintLayout(Journal journal)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                string line = "Ряд " + (i + 1) + ":";
                foreach (Passenger passenger in rows[i])
                {
                    line = line + " " + passenger.Text() + ";";
                }

                journal.Write("Кабіна", line);
            }

            string babies = "Немовлята без місця:";
            foreach (Passenger infant in infants)
            {
                babies = babies + " " + infant.Name + ";";
            }

            journal.Write("Кабіна", babies);
        }

        public IPassengerIterator BoardingOrder()
        {
            return new BoardingIterator(rows);
        }

        public IPassengerIterator CateringOrder()
        {
            return new CateringIterator(rows, infants);
        }

        public IPassengerIterator SecurityOrder()
        {
            return new SecurityIterator(rows, infants);
        }
    }

    class BoardingIterator : IPassengerIterator
    {
        private readonly List<Passenger> order = new List<Passenger>();
        private int index;

        public BoardingIterator(List<List<Passenger>> rows)
        {
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                foreach (Passenger passenger in rows[i])
                {
                    order.Add(passenger);
                }
            }
        }

        public string Name { get { return "Посадка"; } }

        public string Position { get { return index + " з " + order.Count; } }

        public bool HasNext()
        {
            return index < order.Count;
        }

        public Passenger Next()
        {
            Passenger passenger = order[index];
            index = index + 1;
            return passenger;
        }
    }

    class CateringIterator : IPassengerIterator
    {
        private readonly List<Passenger> order = new List<Passenger>();
        private int index;

        public CateringIterator(List<List<Passenger>> rows, List<Passenger> infants)
        {
            foreach (List<Passenger> row in rows)
            {
                foreach (Passenger passenger in row)
                {
                    order.Add(passenger);
                }
            }

            foreach (Passenger infant in infants)
            {
                order.Add(infant);
            }
        }

        public string Name { get { return "Кейтеринг"; } }

        public string Position { get { return index + " з " + order.Count; } }

        public bool HasNext()
        {
            return index < order.Count;
        }

        public Passenger Next()
        {
            Passenger passenger = order[index];
            index = index + 1;
            return passenger;
        }
    }

    class SecurityIterator : IPassengerIterator
    {
        private readonly List<Passenger> order = new List<Passenger>();
        private int index;

        public SecurityIterator(List<List<Passenger>> rows, List<Passenger> infants)
        {
            foreach (List<Passenger> row in rows)
            {
                foreach (Passenger passenger in row)
                {
                    if (passenger.Mark.Length > 0)
                    {
                        order.Add(passenger);
                    }
                }
            }

            foreach (Passenger infant in infants)
            {
                if (infant.Mark.Length > 0)
                {
                    order.Add(infant);
                }
            }
        }

        public string Name { get { return "Безпека"; } }

        public string Position { get { return index + " з " + order.Count; } }

        public bool HasNext()
        {
            return index < order.Count;
        }

        public Passenger Next()
        {
            Passenger passenger = order[index];
            index = index + 1;
            return passenger;
        }
    }

    class BoardingService
    {
        public void Serve(IPassengerIterator passengers, Journal journal)
        {
            journal.Write("Посадка", "Початок обходу, позиція " + passengers.Position);
            while (passengers.HasNext())
            {
                journal.Write("Посадка", passengers.Next().Text());
            }

            journal.Write("Посадка", "Кінець обходу, позиція " + passengers.Position);
        }
    }

    class CateringService
    {
        public void Serve(IPassengerIterator passengers, Journal journal)
        {
            journal.Write("Кейтеринг", "Початок обходу, позиція " + passengers.Position);
            while (passengers.HasNext())
            {
                journal.Write("Кейтеринг", passengers.Next().Text());
            }

            journal.Write("Кейтеринг", "Кінець обходу, позиція " + passengers.Position);
        }
    }

    class SecurityService
    {
        public void Serve(IPassengerIterator passengers, Journal journal)
        {
            journal.Write("Безпека", "Початок обходу, позиція " + passengers.Position);
            while (passengers.HasNext())
            {
                journal.Write("Безпека", passengers.Next().Text());
            }

            journal.Write("Безпека", "Кінець обходу, позиція " + passengers.Position);
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Situation3();
        }

        static void Situation3()
        {
            Journal journal = new Journal(11, 0);
            Cabin cabin = new Cabin();
            cabin.AddSeat(1, "A", "Олена Коваль", "");
            cabin.AddSeat(1, "B", "Іван Петренко", "візок");
            cabin.AddSeat(1, "C", "Марія Сидоренко", "");
            cabin.AddSeat(2, "A", "Петро Мельник", "");
            cabin.AddSeat(2, "B", "Анна Шевченко", "");
            cabin.AddSeat(2, "C", "Олег Бондар", "супровід");
            cabin.AddSeat(3, "A", "Катерина Лисенко", "");
            cabin.AddSeat(3, "B", "Андрій Ткачук", "");
            cabin.AddSeat(3, "C", "Софія Романюк", "");
            cabin.AddInfant("Данило Коваль");
            cabin.AddInfant("Марійка Мельник");

            journal.Write("Сценарій", "Схема салону до обходів");
            cabin.PrintLayout(journal);

            journal.Write("Сценарій", "Посадка від задніх рядів");
            new BoardingService().Serve(cabin.BoardingOrder(), journal);

            journal.Write("Сценарій", "Кейтеринг спереду, разом із немовлятами");
            new CateringService().Serve(cabin.CateringOrder(), journal);

            journal.Write("Сценарій", "Безпека: лише пасажири з позначкою");
            new SecurityService().Serve(cabin.SecurityOrder(), journal);

            journal.Write("Сценарій", "Два обходи одного салону одночасно");
            IPassengerIterator boarding = cabin.BoardingOrder();
            IPassengerIterator catering = cabin.CateringOrder();
            Step(journal, boarding, catering);
            Step(journal, catering, boarding);
            Step(journal, boarding, catering);
        }

        static void Step(Journal journal, IPassengerIterator mover, IPassengerIterator other)
        {
            journal.Write(mover.Name, "До кроку: " + mover.Position);
            journal.Write(other.Name, "До кроку: " + other.Position);
            Passenger passenger = mover.Next();
            journal.Write(mover.Name, "Видано: " + passenger.Text());
            journal.Write(mover.Name, "Після кроку: " + mover.Position);
            journal.Write(other.Name, "Після кроку іншого обходу: " + other.Position);
        }
    }
}
