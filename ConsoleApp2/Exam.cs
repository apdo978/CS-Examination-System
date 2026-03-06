using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleApp2
{
    public enum ExamMode
    {
        Starting,
        Queued,
        Finished
    }

    public abstract class Exam : ICloneable, IComparable<Exam>
    {
        private int _time;
        public int Time
        {
            get => _time;
            set
            {
                if (value <= 0) throw new ArgumentException("Time must be greater than 0.");
                _time = value;
            }
        }

        private int _numberOfQuestions;
        public int NumberOfQuestions
        {
            get => _numberOfQuestions;
            set
            {
                if (value <= 0) throw new ArgumentException("NumberOfQuestions must be greater than 0.");
                _numberOfQuestions = value;
            }
        }

        public Question[] Questions { get; set; }
        public Dictionary<Question, Answer> QuestionAnswerDictionary { get; set; }
        protected Subject Subject;
        public ExamMode Mode;

        public event ExamStartedHandler? ExamStarted;

        protected void OnExamStarted()
        {
            ExamStarted?.Invoke(this, new ExamEventArgs(Subject, this));
        }

        protected Exam(int time, int numberOfQuestions, Subject subject, Question[] questions)
        {
            if (time <= 0) throw new ArgumentException("Time must be greater than 0.", nameof(time));
            if (numberOfQuestions <= 0) throw new ArgumentException("NumberOfQuestions must be greater than 0.", nameof(numberOfQuestions));
            _time = time;
            _numberOfQuestions = numberOfQuestions;
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Questions = questions ?? throw new ArgumentNullException(nameof(questions));
            QuestionAnswerDictionary = [];
            Mode = ExamMode.Queued;
        }

        public abstract void ShowExam();

        public virtual void Start()
        {
            Mode = ExamMode.Starting;
            OnExamStarted();

            Stopwatch stopwatch = new();
            stopwatch.Start();

            Console.WriteLine("Exam Has been Started\n");

            foreach (var q in Questions)
            {
                if (q is ChooseAllQuestion)
                    HandleMultiSelectQuestion(q);
                else
                    HandleSingleSelectQuestion(q);
            }

            stopwatch.Stop();
            Console.WriteLine($"\nYou took {stopwatch.ElapsedMilliseconds / 1000.0:F1} seconds to complete the exam.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        private void HandleSingleSelectQuestion(Question q)
        {
            int cursorIndex = 0;
            bool selecting = true;

            while (selecting)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Header: {q.Header}\n{q.Body} Marks:{q.Marks}\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Choices:\n");
                Console.ResetColor();

                for (int i = 0; i < q.Answers.Count; i++)
                {
                    if (i == cursorIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"-> {q.Answers[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {q.Answers[i]}");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (cursorIndex > 0) cursorIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (cursorIndex < q.Answers.Count - 1) cursorIndex++;
                        break;
                    case ConsoleKey.Enter:
                        var selected = q.Answers[cursorIndex];
                        QuestionAnswerDictionary[q] = new Answer(selected.Id.ToString());
                        Console.WriteLine($"\nYou chose: {selected}");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey(true);
                        selecting = false;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"\n'{keyInfo.KeyChar}' is not a valid option. Use arrow keys and Enter.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private void HandleMultiSelectQuestion(Question q)
        {
            int cursorIndex = 0;
            bool[] selected = new bool[q.Answers.Count];
            bool selecting = true;

            while (selecting)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Header: {q.Header}\n{q.Body} Marks:{q.Marks}\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Choices (Space to toggle, Enter to confirm):\n");
                Console.ResetColor();

                for (int i = 0; i < q.Answers.Count; i++)
                {
                    string marker = selected[i] ? "[X]" : "[ ]";
                    if (i == cursorIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"-> {marker} {q.Answers[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"   {marker} {q.Answers[i]}");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (cursorIndex > 0) cursorIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (cursorIndex < q.Answers.Count - 1) cursorIndex++;
                        break;
                    case ConsoleKey.Spacebar:
                        selected[cursorIndex] = !selected[cursorIndex];
                        break;
                    case ConsoleKey.Enter:
                        string idStr = "";
                        string displayStr = "";
                        for (int i = 0; i < q.Answers.Count; i++)
                        {
                            if (selected[i])
                            {
                                if (idStr.Length > 0) { idStr += ","; displayStr += ", "; }
                                idStr += q.Answers[i].Id.ToString();
                                displayStr += q.Answers[i].Text;
                            }
                        }
                        QuestionAnswerDictionary[q] = new Answer(idStr);
                        Console.WriteLine($"\nYou chose: {displayStr}");
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey(true);
                        selecting = false;
                        break;
                }
            }
        }

        public virtual void Finish()
        {
            Mode = ExamMode.Finished;
        }

        public int CorrectExam()
        {
            int correctCount = 0;
            foreach (var kvp in QuestionAnswerDictionary)
            {
                if (kvp.Key.CheckAnswer(kvp.Value))
                    correctCount++;
            }
            return correctCount;
        }

        public override string ToString()
        {
            string result = $"Exam - Time: {Time} min, Questions: {NumberOfQuestions}, Mode: {Mode}\n";
            foreach (var q in Questions)
                result += q + "\n";
            return result;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Exam other) return false;
            return Time == other.Time && NumberOfQuestions == other.NumberOfQuestions;
        }

        public override int GetHashCode() => HashCode.Combine(Time, NumberOfQuestions);

        public abstract object Clone();

        public int CompareTo(Exam? other)
        {
            if (other is null) return 1;
            int timeComparison = Time.CompareTo(other.Time);
            if (timeComparison != 0) return timeComparison;
            return NumberOfQuestions.CompareTo(other.NumberOfQuestions);
        }
    }

    public class PracticeExam : Exam
    {
        public PracticeExam(int time, int numberOfQuestions, Subject subject, Question[] questions)
            : base(time, numberOfQuestions, subject, questions)
        {
        }

        public override object Clone()
        {
            return new PracticeExam(Time, NumberOfQuestions, Subject, (Question[])Questions.Clone());
        }

        public override void ShowExam()
        {
            Console.WriteLine("=== Practice Exam ===\n");
            foreach (var q in Questions)
                q.Display();
        }

        public override void Finish()
        {
            base.Finish();
            Console.Clear();
            Console.WriteLine("Exam Has Ended\n");
            Console.WriteLine("********************************************\n");

            int totalMarks = 0;
            int earnedMarks = 0;

            foreach (var kvp in QuestionAnswerDictionary)
            {
                totalMarks += kvp.Key.Marks;
                bool isCorrect = kvp.Key.CheckAnswer(kvp.Value);

                if (isCorrect)
                {
                    earnedMarks += kvp.Key.Marks;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[CORRECT] {kvp.Key.Body} - Correct Answer: {kvp.Key.CorrectAnswer}\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    string studentAnswerDisplay = GetStudentAnswerDisplay(kvp.Key, kvp.Value);
                    Console.WriteLine($"[WRONG] {kvp.Key.Body} - Your Answer: {studentAnswerDisplay} - Correct Answer: {kvp.Key.CorrectAnswer}\n");
                }
                Console.ResetColor();
            }

            Console.WriteLine("********************************************\n");
            Console.WriteLine($"Your Grade: {earnedMarks} / {totalMarks}");
        }

        private static string GetStudentAnswerDisplay(Question question, Answer studentAnswer)
        {
            if (question is ChooseAllQuestion)
            {
                string display = "";
                string[] ids = studentAnswer.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string idStr in ids)
                {
                    if (int.TryParse(idStr.Trim(), out int id))
                    {
                        try
                        {
                            var ans = question.Answers.GetById(id);
                            if (display.Length > 0) display += ", ";
                            display += ans.Text;
                        }
                        catch (KeyNotFoundException) { }
                    }
                }
                return display;
            }

            if (int.TryParse(studentAnswer.Text, out int answerId))
            {
                try { return question.Answers.GetById(answerId).Text; }
                catch (KeyNotFoundException) { }
            }
            return studentAnswer.Text;
        }
    }

    public class FinalExam : Exam
    {
        public FinalExam(int time, int numberOfQuestions, Subject subject, Question[] questions)
            : base(time, numberOfQuestions, subject, questions)
        {
        }

        public override object Clone()
        {
            return new FinalExam(Time, NumberOfQuestions, Subject, (Question[])Questions.Clone());
        }

        public override void ShowExam()
        {
            Console.WriteLine("=== Final Exam ===\n");
            foreach (var q in Questions)
                q.Display();
        }

        public override void Finish()
        {
            base.Finish();
            Console.Clear();
            Console.WriteLine("Exam Has Ended\n");
            Console.WriteLine("********************************************\n");

            foreach (var kvp in QuestionAnswerDictionary)
            {
                string studentAnswerDisplay = GetStudentAnswerDisplay(kvp.Key, kvp.Value);
                Console.WriteLine($"Question: {kvp.Key.Body} - Your Answer: {studentAnswerDisplay}\n");
            }

            Console.WriteLine("********************************************\n");
        }

        private static string GetStudentAnswerDisplay(Question question, Answer studentAnswer)
        {
            if (question is ChooseAllQuestion)
            {
                string display = "";
                string[] ids = studentAnswer.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string idStr in ids)
                {
                    if (int.TryParse(idStr.Trim(), out int id))
                    {
                        try
                        {
                            var ans = question.Answers.GetById(id);
                            if (display.Length > 0) display += ", ";
                            display += ans.Text;
                        }
                        catch (KeyNotFoundException) { }
                    }
                }
                return display;
            }

            if (int.TryParse(studentAnswer.Text, out int answerId))
            {
                try { return question.Answers.GetById(answerId).Text; }
                catch (KeyNotFoundException) { }
            }
            return studentAnswer.Text;
        }
    }

    public class Subject
    {
        private string _name;
        private Student[] _enrolledStudents;
        private int _studentCount;

        public string Name { get => _name; set => _name = value ?? throw new ArgumentNullException(nameof(value)); }
        public Student[] EnrolledStudents
        {
            get
            {
                var result = new Student[_studentCount];
                Array.Copy(_enrolledStudents, result, _studentCount);
                return result;
            }
        }

        public PracticeExam? PracticalExam { get; private set; }
        public FinalExam? FinalExam { get; private set; }

        public Subject(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _enrolledStudents = new Student[4];
            _studentCount = 0;
        }

        public void Enroll(Student student)
        {
            if (student is null) throw new ArgumentNullException(nameof(student));
            if (_studentCount >= _enrolledStudents.Length)
            {
                var newArr = new Student[_enrolledStudents.Length * 2];
                Array.Copy(_enrolledStudents, newArr, _studentCount);
                _enrolledStudents = newArr;
            }
            _enrolledStudents[_studentCount++] = student;

            if (PracticalExam is not null)
                PracticalExam.ExamStarted += student.OnExamStarted;
            if (FinalExam is not null)
                FinalExam.ExamStarted += student.OnExamStarted;
        }

        public void CreatePracticalExam(int time, int numberOfQuestions, Subject subject, Question[] questions)
        {
            PracticalExam = new PracticeExam(time, numberOfQuestions, subject, questions);
            for (int i = 0; i < _studentCount; i++)
                PracticalExam.ExamStarted += _enrolledStudents[i].OnExamStarted;
            NotifyStudents();
        }

        public void CreateFinalExam(int time, int numberOfQuestions, Subject subject, Question[] questions)
        {
            FinalExam = new FinalExam(time, numberOfQuestions, subject, questions);
            for (int i = 0; i < _studentCount; i++)
                FinalExam.ExamStarted += _enrolledStudents[i].OnExamStarted;
            NotifyStudents();
        }

        public void NotifyStudents()
        {
            Console.WriteLine("An Exam Has Been Added");
        }
    }

    public class Student
    {
        private static int _idCounter;

        public string Name { get; set; }
        public string Email { get; set; }
        public int Id { get; }

        public Student(string name, string email = "")
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? "";
            Id = ++_idCounter;
        }

        public void OnExamStarted(object sender, ExamEventArgs e)
        {
            Console.WriteLine($"    [Notification] Student {Name}: Exam for '{e.Subject.Name}' has started!");
        }
    }
}
