using ConsoleApp2;

namespace ExamApp.Controls;

public class ExamResultControl : UserControl
{
    public ExamResultControl(GradeRecord[] grades, string studentName,
        Exam? latestExam = null, Subject? latestSubject = null)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.BgDark;
        AutoScroll = true;
        Padding = new Padding(40, 28, 40, 28);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
        };

        // ── Latest exam breakdown ──
        if (latestExam != null && latestSubject != null)
        {
            bool isPractice = latestExam is PracticeExam;
            string examType = isPractice ? "Practice" : "Final";

            int earned = 0, total = 0;
            foreach (var kvp in latestExam.QuestionAnswerDictionary)
            {
                total += kvp.Key.Marks;
                if (kvp.Key.CheckAnswer(kvp.Value)) earned += kvp.Key.Marks;
            }

            double pct = total > 0 ? (double)earned / total * 100 : 0;
            Color scoreColor = pct >= 80 ? Theme.Success : pct >= 50 ? Theme.Warning : Theme.Danger;

            var headerCard = new RoundedPanel
            {
                Width = 700,
                Height = 110,
                BackColor = Theme.BgCard,
                Margin = new Padding(0, 0, 0, 20),
            };

            var lblExamTitle = new Label
            {
                Text = $"{latestSubject.Name} — {examType} Exam Results",
                Font = Theme.FontTitle,
                ForeColor = Theme.Accent,
                Location = new Point(28, 16),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            headerCard.Controls.Add(lblExamTitle);

            var lblScore = new Label
            {
                Text = $"Score: {earned}/{total}  ({pct:F0}%)",
                Font = Theme.FontSubtitle,
                ForeColor = scoreColor,
                Location = new Point(28, 62),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            headerCard.Controls.Add(lblScore);

            var lblBadge = new Label
            {
                Text = pct >= 80 ? "🌟 Excellent!" : pct >= 50 ? "✅ Passed" : "❌ Failed",
                Font = Theme.FontSubtitle,
                ForeColor = scoreColor,
                AutoSize = true,
                Location = new Point(500, 62),
                BackColor = Color.Transparent,
            };
            headerCard.Controls.Add(lblBadge);

            flow.Controls.Add(headerCard);

            // Question-by-question breakdown
            int qNum = 0;
            foreach (var kvp in latestExam.QuestionAnswerDictionary)
            {
                qNum++;
                var q = kvp.Key;
                var studentAns = kvp.Value;
                bool correct = q.CheckAnswer(studentAns);

                var qCard = new RoundedPanel
                {
                    Width = 700,
                    BackColor = Theme.BgCard,
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(24, 16, 24, 16),
                    AutoSize = true,
                };

                int cy = 14;

                var lblQ = new Label
                {
                    Text = $"Q{qNum}. {q.Body}",
                    Font = Theme.FontBodyBold,
                    ForeColor = Theme.TextPrimary,
                    Location = new Point(24, cy),
                    MaximumSize = new Size(640, 0),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                };
                qCard.Controls.Add(lblQ);
                cy += lblQ.GetPreferredSize(new Size(640, 0)).Height + 8;

                // For practice: show correct/wrong with details
                if (isPractice)
                {
                    string studentAnswerText = ResolveAnswerText(q, studentAns);
                    var lblYour = new Label
                    {
                        Text = $"Your answer: {studentAnswerText}",
                        Font = Theme.FontBody,
                        ForeColor = correct ? Theme.Success : Theme.Danger,
                        Location = new Point(24, cy),
                        AutoSize = true,
                        BackColor = Color.Transparent,
                    };
                    qCard.Controls.Add(lblYour);
                    cy += 26;

                    if (!correct)
                    {
                        var lblCorrect = new Label
                        {
                            Text = $"Correct answer: {q.CorrectAnswer.Text}",
                            Font = Theme.FontBody,
                            ForeColor = Theme.Success,
                            Location = new Point(24, cy),
                            AutoSize = true,
                            BackColor = Color.Transparent,
                        };
                        qCard.Controls.Add(lblCorrect);
                        cy += 26;
                    }
                }
                else
                {
                    // Final: only show student's answer
                    string studentAnswerText = ResolveAnswerText(q, studentAns);
                    var lblYour = new Label
                    {
                        Text = $"Your answer: {studentAnswerText}",
                        Font = Theme.FontBody,
                        ForeColor = Theme.TextSecondary,
                        Location = new Point(24, cy),
                        AutoSize = true,
                        BackColor = Color.Transparent,
                    };
                    qCard.Controls.Add(lblYour);
                    cy += 26;
                }

                var lblStatus = new Label
                {
                    Text = correct ? "✓ CORRECT" : (isPractice ? "✗ WRONG" : "—"),
                    Font = Theme.FontBodyBold,
                    ForeColor = correct ? Theme.Success : (isPractice ? Theme.Danger : Theme.TextMuted),
                    Location = new Point(24, cy),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                };
                qCard.Controls.Add(lblStatus);
                cy += 30;

                qCard.Height = cy + 10;
                flow.Controls.Add(qCard);
            }

            // separator
            flow.Controls.Add(new Panel { Height = 20, Width = 10, BackColor = Color.Transparent });
        }

        // ── Grade history table ──
        var lblHistory = Theme.MakeLabel("📊  Grade History", Theme.FontSubtitle);
        lblHistory.Margin = new Padding(0, 0, 0, 12);
        flow.Controls.Add(lblHistory);

        if (grades.Length == 0)
        {
            var lblEmpty = Theme.MakeLabel("No exam results yet.", Theme.FontBody, Theme.TextMuted);
            flow.Controls.Add(lblEmpty);
        }
        else
        {
            var grid = Theme.MakeGrid();
            grid.Height = Math.Min(300, 44 + grades.Length * 40);
            grid.Width = 700;
            grid.Dock = DockStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Add("Subject", "Subject");
            grid.Columns.Add("Type", "Type");
            grid.Columns.Add("Score", "Score");
            grid.Columns.Add("Pct", "%");
            grid.Columns.Add("Date", "Date");

            foreach (var g in grades)
            {
                double pct = g.Total > 0 ? (double)g.Score / g.Total * 100 : 0;
                grid.Rows.Add(g.SubjectName, g.ExamType, $"{g.Score}/{g.Total}", $"{pct:F0}%", g.Date);
            }

            // color code rows
            grid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                var pctStr = grid.Rows[e.RowIndex].Cells["Pct"].Value?.ToString() ?? "0";
                double.TryParse(pctStr.TrimEnd('%'), out double val);
                if (val >= 80) e.CellStyle!.ForeColor = Theme.Success;
                else if (val >= 50) e.CellStyle!.ForeColor = Theme.Warning;
                else e.CellStyle!.ForeColor = Theme.Danger;
            };

            flow.Controls.Add(grid);
        }

        Controls.Add(flow);
    }

    private static string ResolveAnswerText(Question q, Answer studentAnswer)
    {
        if (string.IsNullOrWhiteSpace(studentAnswer.Text)) return "(no answer)";

        if (q is ChooseAllQuestion)
        {
            var parts = studentAnswer.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var texts = new List<string>();
            foreach (var idStr in parts)
            {
                if (int.TryParse(idStr.Trim(), out int id))
                {
                    try { texts.Add(q.Answers.GetById(id).Text); }
                    catch { texts.Add(idStr); }
                }
            }
            return texts.Count > 0 ? string.Join(", ", texts) : "(no answer)";
        }

        if (int.TryParse(studentAnswer.Text, out int answerId))
        {
            try { return q.Answers.GetById(answerId).Text; }
            catch { }
        }
        return studentAnswer.Text;
    }
}
