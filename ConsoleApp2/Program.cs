namespace ConsoleApp2
{
    internal class Program
    {
        static Subject[] _subjects = null!;

        static void Main(string[] args)
        {
            Console.Title = "Examination Management System";
            Console.CursorVisible = false;

            ShowSplash();

            _subjects = BuildSubjectsAndExams();

            Student[] npcStudents = [new("Ahmed"), new("Sara"), new("Mohamed")];
            foreach (var subj in _subjects)
                foreach (var npc in npcStudents)
                    subj.Enroll(npc);

            bool running = true;
            while (running)
            {
                int choice = ArrowMenu("WELCOME", [
                    "Login",
                    "Register",
                    "Exit"
                ]);

                switch (choice)
                {
                    case 0:
                        var account = LoginFlow();
                        if (account is not null)
                        {
                            if (account.Role == UserRole.Student)
                                StudentDashboard(account);
                            else
                                TeacherDashboard(account);
                        }
                        break;
                    case 1:
                        RegisterFlow();
                        break;
                    case 2 or -1:
                        running = false;
                        break;
                }
            }

            ShowExit();
        }


        static void ShowSplash()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            string[] banner =
            [
                "+==================================================+",
                "|                                                  |",
                "|       EXAMINATION  MANAGEMENT  SYSTEM            |",
                "|                                                  |",
                "|       Console-Based Exam Platform  v2.0          |",
                "|       .NET 10 / C# 14                            |",
                "|                                                  |",
                "+==================================================+"
            ];

            foreach (string line in banner)
            {
                Console.WriteLine($"  {line}");
                Thread.Sleep(50);
            }
            Console.ResetColor();
            Pause();
        }

        static void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            string bar = new('-', title.Length + 4);
            Console.WriteLine($"\n  +--{bar}+");
            Console.WriteLine($"  |  {title}    |");
            Console.WriteLine($"  +--{bar}+");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void Pause(string msg = "  Press any key to continue...")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n{msg}");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        static void ShowError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n    {message}");
            Console.ResetColor();
            Pause();
        }

        static int ArrowMenu(string title, string[] options)
        {
            int cursor = 0;
            while (true)
            {
                Console.Clear();
                PrintHeader(title);

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == cursor)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"    > {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"      {options[i]}");
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n    [Up/Down] Navigate    [Enter] Select    [Esc] Back");
                Console.ResetColor();

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        cursor = (cursor - 1 + options.Length) % options.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        cursor = (cursor + 1) % options.Length;
                        break;
                    case ConsoleKey.Enter:
                        return cursor;
                    case ConsoleKey.Escape:
                        return -1;
                }
            }
        }

        static string ReadLine()
        {
            Console.CursorVisible = true;
            string input = Console.ReadLine()?.Trim() ?? "";
            Console.CursorVisible = false;
            return input;
        }

        static string ReadPassword()
        {
            string password = "";
            Console.CursorVisible = true;
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }
            Console.CursorVisible = false;
            Console.WriteLine();
            return password;
        }

        static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            int at = email.IndexOf('@');
            int dot = email.LastIndexOf('.');
            return at > 0 && dot > at + 1 && dot < email.Length - 1;
        }


        static void RegisterFlow()
        {
            Console.Clear();
            PrintHeader("REGISTER NEW ACCOUNT");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Full Name : ");
            Console.ForegroundColor = ConsoleColor.White;
            string name = ReadLine();
            if (string.IsNullOrWhiteSpace(name)) { ShowError("Name cannot be empty."); return; }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Email     : ");
            Console.ForegroundColor = ConsoleColor.White;
            string email = ReadLine();
            if (!IsValidEmail(email)) { ShowError("Invalid email format."); return; }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Password  : ");
            Console.ForegroundColor = ConsoleColor.White;
            string password = ReadPassword();
            if (password.Length < 4) { ShowError("Password must be at least 4 characters."); return; }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Confirm   : ");
            Console.ForegroundColor = ConsoleColor.White;
            string confirm = ReadPassword();
            if (password != confirm) { ShowError("Passwords do not match."); return; }

            int roleChoice = ArrowMenu("SELECT YOUR ROLE", ["Student", "Teacher", "Cancel"]);
            if (roleChoice is 2 or -1) return;
            UserRole role = roleChoice == 0 ? UserRole.Student : UserRole.Teacher;

            bool success = AuthService.Register(email, password, name, role);

            Console.Clear();
            PrintHeader("REGISTRATION");
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    Account created successfully!");
                Console.WriteLine($"    Name  : {name}");
                Console.WriteLine($"    Email : {email}");
                Console.WriteLine($"    Role  : {role}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    Registration failed. Email may already be registered.");
            }
            Console.ResetColor();
            Pause();
        }

        static UserAccount? LoginFlow()
        {
            Console.Clear();
            PrintHeader("LOGIN");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Email    : ");
            Console.ForegroundColor = ConsoleColor.White;
            string email = ReadLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("    Password : ");
            Console.ForegroundColor = ConsoleColor.White;
            string password = ReadPassword();

            var account = AuthService.Login(email, password);
            if (account is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n    Invalid email or password.");
                Console.ResetColor();
                Pause();
                return null;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n    Login successful! Welcome, {account.Name} ({account.Role})");
            Console.ResetColor();
            Pause();
            return account;
        }


        static void StudentDashboard(UserAccount account)
        {
            Student currentStudent = new(account.Name, account.Email);

            foreach (var subj in _subjects)
                subj.Enroll(currentStudent);

            Repository<Exam> sessionHistory = new();

            bool active = true;
            while (active)
            {
                string[] options =
                [
                    "Take Exam",
                    "View My Results",
                    "Session History",
                    "Logout"
                ];

                int choice = ArrowMenu($"STUDENT DASHBOARD - {account.Name}", options);

                switch (choice)
                {
                    case 0:
                        TakeExamFlow(currentStudent, sessionHistory);
                        break;
                    case 1:
                        ViewMyResults(account.Email);
                        break;
                    case 2:
                        ShowSessionHistory(sessionHistory);
                        break;
                    case 3 or -1:
                        active = false;
                        break;
                }
            }
        }

        static void TakeExamFlow(Student student, Repository<Exam> history)
        {
            string[] subjectOptions = new string[_subjects.Length + 1];
            for (int i = 0; i < _subjects.Length; i++)
            {
                var s = _subjects[i];
                subjectOptions[i] = $"{s.Name}  (Practice: {s.PracticalExam?.NumberOfQuestions}Q / Final: {s.FinalExam?.NumberOfQuestions}Q)";
            }
            subjectOptions[_subjects.Length] = "Back";

            int subjChoice = ArrowMenu("SELECT SUBJECT", subjectOptions);
            if (subjChoice < 0 || subjChoice >= _subjects.Length) return;

            Subject subject = _subjects[subjChoice];

            string[] examOptions =
            [
                $"Practice Exam  ({subject.PracticalExam?.NumberOfQuestions} questions, {subject.PracticalExam?.Time} min)",
                $"Final Exam     ({subject.FinalExam?.NumberOfQuestions} questions, {subject.FinalExam?.Time} min)",
                "Back"
            ];

            int examChoice = ArrowMenu($"{subject.Name.ToUpper()} - SELECT EXAM TYPE", examOptions);

            Exam? exam = examChoice switch
            {
                0 => subject.PracticalExam,
                1 => subject.FinalExam,
                _ => null
            };

            if (exam is null) return;

            RunExam(exam, student, subject, history);
        }

        static void RunExam(Exam exam, Student student, Subject subject, Repository<Exam> history)
        {
            Console.Clear();
            PrintHeader($"{subject.Name.ToUpper()} - {(exam is PracticeExam ? "PRACTICE" : "FINAL")} EXAM");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("    Notifying all enrolled students...\n");
            Console.ResetColor();

            ExamStartedHandler pauseHandler = (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n    All students notified. Press any key to begin the exam...");
                Console.ResetColor();
                Console.ReadKey(true);
            };
            exam.ExamStarted += pauseHandler;

            exam.Start();
            exam.Finish();

            ResultLogger.LogExamResult(student, subject, exam);

            var clone = (Exam)exam.Clone();
            foreach (var kvp in exam.QuestionAnswerDictionary)
                clone.QuestionAnswerDictionary[kvp.Key] = kvp.Value;
            history.Add(clone);

            exam.ExamStarted -= pauseHandler;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n    Results saved to file.");
            Console.ResetColor();
            Pause();
        }

        static void ViewMyResults(string email)
        {
            Console.Clear();
            PrintHeader("MY EXAM RESULTS");

            GradeRecord[] grades = ResultLogger.GetGradesForStudent(email);

            if (grades.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No exam results found.");
                Console.ResetColor();
                Pause();
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    #   Subject              Type       Score   Pct      Date");
            Console.WriteLine("    --  -------------------  ---------  ------  -------  -------------------");
            Console.ResetColor();

            for (int i = 0; i < grades.Length; i++)
            {
                var g = grades[i];
                double pct = g.Total > 0 ? (double)g.Score / g.Total * 100 : 0;
                Console.ForegroundColor = pct >= 50 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(
                    $"    {i + 1,-3} {g.SubjectName,-20} {g.ExamType,-10} {g.Score}/{g.Total,-4}  {pct,5:F1}%  {g.Date}");
                Console.ResetColor();
            }

            string[] files = ResultLogger.GetResultFilesForStudent(email);
            if (files.Length > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("    Press [D] to view detailed logs, or any other key to go back.");
                Console.ResetColor();

                if (Console.ReadKey(true).Key == ConsoleKey.D)
                    BrowseResultFiles(files);
            }
            else
            {
                Pause();
            }
        }

        static void BrowseResultFiles(string[] files)
        {
            string[] options = new string[files.Length + 1];
            for (int i = 0; i < files.Length; i++)
                options[i] = Path.GetFileNameWithoutExtension(files[i]);
            options[files.Length] = "Back";

            while (true)
            {
                int choice = ArrowMenu("SELECT RESULT LOG", options);
                if (choice < 0 || choice >= files.Length) return;

                Console.Clear();
                PrintHeader("DETAILED RESULT LOG");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ResultLogger.ReadResultFile(files[choice]));
                Console.ResetColor();
                Pause();
            }
        }

        static void ShowSessionHistory(Repository<Exam> history)
        {
            Console.Clear();
            PrintHeader("SESSION HISTORY");

            history.Sort();
            Exam[] exams = history.GetAll();

            if (exams.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No exams completed this session.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("    #   Type        Time   Questions   Score   Status");
                Console.WriteLine("    --  ----------  -----  ---------   -----   ----------");
                Console.ResetColor();

                for (int i = 0; i < exams.Length; i++)
                {
                    string type = exams[i] is PracticeExam ? "Practice" : "Final";
                    int correct = exams[i].CorrectExam();
                    int total = exams[i].Questions.Length;
                    double pct = total > 0 ? (double)correct / total * 100 : 0;

                    Console.ForegroundColor = pct >= 50 ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine(
                        $"    {i + 1,-3} {type,-10}  {exams[i].Time,3} m  {total,5}       {correct}/{total}     {exams[i].Mode}");
                    Console.ResetColor();
                }
            }

            Pause();
        }



        static void TeacherDashboard(UserAccount account)
        {
            bool active = true;
            while (active)
            {
                string[] options =
                [
                    "View All Registered Students",
                    "View All Exam Results",
                    "View Results by Student",
                    "Performance Summary",
                    "View Question Logs",
                    "Logout"
                ];

                int choice = ArrowMenu($"TEACHER DASHBOARD - {account.Name}", options);

                switch (choice)
                {
                    case 0: TeacherViewAllStudents(); break;
                    case 1: TeacherViewAllResults(); break;
                    case 2: TeacherViewResultsByStudent(); break;
                    case 3: TeacherPerformanceSummary(); break;
                    case 4: TeacherViewQuestionLogs(); break;
                    case 5 or -1: active = false; break;
                }
            }
        }

        static void TeacherViewAllStudents()
        {
            Console.Clear();
            PrintHeader("ALL REGISTERED STUDENTS");

            UserAccount[] students = AuthService.GetAllUsers(UserRole.Student);

            if (students.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No students registered yet.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("    #   Name                 Email");
                Console.WriteLine("    --  -------------------  -------------------------");
                Console.ResetColor();

                for (int i = 0; i < students.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"    {i + 1,-3} {students[i].Name,-20} {students[i].Email}");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n    Total: {students.Length} student(s)");
                Console.ResetColor();
            }

            Pause();
        }

        static void TeacherViewAllResults()
        {
            string[] files = ResultLogger.GetAllResultFiles();

            if (files.Length == 0)
            {
                Console.Clear();
                PrintHeader("ALL EXAM RESULTS");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No exam results found.");
                Console.ResetColor();
                Pause();
                return;
            }

            string[] options = new string[files.Length + 1];
            for (int i = 0; i < files.Length; i++)
                options[i] = Path.GetFileNameWithoutExtension(files[i]);
            options[files.Length] = "Back";

            while (true)
            {
                int choice = ArrowMenu("ALL EXAM RESULTS - SELECT TO VIEW", options);
                if (choice < 0 || choice >= files.Length) return;

                Console.Clear();
                PrintHeader("EXAM RESULT DETAILS");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ResultLogger.ReadResultFile(files[choice]));
                Console.ResetColor();
                Pause();
            }
        }

        static void TeacherViewResultsByStudent()
        {
            UserAccount[] students = AuthService.GetAllUsers(UserRole.Student);

            if (students.Length == 0)
            {
                Console.Clear();
                PrintHeader("VIEW RESULTS BY STUDENT");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No students registered yet.");
                Console.ResetColor();
                Pause();
                return;
            }

            string[] studentOptions = new string[students.Length + 1];
            for (int i = 0; i < students.Length; i++)
                studentOptions[i] = $"{students[i].Name} ({students[i].Email})";
            studentOptions[students.Length] = "Back";

            int studentChoice = ArrowMenu("SELECT STUDENT", studentOptions);
            if (studentChoice < 0 || studentChoice >= students.Length) return;

            string email = students[studentChoice].Email;
            string[] files = ResultLogger.GetResultFilesForStudent(email);

            if (files.Length == 0)
            {
                Console.Clear();
                PrintHeader($"RESULTS - {students[studentChoice].Name}");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No exam results found for this student.");
                Console.ResetColor();
                Pause();
                return;
            }

            string[] fileOptions = new string[files.Length + 1];
            for (int i = 0; i < files.Length; i++)
                fileOptions[i] = Path.GetFileNameWithoutExtension(files[i]);
            fileOptions[files.Length] = "Back";

            while (true)
            {
                int fileChoice = ArrowMenu($"RESULTS - {students[studentChoice].Name}", fileOptions);
                if (fileChoice < 0 || fileChoice >= files.Length) return;

                Console.Clear();
                PrintHeader("EXAM RESULT DETAILS");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ResultLogger.ReadResultFile(files[fileChoice]));
                Console.ResetColor();
                Pause();
            }
        }

        static void TeacherPerformanceSummary()
        {
            Console.Clear();
            PrintHeader("STUDENT PERFORMANCE SUMMARY");

            GradeRecord[] grades = ResultLogger.ReadAllGrades();

            if (grades.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No exam grades recorded yet.");
                Console.ResetColor();
                Pause();
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    #   Student              Subject              Type       Score   Pct      Date");
            Console.WriteLine("    --  -------------------  -------------------  ---------  ------  -------  -------------------");
            Console.ResetColor();

            for (int i = 0; i < grades.Length; i++)
            {
                var g = grades[i];
                double pct = g.Total > 0 ? (double)g.Score / g.Total * 100 : 0;
                Console.ForegroundColor = pct >= 50 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(
                    $"    {i + 1,-3} {g.StudentName,-20} {g.SubjectName,-20} {g.ExamType,-10} {g.Score}/{g.Total,-4}  {pct,5:F1}%  {g.Date}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;

            // Aggregate by student
            string[] seenEmails = new string[grades.Length];
            int seenCount = 0;

            Console.WriteLine("    --- Per-Student Averages ---");
            Console.WriteLine();

            for (int i = 0; i < grades.Length; i++)
            {
                bool alreadySeen = false;
                for (int j = 0; j < seenCount; j++)
                {
                    if (seenEmails[j].Equals(grades[i].Email, StringComparison.OrdinalIgnoreCase))
                    { alreadySeen = true; break; }
                }
                if (alreadySeen) continue;

                seenEmails[seenCount++] = grades[i].Email;

                int totalScore = 0, totalMax = 0, examCount = 0;
                for (int k = 0; k < grades.Length; k++)
                {
                    if (grades[k].Email.Equals(grades[i].Email, StringComparison.OrdinalIgnoreCase))
                    {
                        totalScore += grades[k].Score;
                        totalMax += grades[k].Total;
                        examCount++;
                    }
                }

                double avgPct = totalMax > 0 ? (double)totalScore / totalMax * 100 : 0;
                Console.ForegroundColor = avgPct >= 50 ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(
                    $"    {grades[i].StudentName,-20} Exams: {examCount}   Total: {totalScore}/{totalMax}   Avg: {avgPct:F1}%");
                Console.ResetColor();
            }

            Console.ResetColor();
            Pause();
        }

        static void TeacherViewQuestionLogs()
        {
            string[] logFiles = ["MathQuestions.log", "CSQuestions.log", "PhysicsQuestions.log"];
            string[] logNames = ["Mathematics Questions", "Computer Science Questions", "Physics Questions"];

            string[] existingFiles = new string[logFiles.Length];
            string[] existingNames = new string[logFiles.Length];
            int count = 0;

            for (int i = 0; i < logFiles.Length; i++)
            {
                if (File.Exists(logFiles[i]))
                {
                    existingFiles[count] = logFiles[i];
                    existingNames[count] = logNames[i];
                    count++;
                }
            }

            if (count == 0)
            {
                Console.Clear();
                PrintHeader("QUESTION LOGS");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("    No question log files found.");
                Console.ResetColor();
                Pause();
                return;
            }

            string[] options = new string[count + 1];
            Array.Copy(existingNames, options, count);
            options[count] = "Back";

            while (true)
            {
                int choice = ArrowMenu("QUESTION LOGS - SELECT TO VIEW", options);
                if (choice < 0 || choice >= count) return;

                Console.Clear();
                PrintHeader(existingNames[choice]);
                try
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    using var reader = new StreamReader(existingFiles[choice]);
                    Console.WriteLine(reader.ReadToEnd());
                    Console.ResetColor();
                }
                catch (IOException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"    Error reading file: {e.Message}");
                    Console.ResetColor();
                }
                Pause();
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  SEED DATA
        // ═══════════════════════════════════════════════════════════

        static Subject[] BuildSubjectsAndExams()
        {
            Subject[] subjects = [new("Mathematics"), new("Computer Science"), new("Physics")];

            // ── Mathematics ──
            Answer mA1 = new("5"); Answer mA2 = new("4"); Answer mA3 = new("6"); Answer mA4 = new("7");
            Answer mB1 = new("3"); Answer mB2 = new("2"); Answer mB3 = new("4"); Answer mB4 = new("6");
            Answer mC1 = new("56"); Answer mC2 = new("48"); Answer mC3 = new("54"); Answer mC4 = new("64");
            Answer mTrueAns = new("True");

            ChooseOneQuestion mQ1 = new("Arithmetic", "What is 2 + 3?", 2,
                new AnswerList([mA1, mA2, mA3, mA4]), mA1);
            ChooseOneQuestion mQ2 = new("Division", "What is 12 / 4?", 2,
                new AnswerList([mB1, mB2, mB3, mB4]), mB1);
            ChooseOneQuestion mQ3 = new("Multiplication", "What is 7 x 8?", 3,
                new AnswerList([mC1, mC2, mC3, mC4]), mC1);
            TrueFalseQuestion mQ4 = new("Constants", "Pi is approximately 3.14159.", 1, mTrueAns);

            _ = new QuestionList([mQ1, mQ2, mQ3, mQ4], "MathQuestions.log");
            subjects[0].CreatePracticalExam(5, 3, subjects[0], [mQ1, mQ2, mQ4]);
            subjects[0].CreateFinalExam(10, 3, subjects[0], [mQ1, mQ3, mQ2]);

            // ── Computer Science ──
            Answer cA1 = new("const"); Answer cA2 = new("static");
            Answer cA3 = new("readonly"); Answer cA4 = new("var");
            Answer cB1 = new("int"); Answer cB2 = new("string");
            Answer cB3 = new("bool"); Answer cB4 = new("double");
            Answer cC1 = new("Object Oriented Programming"); Answer cC2 = new("Open Object Protocol");
            Answer cC3 = new("Ordered Object Paradigm"); Answer cC4 = new("Original Operation Process");
            Answer cFalseAns = new("False");

            ChooseOneQuestion cQ1 = new("Keywords", "Which keyword declares a compile-time constant in C#?", 2,
                new AnswerList([cA1, cA2, cA3, cA4]), cA1);
            ChooseAllQuestion cQ2 = new("Types", "Which are value types in C#? (select all)", 3,
                new AnswerList([cB1, cB2, cB3, cB4]), [cB1, cB3, cB4]);
            ChooseOneQuestion cQ3 = new("Concepts", "What does OOP stand for?", 2,
                new AnswerList([cC1, cC2, cC3, cC4]), cC1);
            TrueFalseQuestion cQ4 = new("Inheritance", "C# supports multiple class inheritance.", 1, cFalseAns);

            _ = new QuestionList([cQ1, cQ2, cQ3, cQ4], "CSQuestions.log");
            subjects[1].CreatePracticalExam(5, 3, subjects[1], [cQ1, cQ2, cQ3]);
            subjects[1].CreateFinalExam(10, 3, subjects[1], [cQ1, cQ4, cQ3]);

            // ── Physics ──
            Answer pA1 = new("3 x 10^8 m/s"); Answer pA2 = new("3 x 10^6 m/s");
            Answer pA3 = new("3 x 10^10 m/s"); Answer pA4 = new("3 x 10^4 m/s");
            Answer pB1 = new("Gravity"); Answer pB2 = new("Friction");
            Answer pB3 = new("Electromagnetic"); Answer pB4 = new("Strong Nuclear");
            Answer pC1 = new("9.8 m/s^2"); Answer pC2 = new("10.2 m/s^2");
            Answer pC3 = new("8.9 m/s^2"); Answer pC4 = new("11.0 m/s^2");
            Answer pTrueAns = new("True");

            ChooseOneQuestion pQ1 = new("Optics", "What is the approximate speed of light in vacuum?", 2,
                new AnswerList([pA1, pA2, pA3, pA4]), pA1);
            ChooseAllQuestion pQ2 = new("Forces", "Which are fundamental forces of nature? (select all)", 3,
                new AnswerList([pB1, pB2, pB3, pB4]), [pB1, pB3, pB4]);
            ChooseOneQuestion pQ3 = new("Gravity", "What is the acceleration due to gravity on Earth?", 2,
                new AnswerList([pC1, pC2, pC3, pC4]), pC1);
            TrueFalseQuestion pQ4 = new("Thermodynamics", "Energy can neither be created nor destroyed.", 1, pTrueAns);

            _ = new QuestionList([pQ1, pQ2, pQ3, pQ4], "PhysicsQuestions.log");
            subjects[2].CreatePracticalExam(5, 3, subjects[2], [pQ1, pQ2, pQ3]);
            subjects[2].CreateFinalExam(10, 3, subjects[2], [pQ1, pQ4, pQ3]);

            return subjects;
        }

        // ═══════════════════════════════════════════════════════════
        //  EXIT
        // ═══════════════════════════════════════════════════════════

        static void ShowExit()
        {
            Console.Clear();
            PrintHeader("GOODBYE");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("    Thank you for using the Examination Management System!");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    Persistent data files:");
            Console.WriteLine("      users.dat                    - Registered accounts");
            Console.WriteLine("      grades.dat                   - Grade records");
            Console.WriteLine("      results/                     - Detailed exam result logs");
            Console.WriteLine("      MathQuestions.log            - Mathematics question bank");
            Console.WriteLine("      CSQuestions.log              - Computer Science question bank");
            Console.WriteLine("      PhysicsQuestions.log         - Physics question bank");
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.WriteLine();
        }
    }
}
