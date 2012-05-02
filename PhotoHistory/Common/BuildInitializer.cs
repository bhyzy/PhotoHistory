using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Npgsql;
using System.IO;
using System.Reflection;
using PhotoHistory.Models;
using PhotoHistory.Data;

namespace PhotoHistory.Common
{   //Zakomentujcie odpowiednie linie w InitializeBuild jesli nie chcecie zeby wam modyfikowalo zawartosc bazy
    public class BuildInitializer
    {
        public static void InitializeBuild()
        {
            //ExecuteSQLFile("PhotoHistory.sql.create_tables.sql");
            //CreateUsers();
            //CreateCategories();
        }

        private static void CreateCategories()
        {
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                int category_count = session.QueryOver<CategoryModel>().RowCount();
                if (category_count == 0)
                {
                    CategoryModel[] categories = new CategoryModel[6];
                    categories[0] = new CategoryModel() { Name = "People" };
                    categories[1] = new CategoryModel() { Name = "Building" };
                    categories[2] = new CategoryModel() { Name = "City" };
                    categories[3] = new CategoryModel() { Name = "Landscape" };
                    categories[4] = new CategoryModel() { Name = "Room" };
                    categories[5] = new CategoryModel() { Name = "Other" };

                    using (var trans = session.BeginTransaction())
                    {
                        foreach (var cat in categories)
                            session.Save(cat);
                        trans.Commit();
                    }
                }
            }
        }

        private static void CreateUsers()
        {
            UserRepository users = new UserRepository();
            
            UserModel user = new UserModel()
            {
                Login = "Klocu",
                Password = "qwe".HashMD5(),
                Email = "qbajas@gmail.com",
                ActivationCode = null,
                DateOfBirth = new DateTime(1989,5,5),
                About = " ksadjolkrewjof jlksajflkjrwelkfmslk ajfsaljm lkmaslfj lmflkajlos jmlwema jslafmawl",
                NotifyComment = true,
                NotifyPhoto = true,
                NotifySubscription = true
            };
            users.Create(user);

            user = new UserModel()
            {
                Login = "Pierogu",
                Password = "qwe".HashMD5(),
                Email = "pierogmichal@gmail.com",
                ActivationCode = null,
                DateOfBirth = new DateTime(1989, 10, 15),
                About = "salrjwelkf jlksadjlkf jlkr jflajs ljoiwej ljal;fjal jlafnl lsfdaa;sl jofiewj laf",
                NotifyComment = true,
                NotifyPhoto = true,
                NotifySubscription = true
            };
            users.Create(user);

            user = new UserModel()
            {
                Login = "BH",
                Password = "qwe".HashMD5(),
                Email = "hyzy.bartlomiej@gmail.com",
                ActivationCode = null,
                DateOfBirth = new DateTime(1989, 3, 1),
                About = "klfasflsal ajlfdja lj flajsorweajoifj lsadjflwejoifjlsa lasdfl jlasjfl jlkasfdlewjlfas ",
                NotifyComment = true,
                NotifyPhoto = true,
                NotifySubscription = true
            };
            users.Create(user);
        }

        private static void ExecuteSQLFile(string filePath)
        {
           
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(filePath))
            using (var reader = new StreamReader(stream))
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Executing {0}", filePath));
                string sql= reader.ReadToEnd();
                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=pastexplorer;User Id=postgres;Password=qwe;"))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    using (var command = new NpgsqlCommand(sql, connection, transaction))
                    {
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }

                }
            }
        }
    }
}