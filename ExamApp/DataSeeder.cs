using ConsoleApp2;

namespace ExamApp;

public static class DataSeeder
{
    public static Subject[] BuildSubjects()
    {
        Subject[] subjects = [new("Mathematics"), new("Computer Science"), new("Physics")];

        // ── Mathematics ──
        Answer mA1 = new("5"), mA2 = new("4"), mA3 = new("6"), mA4 = new("7");
        Answer mB1 = new("3"), mB2 = new("2"), mB3 = new("4"), mB4 = new("6");
        Answer mC1 = new("56"), mC2 = new("48"), mC3 = new("54"), mC4 = new("64");
        Answer mTrueAns = new("True");

        var mQ1 = new ChooseOneQuestion("Arithmetic", "What is 2 + 3?", 2,
            new AnswerList([mA1, mA2, mA3, mA4]), mA1);
        var mQ2 = new ChooseOneQuestion("Division", "What is 12 / 4?", 2,
            new AnswerList([mB1, mB2, mB3, mB4]), mB1);
        var mQ3 = new ChooseOneQuestion("Multiplication", "What is 7 × 8?", 3,
            new AnswerList([mC1, mC2, mC3, mC4]), mC1);
        var mQ4 = new TrueFalseQuestion("Constants", "Pi is approximately 3.14159.", 1, mTrueAns);

        subjects[0].CreatePracticalExam(5, 3, subjects[0], [mQ1, mQ2, mQ4]);
        subjects[0].CreateFinalExam(10, 3, subjects[0], [mQ1, mQ3, mQ2]);

        // ── Computer Science ──
        Answer cA1 = new("const"), cA2 = new("static"), cA3 = new("readonly"), cA4 = new("var");
        Answer cB1 = new("int"), cB2 = new("string"), cB3 = new("bool"), cB4 = new("double");
        Answer cC1 = new("Object Oriented Programming"), cC2 = new("Open Object Protocol");
        Answer cC3 = new("Ordered Object Paradigm"), cC4 = new("Original Operation Process");
        Answer cFalseAns = new("False");

        var cQ1 = new ChooseOneQuestion("Keywords", "Which keyword declares a compile-time constant in C#?", 2,
            new AnswerList([cA1, cA2, cA3, cA4]), cA1);
        var cQ2 = new ChooseAllQuestion("Types", "Which are value types in C#? (select all)", 3,
            new AnswerList([cB1, cB2, cB3, cB4]), [cB1, cB3, cB4]);
        var cQ3 = new ChooseOneQuestion("Concepts", "What does OOP stand for?", 2,
            new AnswerList([cC1, cC2, cC3, cC4]), cC1);
        var cQ4 = new TrueFalseQuestion("Inheritance", "C# supports multiple class inheritance.", 1, cFalseAns);

        subjects[1].CreatePracticalExam(5, 3, subjects[1], [cQ1, cQ2, cQ3]);
        subjects[1].CreateFinalExam(10, 3, subjects[1], [cQ1, cQ4, cQ3]);

        // ── Physics ──
        Answer pA1 = new("3 × 10⁸ m/s"), pA2 = new("3 × 10⁶ m/s");
        Answer pA3 = new("3 × 10¹⁰ m/s"), pA4 = new("3 × 10⁴ m/s");
        Answer pB1 = new("Gravity"), pB2 = new("Friction");
        Answer pB3 = new("Electromagnetic"), pB4 = new("Strong Nuclear");
        Answer pC1 = new("9.8 m/s²"), pC2 = new("10.2 m/s²");
        Answer pC3 = new("8.9 m/s²"), pC4 = new("11.0 m/s²");
        Answer pTrueAns = new("True");

        var pQ1 = new ChooseOneQuestion("Optics", "What is the approximate speed of light in vacuum?", 2,
            new AnswerList([pA1, pA2, pA3, pA4]), pA1);
        var pQ2 = new ChooseAllQuestion("Forces", "Which are fundamental forces? (select all)", 3,
            new AnswerList([pB1, pB2, pB3, pB4]), [pB1, pB3, pB4]);
        var pQ3 = new ChooseOneQuestion("Gravity", "What is the acceleration due to gravity on Earth?", 2,
            new AnswerList([pC1, pC2, pC3, pC4]), pC1);
        var pQ4 = new TrueFalseQuestion("Thermodynamics", "Energy can neither be created nor destroyed.", 1, pTrueAns);

        subjects[2].CreatePracticalExam(5, 3, subjects[2], [pQ1, pQ2, pQ3]);
        subjects[2].CreateFinalExam(10, 3, subjects[2], [pQ1, pQ4, pQ3]);

        return subjects;
    }
}
