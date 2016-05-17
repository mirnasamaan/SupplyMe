using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.Context;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Data.Objects.SqlClient;
using System.Data.Objects;

namespace DataLayer.Repositories
{
    public class HotelRepository : BaseRepository<Hotel>
    {
        /// <summary>
        /// Get hotel by hotel Id
        /// </summary>
        /// <param name="hotelId">Hotel Id</param>
        /// <returns></returns>
        public Hotel GetHotelById(int hotelId)
        {
            if (Context.Hotels.Any(x => x.HotelId == hotelId))
            {
                return Context.Hotels.FirstOrDefault(x => x.HotelId == hotelId);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get hotel by user name
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <returns></returns>
        public Hotel GetHotelByUserName(string userName)
        {
            if (Context.Hotels.Any(x => x.UserName == userName))
            {
                return Context.Hotels.FirstOrDefault(x => x.UserName == userName);
            }
            else
            {
                return null;
            }
        }


        public void ModifyLastLogin(Hotel hotel)
        {
            Context.Hotels.Attach(hotel);
            hotel.LastLoginDate = DateTime.UtcNow.ToLocalTime();
            Context.SaveChanges();
        }

        /// <summary>
        /// Check if the hotel account exists for login
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public Hotel HotelExists(string userName, string password)
        {
            if (Context.Hotels.Any(x => x.UserName == userName && x.Password == password))
            {
                return Context.Hotels.FirstOrDefault(i => i.UserName == userName && i.Password == password);
            }
            else
            {
                return null;
            }
        }

    }
}
