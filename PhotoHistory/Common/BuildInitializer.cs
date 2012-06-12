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
			  //ExecuteSQLFile( "PhotoHistory.sql.create_tables.sql" );
			  //CreateUsers();
			  //CreateCategories();
			  //CreateAlbums();
        }

        private static void CreateAlbums()
        {
            UserRepository users = new UserRepository();
            UserModel user = users.GetByUsername("Klocu");
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
                Description = "Jak zmieniałem się w czasie",
                Name = "Moja twarz",
                Public = true,
                Rating = 0,
                User = user,
                Views = 1234
            };
            albums.Create(album);
            

            LinkedList<PhotoModel> list = new LinkedList<PhotoModel>();
            PhotoModel photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 1, 1, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/photo_2012051022444645.jpg" 
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011,4,30,22,33,5),
                Description = "Oto ja",
                Path = "/Static/photos/photo_2012051022450267.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2012, 2,28 , 1, 8, 59),
                Description = "Oto ja",
                Path = "/Static/photos/photo_2012051022452109.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 1, 8, 1, 8, 59),
                Description = "Oto ja",
                Path = "/Static/photos/20110108.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 1, 15, 1, 8, 59),
                Description = "Oto ja",
                Path = "/Static/photos/20110115.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 1, 22, 1, 8, 59),
                Description = "Oto ja",
                Path = "/Static/photos/20110122.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 1, 29, 1, 8, 59),
                Description = "Oto ja",
                Path = "/Static/photos/20110129.jpg"
            };
            list.AddLast(photo);
            

            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "",
                Name = "Widok za moin oknem",
                Public = true,
                Rating = 0,
                User = user,
                Views = 2
            };
            albums.Create(album);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2011-12-29 06.48.45.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2012-04-30 18.07.20.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2012-04-30 18.07.35.jpg"
            };
            list.AddLast(photo);


            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Zmieniający się rynek",
                Name = "Zmieniający się rynek",
                Public = true,
                Rating = 0,
                User = user,
                Views = 111
            };
            albums.Create(album);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2011-12-29 06.48.45.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2012-04-30 18.07.20.jpg"
            };
            list.AddLast(photo);
            photo = new PhotoModel()
            {
                Album = album,
                Date = new DateTime(2011, 4, 30, 22, 33, 5),
                Description = "Oto ja",
                Path = "/Static/photos/2012-04-30 18.07.35.jpg"
            };
            list.AddLast(photo);


            /*
            album = new AlbumModel()
            {
                Category = category,
                CommentsAllow = true,
                CommentsAuth = false,
                Description = "Jak zmieniałem się w czasie",
                Name = "Moja twarz",
                Public = true,
                Rating = 0,
                User = user,
                Views = 1234
            };
            */

            using(var session= SessionProvider.SessionFactory.OpenSession())
            using (var trans = session.BeginTransaction())
            {
                foreach (PhotoModel p in list)
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
                About = "Lubię placki.",
                NotifyComment = false,
                NotifyPhoto = false,
                NotifySubscription = false
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