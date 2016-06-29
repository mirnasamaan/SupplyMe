﻿using DataLayer.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace DataLayer.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private supplyme_dbEntities _entities = new supplyme_dbEntities();
        public supplyme_dbEntities Context
        {

            get { return _entities; }
            set { _entities = value; }
        }

        public virtual IQueryable<T> GetAll()
        {

            IQueryable<T> query = _entities.Set<T>();
            return query;
        }

        public IQueryable<T> FindBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {

            IQueryable<T> query = _entities.Set<T>().Where(predicate);
            return query;
        }

        public virtual void Add(T entity)
        {
            _entities.Set<T>().Add(entity);
            _entities.SaveChanges();
        }

        public virtual void Delete(T entity)
        {
            _entities.Set<T>().Remove(entity);
            _entities.SaveChanges();
        }

        public virtual void Edit(T entity)
        {
            _entities.Entry(entity).State = System.Data.EntityState.Modified;
            _entities.SaveChanges();
        }
    }
}
