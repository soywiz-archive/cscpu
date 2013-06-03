using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Chip8
{
	public class Timer
	{
		public string Name;
		public byte Value;
		//private 
		private DateTime StartDateTime = DateTime.UtcNow;

		public Timer(string Name)
		{
			this.Name = Name;
		}

		public void Update()
		{
			// TODO: Improve timer.
			if (Value > 0)
			{
				var CurrentTime = DateTime.UtcNow;
				var ElapsedTime = (CurrentTime - StartDateTime);
				if (ElapsedTime.TotalMilliseconds >= (1000 / 60))
				{
					Value--;
					StartDateTime = CurrentTime;
				}
			}
			//Console.WriteLine("Timer({0}): {1}", Name, Value);
		}
	}
}
