using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApp2
{
    public class Answer : IEquatable<Answer>, IComparable<Answer>
    {
        private static int _idCounter;

        public int Id { get; }
        public string Text { get; set; }

        public Answer(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Id = _idCounter++;
        }

        public override string ToString() => $"id:{Id}, {Text}";

        public override bool Equals(object? obj)
        {
            return obj is Answer other && Equals(other);
        }

        public bool Equals(Answer? other)
        {
            if (other is null) return false;
            return Id == other.Id && Text == other.Text;
        }

        public override int GetHashCode() => HashCode.Combine(Id, Text);

        public int CompareTo(Answer? other)
        {
            if (other is null) return 1;
            return Id.CompareTo(other.Id);
        }
    }

    public class AnswerList
    {
        private Answer[] _answers;
        private int _count;

        public int Count => _count;

        public AnswerList(int capacity)
        {
            _answers = new Answer[capacity];
            _count = 0;
        }

        public AnswerList(Answer[] answers)
        {
            _answers = (Answer[])answers.Clone();
            _count = answers.Length;
        }

        public void Add(Answer answer)
        {
            if (answer is null) throw new ArgumentNullException(nameof(answer));
            if (_count >= _answers.Length)
            {
                var newArr = new Answer[_answers.Length * 2];
                Array.Copy(_answers, newArr, _count);
                _answers = newArr;
            }
            _answers[_count++] = answer;
        }

        public Answer GetById(int id)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_answers[i].Id == id)
                    return _answers[i];
            }
            throw new KeyNotFoundException($"Answer with Id {id} not found.");
        }

        public Answer this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException($"Index {index} is out of range.");
                return _answers[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException($"Index {index} is out of range.");
                _answers[index] = value;
            }
        }

        public override string ToString()
        {
            string result = "\n";
            for (int i = 0; i < _count; i++)
                result += _answers[i] + "\n";
            return result;
        }
    }

    public abstract class Question
    {
        private string _header;
        private string _body;
        private int _marks;

        public string Header
        {
            get => _header;
            set => _header = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Body
        {
            get => _body;
            set => _body = value ?? throw new ArgumentNullException(nameof(value));
        }

        public int Marks
        {
            get => _marks;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Marks must be greater than 0.");
                _marks = value;
            }
        }

        public AnswerList Answers { get; }
        public Answer CorrectAnswer { get; }

        protected Question(string header, string body, int marks, AnswerList answers, Answer correctAnswer)
        {
            _header = header ?? throw new ArgumentNullException(nameof(header));
            _body = body ?? throw new ArgumentNullException(nameof(body));
            if (marks <= 0) throw new ArgumentException("Marks must be greater than 0.", nameof(marks));
            _marks = marks;
            Answers = answers ?? throw new ArgumentNullException(nameof(answers));
            CorrectAnswer = correctAnswer ?? throw new ArgumentNullException(nameof(correctAnswer));
        }

        public abstract void Display();
        public abstract bool CheckAnswer(Answer studentAnswer);

        public override string ToString()
        {
            return $"Header: {Header}\n{Body} Marks:{Marks}\nChoices: {Answers}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Question other) return false;
            return Header == other.Header && Body == other.Body;
        }

        public override int GetHashCode() => HashCode.Combine(Header, Body);
    }

    public class TrueFalseQuestion : Question
    {
        public TrueFalseQuestion(string header, string body, int marks, Answer correctAnswer)
            : base(header, body, marks, new AnswerList([new Answer("True"), new Answer("False")]), correctAnswer)
        {
        }

        public override bool CheckAnswer(Answer studentAnswer)
        {
            if (studentAnswer is null) return false;
            if (int.TryParse(studentAnswer.Text, out int selectedId))
            {
                try
                {
                    return Answers.GetById(selectedId).Text.Equals(
                        CorrectAnswer.Text, StringComparison.OrdinalIgnoreCase);
                }
                catch (KeyNotFoundException) { return false; }
            }
            return false;
        }

        public override void Display()
        {
            Console.WriteLine($"Header: {Header}\n{Body}  ({Marks})\nChoices: {Answers}");
        }
    }

    public class ChooseOneQuestion : Question
    {
        public ChooseOneQuestion(string header, string body, int marks, AnswerList answers, Answer correctAnswer)
            : base(header, body, marks, answers, correctAnswer)
        {
        }

        public override bool CheckAnswer(Answer studentAnswer)
        {
            if (studentAnswer is null) return false;
            return int.TryParse(studentAnswer.Text, out int selectedId) && selectedId == CorrectAnswer.Id;
        }

        public override void Display()
        {
            Console.WriteLine($"Header: {Header}\n{Body}  ({Marks})\nChoices: {Answers}");
        }
    }

    public class ChooseAllQuestion : Question
    {
        private readonly Answer[] _correctAnswers;
        public Answer[] CorrectAnswers => (Answer[])_correctAnswers.Clone();

        public ChooseAllQuestion(string header, string body, int marks, AnswerList answers, Answer[] correctAnswers)
            : base(header, body, marks, answers,
                  correctAnswers?.Length > 0 ? correctAnswers[0] : throw new ArgumentException("Must have at least one correct answer.", nameof(correctAnswers)))
        {
            _correctAnswers = (Answer[])correctAnswers.Clone();
        }

        public override bool CheckAnswer(Answer studentAnswer)
        {
            if (studentAnswer is null) return false;

            string[] parts = studentAnswer.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != _correctAnswers.Length) return false;

            int[] selectedIds = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i].Trim(), out selectedIds[i]))
                    return false;
            }

            int[] correctIds = new int[_correctAnswers.Length];
            for (int i = 0; i < _correctAnswers.Length; i++)
                correctIds[i] = _correctAnswers[i].Id;

            Array.Sort(selectedIds);
            Array.Sort(correctIds);

            for (int i = 0; i < selectedIds.Length; i++)
            {
                if (selectedIds[i] != correctIds[i]) return false;
            }
            return true;
        }

        public override void Display()
        {
            Console.WriteLine($"Header: {Header}\n{Body}  ({Marks})\nChoices (select all that apply): {Answers}");
        }
    }

    public class QuestionList : List<Question>
    {
        private readonly string _fileName;

        public string FileName => _fileName;

        public QuestionList(string fileName) : base()
        {
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public QuestionList(Question[] questions, string fileName) : base(questions)
        {
            _fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            LogAllQuestions();
        }

        public new void Add(Question question)
        {
            if (question is null) throw new ArgumentNullException(nameof(question));
            base.Add(question);
            LogQuestion(question);
        }

        private void LogAllQuestions()
        {
            try
            {
                using var writer = new StreamWriter(_fileName, false);
                foreach (var q in this)
                    writer.WriteLine(q);
            }
            catch (IOException e)
            {
                Console.WriteLine($"An error occurred while logging: {e.Message}");
            }
        }

        private void LogQuestion(Question question)
        {
            try
            {
                using var writer = new StreamWriter(_fileName, true);
                writer.WriteLine(question);
            }
            catch (IOException e)
            {
                Console.WriteLine($"An error occurred while logging: {e.Message}");
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (var q in this)
                result += $"{q}\n\n";
            return result;
        }
    }
}
