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
            /*using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                var model = session.CreateQuery("from PhotoModel where Date =:date").SetParameter("date", new DateTime(2012, 5, 1, 22, 33, 5)).UniqueResult<PhotoModel>();
                if(model==null)
                    System.Diagnostics.Debug.WriteLine("MODEL ==========NULLLL");
                else
                    System.Diagnostics.Debug.WriteLine(model.Id + " " + model.Date.ToString());
            }*/
            ExecuteSQLFile("PhotoHistory.sql.create_tables.sql");
            CreateUsers();
            CreateCategories();
            CreateAlbums();
        }

        private static void CreateAlbums()
        {
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername("Pierogu");
            AlbumRepository albums = new AlbumRepository();
            
            CategoryModel category=null;
            using (var session = SessionProvider.SessionFactory.OpenSession())
            {
                category=session.CreateQuery("from CategoryModel where Name =:name").SetParameter("name","People").UniqueResult<CategoryModel>();
            }

            AlbumModel album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Opis super fajnego albumu nr 1. LOL. ROFTEWRFS saflksjalfrwjlfj alksfdjlasjfl",
                Name = "Super fajny album nr.1",
                Password = "",
                Public = true,
                Rating = 10,
                User = user,
                Views = 1234
            };

            LinkedList<PhotoModels> list = new LinkedList<PhotoModels>();
            PhotoModels photo = new PhotoModels()
            {
                Album=album, Date= DateTime.Today, Description="Słit focia nr1 album 1" 
            };
            list.AddLast(photo);
            photo = new PhotoModels()
            {
                Album = album,
                Date = new DateTime(2012,5,1,22,33,5),
                Description = "Słit focia nr2 album 1"
            };
            list.AddLast(photo);
            photo = new PhotoModels()
            {
                Album = album,
                Date = new DateTime(2012, 4,30 , 1, 8, 59),
                Description = "Słit focia nr3 album 1"
            };
            list.AddLast(photo);
            

            albums.Create(album);

            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Opis super fajnego albumu nr 2. LOL. weratsyhdjufkilokhjgfdscxsZcvnmjkiol8i7u6tgrefcdsvfbgnh",
                Name = "Super fajny album nr.2!!!!",
                Password = "",
                Public = true,
                Rating = 11,
                User = user,
                Views = 12334
            };
            albums.Create(album);

            photo = new PhotoModels()
            {
                Album = album,
                Date = DateTime.Today,
                Description = "Słit focia nr1 album 2"
            };
            list.AddLast(photo);
            photo = new PhotoModels()
            {
                Album = album,
                Date = new DateTime(2012,1,2,22,11,3),
                Description = "Słit focia nr2 album 2"
            };
            list.AddLast(photo);

            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Opis super fajnego albumu nr 3. LOL.fsaerfgt,aem jlejarklfnjgskjafn kqnawfn kasnfk jnbwkanf kgnmwaklenb kjnawk bnkjwan knfskjhfkjna kjnfwkaj nkwanef kjnkja fnkj. Ale sie rozpisalem.",
                Name = "Super fajny album nr.3 LOL",
                Password = "",
                Public = true,
                Rating = 1,
                User = user,
                Views = 1
            };
            albums.Create(album);

            photo = new PhotoModels()
            {
                Album = album,
                Date = DateTime.Today,
                Description = "Słit focia nr1 album 3"
            };
            list.AddLast(photo);


            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Opis super fajnego albumu nr 4. heh",
                Name = "Super fajny album nr.4444444444444444",
                Password = "",
                Public = true,
                Rating = 44,
                User = user,
                Views = 4444
            };
            albums.Create(album);

            using(var session= SessionProvider.SessionFactory.OpenSession())
            using (var trans = session.BeginTransaction())
            {
                foreach (PhotoModels p in list)
                    session.Save(p);
                trans.Commit();
            }
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