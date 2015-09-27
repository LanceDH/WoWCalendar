using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;

namespace Calendar.DAL
{
    public class Package
    {

        public static string URL { set; get; }
        string service = "";
        List<KeyValuePair<string, string>> parameters;

        public Package(String service)
        {
            this.service = service;
            parameters = new List<KeyValuePair<string, string>>();
        }

        // Voeg Input Parameters toe
        public void AddParameter(string key, string value)
        {
            parameters.Add(new KeyValuePair<string, string>(key, value));
        }

        public String Send()
        {
            string output = "";
            string param_string = "";

            foreach (KeyValuePair<string, string> dx in parameters)
                param_string += dx.Key + "=" + dx.Value + "&";

            byte[] buffer = Encoding.ASCII.GetBytes(param_string);
            string lex = URL + service + "?" + param_string;


            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(lex);
            WebReq.Method = WebRequestMethods.Http.Post;
            WebReq.ContentType = "application/x-www-form-urlencoded";

            WebReq.ContentLength = buffer.Length;
            using (Stream PostData = WebReq.GetRequestStream())
                PostData.Write(buffer, 0, buffer.Length);

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            using (StreamReader reader = new StreamReader(WebResp.GetResponseStream()))
                output = reader.ReadToEnd();

            return output;

        }
    }

    public class Weather{
        public static int GetTodayTemp()
        {
            Package pAssignment = new Package("");
            pAssignment.AddParameter("q", "antwerpen,be");
            pAssignment.AddParameter("units", "metric");
            JObject jCourses = new JObject();

            jCourses = JObject.Parse(pAssignment.Send());
            JToken jt = jCourses["main"];
            string s = jt["temp"].ToString();

            return (int)Double.Parse(s);
        }
    }

    public class CalendarEvent
    {
        public static List<BLL.CalendarEvent> GetAllBetweenDates(DateTime start, DateTime end)
        {
            List<BLL.CalendarEvent> events = new List<BLL.CalendarEvent>();

            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBCalendar"].ToString());

            SqlCommand comm = new SqlCommand("CalendarEventSelectByDate", con);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("@StartDate", start);
            comm.Parameters.AddWithValue("@EndDate", end);

            SqlDataReader reader;
            try
            {
                con.Open();

                using (reader = comm.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            BLL.CalendarEvent calEvent = new BLL.CalendarEvent((int)reader["Id"]);
                            calEvent.CalendarDate = (DateTime)reader["Date"];
                            calEvent.Title = reader["Title"].ToString();
                            calEvent.Detail = reader["Detail"].ToString();

                            events.Add(calEvent);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                con.Close();
            }

            return events;
        }

        public static int InsertOne(BLL.CalendarEvent calEvent)
        {
            int id = 0;

            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBCalendar"].ToString());

            SqlCommand comm = new SqlCommand("CalendarEventInsertOne", con);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("@Date", calEvent.CalendarDate);
            comm.Parameters.AddWithValue("@Title", calEvent.Title);
            comm.Parameters.AddWithValue("@Detail", calEvent.Detail);
            SqlParameter parId = new SqlParameter("@Id", DbType.Int32);
            parId.Direction = ParameterDirection.Output;
            comm.Parameters.Add(parId);

            try
            {
                con.Open();

                comm.ExecuteNonQuery();
                id = (int)parId.Value;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                con.Close();
            }

            return id;
        }

        public static void DeleteOne(int id)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBCalendar"].ToString());

            SqlCommand comm = new SqlCommand("CalendarEventDeleteOne", con);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("@Id", id);

            try
            {
                con.Open();

                comm.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                con.Close();
            }
        }

        public static Boolean UpdateOne(BLL.CalendarEvent calEvent)
        {
            int rows = 0;

            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBCalendar"].ToString());

            SqlCommand comm = new SqlCommand("CalendarEventUpdateOne", con);
            comm.CommandType = CommandType.StoredProcedure;
            comm.Parameters.AddWithValue("@Id", calEvent.GetId());
            comm.Parameters.AddWithValue("@Date", calEvent.CalendarDate);
            comm.Parameters.AddWithValue("@Title", calEvent.Title);
            comm.Parameters.AddWithValue("@Detail", calEvent.Detail);


            try
            {
                con.Open();

                rows = comm.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            finally
            {
                con.Close();
            }

            return (rows > 0);
        }
    }
}
