
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;

    public interface ISqlRepository<TEntity>
    {

    }

    public class SqlRepository<T> : ISqlRepository<T> where T : class
    {
        public static List<T> GetAll()
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>();
                return query.ToList();
            }
        }

        public static List<T> GetAll(SqlContext databaseContext)
        {
            var query = databaseContext.Set<T>();
            return query.ToList();
        }

        public static List<T> GetList(Expression<Func<T, bool>> predicate)
        {
            using (var databaseContext = new SqlContext())
            {
                return databaseContext.Set<T>().Where(predicate).ToList();
            }
        }

        public static List<T> GetList(Expression<Func<T, bool>> predicate, Expression<Func<T, byte>> orderByPredicate)
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>().Where(predicate).OrderBy(orderByPredicate);
                return query.ToList();
            }
        }

        public static List<T> GetList(Expression<Func<T, bool>> predicate, Expression<Func<T, int?>> orderByPredicate)
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>().Where(predicate).OrderBy(orderByPredicate);
                return query.ToList();
            }
        }

        
        public static List<T> GetList(Expression<Func<T, bool>> predicate, Expression<Func<T, decimal>> orderByPredicate, int count)
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>().Where(predicate).OrderByDescending(orderByPredicate).Take(count);
                return query.ToList();
            }
        }

        public static IEnumerable<T> GetList(Expression<Func<T, bool>> predicate, SqlContext databaseContext)
        {
            var query = databaseContext.Set<T>().Where(predicate);
            return query;
        }

        public static T Get(Expression<Func<T, bool>> predicate)
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>().FirstOrDefault(predicate);
                return query;
            }
        }
        /// <summary>
        /// The same as Get but with a second predicate for orderByDescending
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="predicate2"></param>
        /// <returns></returns>
        public static T GetDesc(Expression<Func<T, bool>> predicate, Expression<Func<T, int?>> predicate2)//todo test this function for task 1134
        {
            using (var databaseContext = new SqlContext())
            {
                var query = databaseContext.Set<T>().Where(predicate);
                var query2 = query.OrderByDescending(predicate2).FirstOrDefault();
                return query2;
            }
        }

        public static T Get(Expression<Func<T, bool>> predicate, SqlContext databaseContext)
        {
            var query = databaseContext.Set<T>().FirstOrDefault(predicate);
            return query;
        }

        public static void Add(T entity)
        {
            using (var databaseContext = new SqlContext())
            {
                databaseContext.Set<T>().Add(entity);
                databaseContext.SaveChanges();
            }
        }

        public static void AddAll(List<T> entity)
        {
            using (var databaseContext = new SqlContext())
            {
                databaseContext.Set<T>().AddRange(entity);
                databaseContext.SaveChanges();
            }
        }

        public static void Add(T entity, SqlContext databaseContext)
        {
            databaseContext.Set<T>().Add(entity);
        }

        public static void Remove(T entity, SqlContext databaseContext)
        {
            databaseContext.Set<T>().Remove(entity);
        }
    }
