using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Chip8
{
	public class Chip8Timer
	{
		static public Chip8Timer _NullInstance = new Chip8Timer();

		public string Name;
		public byte Value;
		//private 
		private DateTime StartDateTime = DateTime.UtcNow;

		private Chip8Timer()
		{
		}

		public Chip8Timer(string Name)
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
