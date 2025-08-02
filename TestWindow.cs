using System;
using Gtk;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace autotester {

    class Timer : Label {
        int secs, startSecs;  //StartSecs lol
        Thread timer;
        bool run;

        public Timer(int nSecs): base(Timestamp(nSecs)) {
            startSecs = secs = nSecs;
            run = true;

            timer = new Thread(new ThreadStart(TimerLoop));
            timer.Start();
        }

        static string Timestamp(int sec) {
            string buffer = "";

            buffer += (sec / 3600).ToString("00") + " : ";
            sec = sec % 3600;
            buffer += (sec / 60).ToString("00") + " : ";
            buffer += (sec % 60).ToString("00");

            return buffer;
        }

        public void Stop() {
            run = false;
        }

        void TimerLoop() {
            while(run && secs > 0) {
                Thread.Sleep(1000);

                Gtk.Application.Invoke(delegate
                {
                    Markup = Timestamp(secs);

                    if(Math.Abs(secs * 4 - startSecs) < 4)
                        ModifyFg(StateType.Normal, Colors.RED);
                    secs--;
                });
            }
        }
    }

    class Option {
        int optionId;
        public RadioButton rb;

        public int OptionId {
            get {
                return optionId;
            }
        }

        public Option(int id, string opt, RadioButton bs) {
            optionId = id;
            rb = new RadioButton(bs, opt);
            rb.Visible = true;
        }
    }

    class TestBox : HBox {
        Option[] options;
        Label question;
        int questionId;

        public TestBox(int questionId, string questionMarkup, Option[] options): base(false, 41) {
            this.questionId = questionId;
            VBox vb = new VBox();
            this.options = options;

            question = new Label();
            question.Visible = true;
            question.Markup = questionMarkup;

            Visible = true;
            vb.Add(question);
            vb.Visible = true;
            Add(vb);

            vb = new VBox();
            vb.Visible = true;

            foreach(var op in options)
                vb.Add(op.rb);
            Add(vb);
        }

        public int QuestionId {
            get {
                return questionId;
            }
        }

        public int SelectedOption {
            get {
                foreach(var opt in options)
                    if(opt.rb.Active == true)
                        return opt.OptionId;
                return -1;
            }
        }
    }

    class TestWindow : Window {
        Label display;
        TestBox[] questions;
        Timer testTimer;
        VBox mainBox;
        Button next, previous, submit;
        int currentQuestion = 0;
        int studentId, examId;

        void OnDelete(Object obj, DeleteEventArgs args) {
            testTimer.Stop();
        }

        void SubmitAnswers() {
            string buffer = "";

            for(int i = 0; i < questions.Length; i++) {
                if(i > 0)
                    buffer += ", ";
                buffer += "(" + studentId + ", " + questions[i].QuestionId + ", " +
                    questions[i].SelectedOption + ")";
            }

            string commandString = "INSERT INTO tblAnswers(answeredByStudent, answerForQuestion, answerOption) VALUES "
                + buffer;

            using(var connection = DBResources.dataSource.OpenConnection())
            using(var command = new NpgsqlCommand(commandString, connection)) {
                command.ExecuteNonQuery();
            }

            using(var connection = DBResources.dataSource.OpenConnection())
            using(var command = new NpgsqlCommand($"select getScoreForExam({studentId}, {examId})", connection))
            using(var reader = command.ExecuteReader())
            {
                reader.Read();
                Console.WriteLine("You scored: " + reader.GetDouble(0));
            }
        }

        void NextQuestion(Object obj, EventArgs args) {
            if(currentQuestion < questions.Length - 1) {
                mainBox.Remove(questions[currentQuestion++]);
                mainBox.Add(questions[currentQuestion]);
            }
        }

        void PrevQuestion(Object obj, EventArgs args) {
            if(currentQuestion > 0) {
                mainBox.Remove(questions[currentQuestion--]);
                mainBox.Add(questions[currentQuestion]);
            }
        }

        void OnSubmitClick(Object obj, EventArgs args) {
            SubmitAnswers();
            Visible = false;
        }

        public TestWindow(int si, int ei):base("") {
            studentId = si;
            examId = ei;

            DeleteEvent += OnDelete;

            using (var connection = DBResources.dataSource.OpenConnection()) {

                using (var query = new NpgsqlCommand($"SELECT examAllotedTime, examName FROM tblExam WHERE examId = {examId}", connection))
                using(var result = query.ExecuteReader())
                {
                    result.Read();
                    testTimer = new Timer((int)result.GetTimeSpan(0).TotalSeconds);
                    Title = result.GetString(1);
                }

            }

            next = new Button("Next >");
            previous = new Button("< Prev");
            submit = new Button("Submit");
            submit.Visible = true;
            submit.Clicked += OnSubmitClick;
            next.Clicked += NextQuestion;
            previous.Clicked += PrevQuestion;
            HBox bar = new HBox();
            bar.PackStart(previous, false, false, 3);
            bar.PackStart(submit, false, false, 2);
            bar.PackStart(next, false, false, 3);
            bar.Visible = true;

            mainBox = new VBox();
            HBox topBar = new HBox();
            testTimer.Xalign = 1;
            testTimer.Visible = true;
            topBar.Visible = true;
            topBar.Add(bar);
            topBar.Add(testTimer);

            mainBox.PackStart(topBar, false, false, 0);

            Add(mainBox);

            DefaultWidth = 700;
            DefaultHeight = 300;
            Resizable = false;

            display = new Label();
            LoadQuestions();
            ShowAll();
        }

        private void LoadQuestions() {
            Option[] options = null;
            var str = "";

            var conn = DBResources.dataSource.OpenConnection();
            var conn2 = DBResources.dataSource.OpenConnection();

            List<TestBox> questionList = new List<TestBox>();

            using (var qCmd = new NpgsqlCommand ("SELECT questionId, questionContent FROM tblQuestion", conn))
            using (var qReader = qCmd.ExecuteReader())
                {
                    while(qReader.Read()) {
                        int qId = (int)qReader.GetDouble(0);
                        str = qReader.GetString(1);

                        List<Option> optionsList = new List<Option>();
                        using (var cmd = new NpgsqlCommand("SELECT optionId, optionContent FROM tblOption WHERE optionQuestion = " + qId, conn2))
                        using (var reader = cmd.ExecuteReader())
                        {
                            RadioButton nothingSelected = new RadioButton(null, "");

                            while(reader.Read()) {
                                int id = (int)reader.GetDouble(0);
                                string content = reader.GetString(1);

                                Option op = new Option(id, content, nothingSelected);
                                optionsList.Add(op);
                                op.rb.Visible = true;
                            }
                        }

                        options = optionsList.ToArray();
                        Randomizer.Randomize<Option>(options);
                        if(options[0] != null){
                            options[0].rb.Active = true;
                        }

                        questionList.Add(new TestBox(qId, str, options));

                    }
                }

                questions = questionList.ToArray();
                Randomizer.Randomize<TestBox>(questions);

                if(questions.Length > 0 && questions[0] != null)
                    mainBox.Add(questions[0]);
        }

    }

}
