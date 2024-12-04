using Dapper;
using KevDataAccess.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace KevDataAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(Path.Combine("Configurations", "appsettings.Development.json"), optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string not found");
            }
            else
            {
                Console.WriteLine("Connection String: " + connectionString);
            }

            //Discomment to execute the method
            using (var connection = new SqlConnection(connectionString))
            {   
                Console.WriteLine("Connected");
                //CreateCategory(connection);
                //CreateManysCategory(connection);
                //ListCategories(connection);
                //UpdateCategory(connection);
                //OneToOne(connection);
                //OneToMany(connection);
                //QueryMultiple(connection);
                //SelectIn(connection);
                //Like(connection, "api");
                //Transaction(connection);
            }
        }
        static void CreateCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = (Guid.NewGuid());
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "AWS Cloud";
            category.Order = 8;
            category.Description = "Categoria destinada a serviços AWS";
            category.Featured = false;

            var insertSql = @"INSERT INTO
                                [Category]
                            VALUES(
                                @Id,
                                @Title,
                                @Url,
                                @Summary,
                                @Order,
                                @Description,
                                @Featured)";

            Console.WriteLine("Connected");
            var rows = connection.Execute(insertSql, new
            {
                category.Id,
                category.Title,
                category.Url,
                category.Summary,
                category.Order,
                category.Description,
                category.Featured
            });
            Console.WriteLine($"{rows} linhas afetadas");
        }
        static void CreateManysCategory(SqlConnection connection)
        {
            var category = new Category();
            category.Id = (Guid.NewGuid());
            category.Title = "Amazon AWS";
            category.Url = "amazon";
            category.Summary = "AWS Cloud";
            category.Order = 8;
            category.Description = "Categoria destinada a serviços AWS";
            category.Featured = false;

            var category2 = new Category();
            category2.Id = (Guid.NewGuid());
            category2.Title = "Many";
            category2.Url = "many";
            category2.Summary = "categoria nova";
            category2.Order = 8;
            category2.Description = "Teste categoria manys";
            category2.Featured = false;

            var insertSql = @"INSERT INTO
                                [Category]
                            VALUES(
                                @Id,
                                @Title,
                                @Url,
                                @Summary,
                                @Order,
                                @Description,
                                @Featured)";

            Console.WriteLine("Connected");
            var rows = connection.Execute(insertSql, new[]
            {
                new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured
                },
                new
                {
                    category2.Id,
                    category2.Title,
                    category2.Url,
                    category2.Summary,
                    category2.Order,
                    category2.Description,
                    category2.Featured
                }
            });
            Console.WriteLine($"{rows} linhas afetadas");
        }
        static void ListCategories(SqlConnection connection)
        {
            var categories = connection.Query<Category>("SELECT [Id], [Title] FROM [Category]");
            foreach (var item in categories)
            {
                Console.WriteLine($"{item.Id} - {item.Title}");
            }
        }
        static void UpdateCategory(SqlConnection connection)
        {
            var updateQuery = "UPDATE [Category] SET [Title]=@title WHERE [Id]=@id";
            var rows = connection.Execute(updateQuery, new
            {
                id = new Guid("a906431b-f578-47c2-9c72-49b53f328bc7"),
                title = "Amazon AWS 2024"
            });
        }
        static void OneToOne(SqlConnection connection)
        {
            var sql = @"
                SELECT 
                    * 
                FROM 
                    [CareerItem]
                INNER JOIN
                    [Course] ON [CareerItem].[CourseId] = [Course].[Id]";

            var items = connection.Query<CareerItem, Course, CareerItem>(

                  sql,
                  (careerItem, course) =>
                  {
                    careerItem.Course = course;
                    return careerItem;
                  }, splitOn: "Id");

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Title} - {item.Course.Title}");
            }
        }
        static void OneToMany(SqlConnection connection)
        {
             var sql = @"
                USE balta;
                SELECT 
                    [Career].[Id],
                    [Career].[Title],
                    [CareerItem].[CareerId],
                    [CareerItem].[Title]
                FROM 
                    [Career] 
                INNER JOIN 
                    [CareerItem] ON [CareerItem].[CareerId] = [Career].[Id]
                ORDER BY
                    [Career].[Title]";

            var careers = new List<Career>();
            var items = connection.Query<Career, CareerItem, Career>(
                sql,
                (career, item) =>
                {
                    var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
                    if (car == null)
                    {
                        car = career;
                        car.Items.Add(item);
                        careers.Add(car);
                    }
                    else
                    {
                        car.Items.Add(item);
                    }
                    return career;
                }, splitOn: "CareerId");

            foreach (var career in careers)
            {
                Console.WriteLine($"{career.Title}");
                foreach (var item in career.Items)
                {
                    Console.WriteLine($" - {item.Title}");
                }
            }
        }
        static void QueryMultiple(SqlConnection connection)
        {
            var query = "SELECT * FROM [Category]; SELECT * FROM [Course]";

            using (var multi = connection.QueryMultiple(query))
            {
                var categories = multi.Read<Category>();
                var course = multi.Read<Course>();

                Console.WriteLine("CATEGORIES");
                foreach (var item in categories)
                {
                    Console.WriteLine(item.Title);
                }

                Console.WriteLine("COURSE");
                foreach (var item in course)
                {
                    Console.WriteLine(item.Title);
                }
            }
        }
        static void SelectIn(SqlConnection connection)
        {
            var query = @"SELECT * FROM Career Where [Id] IN @Id";

            var items = connection.Query<Career>(query, new
            {
                Id = new[]
                {
                    "01ae8a85-b4e8-4194-a0f1-1c6190af54cb",
                    "4327ac7e-963b-4893-9f31-9a3b28a4e72b"
                }
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }
 
        static void Like(SqlConnection connection, string term)
        {
            var query = @"SELECT * FROM [Course] Where [Title] LIKE @exp";

            var items = connection.Query<Course>(query, new
            {
                exp = $"%{term}%"
            });

            foreach (var item in items)
            {
                Console.WriteLine(item.Title);
            }
        }

        static void Transaction(SqlConnection connection)
        {
            var category = new Category();
            category.Id = (Guid.NewGuid());
            category.Title = "Categoria transaction2";
            category.Url = "amazon";
            category.Summary = "AWS Cloud";
            category.Order = 8;
            category.Description = "Categoria destinada a serviços AWS";
            category.Featured = false;

            var insertSql = @"INSERT INTO
                                [Category]
                            VALUES(
                                @Id,
                                @Title,
                                @Url,
                                @Summary,
                                @Order,
                                @Description,
                                @Featured)";

             connection.Open();
            using(var transaction = connection.BeginTransaction())
            {
                var rows = connection.Execute(insertSql, new
                {
                    category.Id,
                    category.Title,
                    category.Url,
                    category.Summary,
                    category.Order,
                    category.Description,
                    category.Featured
                }, transaction);

                transaction.Commit();
                Console.WriteLine($"{rows} linhas afetadas");
            }
        }
    }
}