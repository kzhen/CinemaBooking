﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowService.Messages
{
	public class DisambiguateMovieBookingFinished
	{
		public string PhoneNumber { get; set; }
		public string MovieId { get; set; }
	}
}
