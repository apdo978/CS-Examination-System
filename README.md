# Examination Management System

A fully-featured, console-based Examination Management System built with **C# 14** and **.NET 10**, demonstrating advanced object-oriented design, event-driven architecture, file-based persistence, and role-based access control.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Design Patterns & OOP Concepts](#design-patterns--oop-concepts)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Usage Guide](#usage-guide)
- [File Persistence](#file-persistence)
- [Class Diagram](#class-diagram)
- [Sample Output](#sample-output)
- [Design Decisions](#design-decisions)
- [Technical Constraints](#technical-constraints)

---

## Overview

This system allows **students** to register, log in, and take exams across multiple subjects, while **teachers** can monitor student performance, review detailed answer logs, and browse question banks — all through an interactive console UI with arrow-key navigation.

Two users can run the application **simultaneously in separate terminals**: a student taking exams in one, and a teacher reviewing results in real-time in another — powered entirely by shared flat-file persistence.

---

## Features

### Authentication & Roles
- Email + password registration with **SHA-256** hashing
- Role selection: **Student** or **Teacher**
- Persistent accounts stored in `users.dat`
- Masked password input (displays `****`)
- Duplicate email detection and input validation

### Student Experience
- **Take Exam** — select a subject, choose Practice or Final, answer questions with arrow-key navigation
- **Three question types**: True/False, Choose One, Choose All (multi-select with Space to toggle)
- **Practice Exam** — shows correct/wrong per question with color coding + final grade
- **Final Exam** — shows only student answers (no correct answers revealed)
- **View My Results** — color-coded grade table from `grades.dat` with drill-down into detailed logs
- **Session History** — in-memory `Repository<Exam>` view for the current session
- **Event notifications** — all enrolled students receive `[Notification]` when an exam starts

### Teacher Experience
- **View All Registered Students** — reads `users.dat`, displays student table
- **View All Exam Results** — browse every result log in the `results/` directory
- **View Results by Student** — select a student, see all their exam attempts
- **Performance Summary** — aggregate grade table from `grades.dat` with per-student averages
- **View Question Logs** — read `MathQuestions.log`, `CSQuestions.log`, `PhysicsQuestions.log`

### Console UI
- Interactive arrow-key menus with highlighted cursor
- Color-coded output (green = correct/pass, red = wrong/fail, cyan = info, magenta = headers)
- Animated splash screen
- Clean screen transitions between every view

---

## Architecture

```
Program.cs (Entry Point + UI Orchestration)
    |
    +-- Auth.cs .............. UserRole, UserAccount, AuthService (file I/O)
    +-- Exam.cs .............. Exam hierarchy, Subject, Student
    +-- Qustions.cs .......... Question hierarchy, Answer, AnswerList, QuestionList
    +-- Teacher.cs ........... Teacher domain class
    +-- ResultLogger.cs ...... GradeRecord, ResultLogger (file I/O)
    +-- Repository.cs ........ Generic Repository<T> (array-backed)
    +-- Event Infrastructure.. ExamStartedHandler delegate, ExamEventArgs
```

### Data Flow

```
Registration ──> users.dat
                     |
Login ───────────────+
    |                |
    v                v
 Student          Teacher
    |                |
    v                |
 Take Exam           |
    |                |
    v                v
 results/*.log <── View Results
 grades.dat    <── Performance Summary
```

---

## Design Patterns & OOP Concepts

| Concept | Implementation |
|---|---|
| **Inheritance** | `Question` → `TrueFalseQuestion`, `ChooseOneQuestion`, `ChooseAllQuestion`; `Exam` → `PracticeExam`, `FinalExam` |
| **Polymorphism** | `CheckAnswer()` dispatches differently per question type; `Finish()` shows different output for Practice vs Final |
| **Abstract Classes** | `Question` (abstract `Display()`, `CheckAnswer()`); `Exam` (abstract `ShowExam()`, `Clone()`) |
| **Interfaces** | `ICloneable` on `Exam`; `IComparable<T>` on `Exam`, `Answer`; `IEquatable<T>` on `Answer` |
| **Generics + Constraints** | `Repository<T> where T : ICloneable, IComparable<T>` — array-backed with Add, Remove, Sort, GetAll |
| **Events & Delegates** | `ExamStartedHandler` delegate; `ExamStarted` event on `Exam`; `Student.OnExamStarted` handler; `Subject` wires subscriptions on enrollment |
| **Enums** | `ExamMode` (Starting, Queued, Finished); `UserRole` (Student, Teacher) |
| **Operator Overriding** | `ToString()`, `Equals()`, `GetHashCode()` overridden on `Answer`, `Question`, `Exam` |
| **Composition** | `Subject` contains `Student[]`, `PracticeExam`, `FinalExam`; `Question` contains `AnswerList` |
| **Encapsulation** | Private backing fields with validated properties (e.g., `Marks > 0`, non-null strings) |
| **File I/O** | `StreamWriter`/`StreamReader` with `using` statements for `QuestionList` logging, auth persistence, result logging |
| **Exception Handling** | `ArgumentNullException`, `ArgumentException`, `KeyNotFoundException`, `IndexOutOfRangeException`, `IOException` handling throughout |
| **Arrays over List\<T\>** | All internal collections use `T[]` with dynamic resizing (Repository, AnswerList, Subject.EnrolledStudents) — except `QuestionList` which extends `List<Question>` per spec |

---

## Project Structure

```
ConsoleApp2/
├── ConsoleApp2.csproj          # .NET 10, C# 14, nullable enabled
├── Program.cs                  # Entry point, UI, auth flow, dashboards
├── Auth.cs                     # UserRole, UserAccount, AuthService
├── Exam.cs                     # ExamMode, Exam, PracticeExam, FinalExam, Subject, Student
├── Qustions.cs                 # Answer, AnswerList, Question, TrueFalse/ChooseOne/ChooseAll, QuestionList
├── Teacher.cs                  # Teacher domain class
├── Repository.cs               # Generic array-backed Repository<T>
├── ResultLogger.cs             # GradeRecord, ResultLogger
├── Event Infrastructure .cs    # ExamStartedHandler delegate, ExamEventArgs
│
├── users.dat                   # [Generated] Registered accounts
├── grades.dat                  # [Generated] Grade records
├── results/                    # [Generated] Detailed exam result logs
│   └── *.log
├── MathQuestions.log            # [Generated] Math question bank
├── CSQuestions.log              # [Generated] CS question bank
└── PhysicsQuestions.log         # [Generated] Physics question bank
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Windows / macOS / Linux terminal

### Build & Run

```bash
cd ConsoleApp2
dotnet build
dotnet run
```

### Multi-Terminal Demo

1. **Terminal 1** — Register as a **Student**, take some exams
2. **Terminal 2** — Register as a **Teacher**, view results in real-time

Both terminals share the same `users.dat`, `grades.dat`, and `results/` directory.

---

## Usage Guide

### Registration
1. Launch the app → Select **Register**
2. Enter: Full Name, Email, Password (masked), Confirm Password
3. Select role: **Student** or **Teacher**
4. Account is saved to `users.dat` with SHA-256 hashed password

### Student Workflow
1. **Login** with registered email + password
2. **Take Exam** → Select subject (Mathematics / Computer Science / Physics) → Select type (Practice / Final)
3. All enrolled students receive `[Notification]` via the event system
4. Answer questions using **arrow keys** + **Enter** (or **Space** to toggle for multi-select)
5. View results → answers logged to `results/` directory + grade appended to `grades.dat`
6. **View My Results** for a color-coded grade table across all past exams

### Teacher Workflow
1. **Login** with a Teacher account
2. **View All Students** — see every registered student
3. **View All Results** — browse and read any student's detailed exam log
4. **View by Student** — drill into a specific student's history
5. **Performance Summary** — aggregate table with per-student averages
6. **Question Logs** — read the question banks logged by `QuestionList`

---

## File Persistence

| File | Format | Purpose |
|---|---|---|
| `users.dat` | `email\|hash\|role\|name` | Registered accounts |
| `grades.dat` | `email\|name\|subject\|type\|score\|total\|date` | Append-only grade ledger |
| `results/*.log` | Human-readable text | Per-attempt detailed answer logs |
| `MathQuestions.log` | Question.ToString() | Mathematics question bank |
| `CSQuestions.log` | Question.ToString() | Computer Science question bank |
| `PhysicsQuestions.log` | Question.ToString() | Physics question bank |

All file I/O uses `StreamWriter`/`StreamReader` with proper `using` statements and `IOException` handling.

---

## Class Diagram

```
                        ICloneable    IComparable<Exam>
                             \           /
                              \         /
    ExamStartedHandler ──event── [Exam] (abstract)
         |                     /    |    \
         |            PracticeExam  |  FinalExam
         |                          |
    ExamEventArgs              ExamMode (enum)

    [Question] (abstract)
       /      |        \
  TrueFalse  ChooseOne  ChooseAll
       \      |        /
        AnswerList ── Answer : IEquatable, IComparable

    QuestionList : List<Question>  (+ file logging)

    Subject ──contains──> Student[], PracticeExam, FinalExam
    Student ──handler──> ExamStartedHandler

    Repository<T> where T : ICloneable, IComparable<T>

    AuthService ──persists──> UserAccount (UserRole enum)
    ResultLogger ──persists──> GradeRecord
    Teacher
```

---

## Sample Output

### Splash Screen
```
  +==================================================+
  |                                                  |
  |       EXAMINATION  MANAGEMENT  SYSTEM            |
  |                                                  |
  |       Console-Based Exam Platform  v2.0          |
  |       .NET 10 / C# 14                            |
  |                                                  |
  +==================================================+
```

### Student Notification (Event System)
```
    [Notification] Student Ahmed: Exam for 'Mathematics' has started!
    [Notification] Student Sara: Exam for 'Mathematics' has started!
    [Notification] Student Mohamed: Exam for 'Mathematics' has started!
    [Notification] Student John: Exam for 'Mathematics' has started!
```

### Practice Exam Result
```
  ********************************************

  [CORRECT] What is 2 + 3? - Correct Answer: id:0, 5
  [WRONG]   What is 12 / 4? - Your Answer: 2 - Correct Answer: id:4, 3

  ********************************************

  Your Grade: 2 / 4
```

### Teacher Performance Summary
```
    #   Student              Subject              Type       Score   Pct      Date
    --  -------------------  -------------------  ---------  ------  -------  -------------------
    1   John                 Mathematics          Practice   4/5     80.0%    2025-07-11 14:30:22
    2   John                 Computer Science     Final      5/7     71.4%    2025-07-11 14:35:10

    --- Per-Student Averages ---

    John                 Exams: 2   Total: 9/12   Avg: 75.0%
```

### Result Log File (results/*.log)
```
Exam Result Log
==============================================
Student : John (ID: 4)
Email   : john@example.com
Subject : Mathematics
Type    : Practice
Date    : 2025-07-11 14:30:22
==============================================

Q: Arithmetic - What is 2 + 3?
   Marks         : 2
   Your Answer   : 5
   Correct Answer: 5
   Result        : CORRECT

==============================================
Final Score : 4 / 5
Percentage  : 80.0%
```

---

## Design Decisions

1. **Arrays over List\<T\>** — All internal storage uses `T[]` with manual resizing to demonstrate low-level data structure management. `QuestionList` extends `List<Question>` as a deliberate exception per the specification.

2. **File-based persistence over database** — Flat files (`users.dat`, `grades.dat`, `results/`) enable multi-terminal operation without a database server. Two processes can share data through the file system.

3. **SHA-256 password hashing** — Passwords are never stored in plaintext. `System.Security.Cryptography.SHA256` produces a one-way hash before writing to `users.dat`.

4. **Event-driven notifications** — The `ExamStarted` event on `Exam` uses the observer pattern via C# delegates. `Subject` wires student handlers on enrollment, so all enrolled students are automatically notified when any exam starts.

5. **Separate Finish() behavior** — `PracticeExam.Finish()` reveals correct answers and grades for learning. `FinalExam.Finish()` shows only student answers to simulate real exam conditions.

6. **Repository\<T\> with generic constraints** — `where T : ICloneable, IComparable<T>` ensures type safety at compile time. The repository provides Add, Remove, Sort (via `Array.Sort`), and GetAll operations on an array-backed store.

7. **QuestionList file logging** — Each `QuestionList` logs questions to a unique file on construction and on each `Add()`, using `StreamWriter` in append mode with proper resource disposal.

---

## Technical Constraints

- **.NET 10** / **C# 14** — uses collection expressions (`[]`), pattern matching, file-scoped namespaces-compatible code
- **No external packages** — zero NuGet dependencies, pure BCL
- **Nullable reference types** enabled — all null paths handled with `?` and `??` operators
- **No List\<T\> in domain models** — all collections use `T[]` with dynamic resizing
- **SOLID principles** applied — single responsibility per class, open for extension (new question types), dependency on abstractions (interfaces)
