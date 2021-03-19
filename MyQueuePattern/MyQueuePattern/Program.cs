using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQueuePattern
{
    class Program
    {
        static void Main(string[] args)
        {
            BestPracticeDAO dao = new BestPracticeDAO();
            var sp_get_all_movies = dao.Run_sp("sp_get_all_movies", new { });

            List<Movie> the_movies = dao.Run_sp<Movie>("sp_get_all_movies", new { });

            Console.WriteLine("from sp generic!");
            the_movies.ForEach(m => Console.WriteLine(m));
        }
    }
}
