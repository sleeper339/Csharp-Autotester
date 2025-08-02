using System;
using Gtk;
using Npgsql;
using System.Security.Cryptography;

namespace autotester {
    static class DBResources {
        public static NpgsqlDataSource dataSource =
            new NpgsqlDataSourceBuilder
            (
            "Host=localhost;Username=mine;Database=autotester;"
            ).Build();
    }

    static class Colors {
        public static readonly Gdk.Color RED = new Gdk.Color(255, 0, 0);
        public static readonly Gdk.Color BLUE = new Gdk.Color(0, 0, 255);
        public static readonly Gdk.Color GREEN = new Gdk.Color(0, 255, 0);
        public static readonly Gdk.Color WHITE = new Gdk.Color(255, 255, 255);
        public static readonly Gdk.Color BLACK = new Gdk.Color(0, 0, 0);
    }

    static class UIUtils {
        public static HBox Label(string title, Widget w) {
            HBox hb = new HBox();
            hb.Visible = true;

            Label lb = new Label(title);
            lb.Visible = true;
            hb.PackStart(lb, false, false, 10);
            hb.PackStart(w, false, false, 10);
            return hb;
        }

        public static void ErrorMessage(Window parent, String message) {
                MessageDialog dial = new MessageDialog(parent, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, message);
                dial.Run();
                dial.Destroy();
        }
    }

    static class Randomizer {
        private static void Randomize<T>(T[] array, int length) {
            Random rnd = new Random();

            for(int i = 0; i < length - 1; i++) {
                int i2 = rnd.Next(i, length);

                T tmp = array[i2];
                array[i2] = array[i];
                array[i] = tmp;
            }
        }

        public static void Randomize<T>(T[] array) {
            Randomize<T>(array, array.Length);
        }
    }

    static class MD5Util {
        public static string HashString(string input) {
            using(MD5 md5 = MD5.Create()) {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }
    }
}
