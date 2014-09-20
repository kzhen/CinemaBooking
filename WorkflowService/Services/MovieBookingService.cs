using Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Services
{
	internal sealed class MovieBookingService : IMovieBookingService
	{
		private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private Infrastructure.IBus bus;
		private List<string> cinemaList;
		private List<string> movieList;

		public MovieBookingService(Infrastructure.IBus bus)
		{
			this.bus = bus;

			this.cinemaList = new List<string>() { "Streatham", "Brixton", "West Norwood" };
			this.movieList = new List<string>() { "Captain America", "Thor", "The Hunger Games", "The Hunt for Red October" };
		}

		public List<string> GetListOfCinemas()
		{
			return this.cinemaList;
		}
		public string GetCinema(string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				return null;
			}

			int idx = ALPHABET.IndexOf(data.ToUpper());

			return (cinemaList.Count > idx) && (idx >= 0) ? cinemaList[idx] : null;
		}

		public string GetMovie(string data)
		{
			int idx = ALPHABET.IndexOf(data.ToUpper());

			return (movieList.Count > idx) && (idx >= 0) ? movieList[idx] : null;
		}

		public void SendCinemaSelection(string phoneNumber, List<string> cineams)
		{
			string list = string.Join(", ", cineams.Select((i, idx) => { return ALPHABET[idx] + " " + i; }));

			string body = "Please select a cinema: " + list;
			bus.Publish(new SendSms() { PhoneNumber = phoneNumber, Body = body });
		}

		public void SendMovieSelection(string phoneNumber, string cinemaKey)
		{
			string list = string.Join(", ", movieList.Select((i, idx) => { return ALPHABET[idx] + " " + i; }));
			string body = "Please select a movie: " + list;

			bus.Publish(new SendSms() { PhoneNumber = phoneNumber, Body = body });
		}

		public void SendMovieSlots(string phoneNumber, string cinemaKey, string movieKey)
		{
			bus.Publish(new SendSms() { PhoneNumber = phoneNumber, Body = "SendMovieSlots" });
		}

    public void SendConfirmation(string phoneNumber)
    {
			Random r = new Random();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 10; i++)
			{
				sb.Append(ALPHABET[r.Next(0, 25)]);
			}

      bus.Publish(new SendSms() { PhoneNumber = phoneNumber, Body = "You confirmation code is: " + sb.ToString() });
    }
  }
}
