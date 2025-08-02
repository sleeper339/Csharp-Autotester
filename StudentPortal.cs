using System;
using Gtk;
using Npgsql;

namespace autotester {
    class Login : Window {
        Button cancel, login, register;
        Entry userName, passWord;

        public Login(): base("Student Login") {
            Image im = new Image();
            im.Visible = true;
            im.SetSizeRequest(10, 10);
            im.Pixbuf = new Gdk.Pixbuf("assets/login_s.png");

            Resizable = false;
            Visible = true;
            DeleteEvent += OnDelete;

            cancel = new Button("Cancel");
            cancel.Visible = true;
            cancel.Clicked += OnCancelClick;

            userName = new Entry();
            userName.Visible = true;

            passWord = new Entry();
            passWord.Visible = true;
            passWord.Visibility = false;

            login = new Button("Login");
            login.Visible = true;
            login.Clicked += OnLoginClick;

            register = new Button("Register");
            register.Visible = true;

            HBox hb = new HBox();
            hb.PackStart(login, false, false, 10);
            hb.PackStart(register, false, false, 10);
            hb.PackStart(cancel, false, false, 10);

            Fixed layout = new Fixed();
            layout.Visible = true;
            layout.Put(hb, 20, 300);

            layout.Put(UIUtils.Label("  Email:", userName), 64, 160);
            layout.Put(UIUtils.Label("Password: ", passWord), 40, 200);
            layout.Put(im, 143, 75);

            DefaultWidth = 350;
            DefaultHeight = 400;
            Add(layout);
            ShowAll();
        }

        /****************
         *Event Handlers*
         ****************/
        void OnDelete(object sender, DeleteEventArgs e) {
            Application.Quit();
        }

        void OnCancelClick(object sender, EventArgs e) {
            Application.Quit();
        }

        void OnLoginClick(object sender, EventArgs args) {
            string passwordHash = MD5Util.HashString(passWord.Text);
            string commandString = $"SELECT studentId FROM tblStudent WHERE " +
                $"studentEmail = '{userName.Text}' AND studentPasswordHash = '{passwordHash}'";

            using(var conn = DBResources.dataSource.OpenConnection())
            using(var command = new NpgsqlCommand(commandString, conn))
            try
            {
                using(var result = command.ExecuteReader()) {
                    result.Read();

                    int studentId = result.GetInt32(0);
                    new StudentPortal(studentId);
                    Visible = false;
                }
            } catch(Exception e) {
                UIUtils.ErrorMessage(this, "Invalid Username Or Password!");
            }
        }
    }

    class StudentPortal : Window {
        VBox pastExams;
        void OnDelete(object sender, EventArgs e) {
            Application.Quit();
        }

        public StudentPortal(int student): base("Student Portal") {
            Resizable = false;
            Visible = true;
            DeleteEvent += OnDelete;
            SetDefaultSize(900, 600);

            pastExams = new VBox();
            ScrolledWindow swin = new ScrolledWindow();
            swin.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            pastExams.ModifyBg(StateType.Normal, Colors.WHITE);

            swin.AddWithViewport(pastExams);
            Add(swin);
        }
    }

}
