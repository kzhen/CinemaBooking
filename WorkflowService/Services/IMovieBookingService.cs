using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorkflowService.Services
{
	public interface IMovieBookingService
	{
		List<string> GetListOfCinemas();
		string GetCinema(string data);
		string GetMovie(string data);
		void SendCinemaSelection(string phoneNumber, List<string> cinemas);
		void SendMovieSelection(string phoneNumber, string cinemaKey);
		void SendUnknownResponse(string phoneNumber);
		void SendMovieSlots(string phoneNumber, string cinemaKey, string movieKey);
	}
}
