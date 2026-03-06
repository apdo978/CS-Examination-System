using System;

namespace ConsoleApp2
{
    public delegate void ExamStartedHandler(object sender, ExamEventArgs e);

    public class ExamEventArgs : EventArgs
    {
        public Subject Subject { get; }
        public Exam Exam { get; }

        public ExamEventArgs(Subject subject, Exam exam)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Exam = exam ?? throw new ArgumentNullException(nameof(exam));
        }
    }
}
