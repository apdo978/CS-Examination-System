namespace ConsoleApp2
{
    public class GradeRecord
    {
        public string Email { get; }
        public string StudentName { get; }
        public string SubjectName { get; }
        public string ExamType { get; }
        public int Score { get; }
        public int Total { get; }
        public string Date { get; }

        public GradeRecord(string email, string studentName, string subjectName,
            string examType, int score, int total, string date)
        {
            Email = email;
            StudentName = studentName;
            SubjectName = subjectName;
            ExamType = examType;
            Score = score;
            Total = total;
            Date = date;
        }

        public override string ToString() =>
            $"{Email}|{StudentName}|{SubjectName}|{ExamType}|{Score}|{Total}|{Date}";

        public static GradeRecord? FromLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            string[] parts = line.Split('|');
            if (parts.Length < 7) return null;
            if (!int.TryParse(parts[4], out int score)) return null;
            if (!int.TryParse(parts[5], out int total)) return null;
            return new GradeRecord(parts[0], parts[1], parts[2], parts[3], score, total, parts[6]);
        }
    }

    public static class ResultLogger
    {
        private const string ResultsDir = "results";
        private const string GradesFile = "grades.dat";

        public static void LogExamResult(Student student, Subject subject, Exam exam)
        {
            Directory.CreateDirectory(ResultsDir);

            string examType = exam is PracticeExam ? "Practice" : "Final";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string safeEmail = SanitizeFileName(student.Email);
            string safeSubject = SanitizeFileName(subject.Name);
            string fileName = $"{safeEmail}_{safeSubject}_{examType}_{timestamp}.log";
            string filePath = Path.Combine(ResultsDir, fileName);

            int totalMarks = 0;
            int earnedMarks = 0;

            try
            {
                using var writer = new StreamWriter(filePath, false);
                writer.WriteLine("Exam Result Log");
                writer.WriteLine("==============================================");
                writer.WriteLine($"Student : {student.Name} (ID: {student.Id})");
                writer.WriteLine($"Email   : {student.Email}");
                writer.WriteLine($"Subject : {subject.Name}");
                writer.WriteLine($"Type    : {examType}");
                writer.WriteLine($"Date    : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine("==============================================");
                writer.WriteLine();

                foreach (var kvp in exam.QuestionAnswerDictionary)
                {
                    var question = kvp.Key;
                    var studentAnswer = kvp.Value;
                    bool isCorrect = question.CheckAnswer(studentAnswer);

                    totalMarks += question.Marks;
                    if (isCorrect) earnedMarks += question.Marks;

                    string answerDisplay = ResolveAnswerDisplay(question, studentAnswer);

                    writer.WriteLine($"Q: {question.Header} - {question.Body}");
                    writer.WriteLine($"   Marks         : {question.Marks}");
                    writer.WriteLine($"   Your Answer   : {answerDisplay}");
                    writer.WriteLine($"   Correct Answer: {question.CorrectAnswer.Text}");
                    writer.WriteLine($"   Result        : {(isCorrect ? "CORRECT" : "WRONG")}");
                    writer.WriteLine();
                }

                double pct = totalMarks > 0 ? (double)earnedMarks / totalMarks * 100 : 0;
                writer.WriteLine("==============================================");
                writer.WriteLine($"Final Score : {earnedMarks} / {totalMarks}");
                writer.WriteLine($"Percentage  : {pct:F1}%");
            }
            catch (IOException e)
            {
                Console.WriteLine($"    Error logging result: {e.Message}");
            }

            AppendGradeRecord(student.Email, student.Name, subject.Name, examType, earnedMarks, totalMarks);
        }

        private static void AppendGradeRecord(string email, string studentName,
            string subjectName, string examType, int score, int total)
        {
            try
            {
                var record = new GradeRecord(email, studentName, subjectName, examType,
                    score, total, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                using var writer = new StreamWriter(GradesFile, true);
                writer.WriteLine(record);
            }
            catch (IOException) { }
        }

        public static GradeRecord[] ReadAllGrades()
        {
            if (!File.Exists(GradesFile)) return [];

            GradeRecord[] buffer = new GradeRecord[16];
            int count = 0;

            try
            {
                using var reader = new StreamReader(GradesFile);
                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    var record = GradeRecord.FromLine(line);
                    if (record is null) continue;

                    if (count >= buffer.Length)
                    {
                        var newBuf = new GradeRecord[buffer.Length * 2];
                        Array.Copy(buffer, newBuf, count);
                        buffer = newBuf;
                    }
                    buffer[count++] = record;
                }
            }
            catch (IOException) { }

            var result = new GradeRecord[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        public static GradeRecord[] GetGradesForStudent(string email)
        {
            GradeRecord[] all = ReadAllGrades();

            GradeRecord[] buffer = new GradeRecord[all.Length];
            int count = 0;

            foreach (var g in all)
            {
                if (g.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                    buffer[count++] = g;
            }

            var result = new GradeRecord[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        public static string[] GetAllResultFiles()
        {
            if (!Directory.Exists(ResultsDir)) return [];
            return Directory.GetFiles(ResultsDir, "*.log");
        }

        public static string[] GetResultFilesForStudent(string email)
        {
            if (!Directory.Exists(ResultsDir)) return [];
            string safeEmail = SanitizeFileName(email);
            return Directory.GetFiles(ResultsDir, $"{safeEmail}_*.log");
        }

        public static string ReadResultFile(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                return reader.ReadToEnd();
            }
            catch (IOException e)
            {
                return $"Error reading file: {e.Message}";
            }
        }

        private static string ResolveAnswerDisplay(Question question, Answer studentAnswer)
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

        private static string SanitizeFileName(string name)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            string result = name;
            foreach (char c in invalid)
                result = result.Replace(c, '_');
            return result.Replace('@', '_').Replace('.', '_');
        }
    }
}
