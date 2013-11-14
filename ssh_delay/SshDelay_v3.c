#include <stdio.h>
#include <time.h> 
#include <stdlib.h>
#include <stdint.h>

/*
Returns the current TSC (timestamp count). Works on x86 and x86-64.

The time each TSC tick takes is the inverse of the CPU speed. A 2ghz CPU would
take 1/2ghz (0.5ns) per tick.
*/
__inline__ uint64_t rdtsc()
{
   uint32_t lo, hi;
   __asm__ __volatile__ ("rdtsc" : "=a" (lo), "=d" (hi));
   return (uint64_t)hi << 32 | lo;
}

//used by timeprecise() to set what unit of time it will return
const int NANOSECONDS = 9;
const int MICROSECONDS = 6;
const int MILLISECONDS = 3;
const int SECONDS = 0;

/*
Returns CPU speed (hz). Sleeps for 1 second the first time it is called.
*/
static unsigned int CPU_hz = 0;
static struct timespec interval, remainder;

unsigned int hz()
{
   if(CPU_hz != 0){
      return CPU_hz;
   }
   else
   {
	interval.tv_sec = 1; 
	interval.tv_nsec = 0; 
	   
    uint64_t start = rdtsc();
    nanosleep(&interval, &remainder);
    uint64_t end = rdtsc();
    return CPU_hz = end - start;
   }
}

int main(int argc, char *argv[])
{
	//We need to create an array of these timespec intervals
 	double* intervals;
	double time = 1.0;
	int count=0, inputSize=100;
	count = 0;
	int seconds = 0;
	long nanoseconds = 0.0;
	uint64_t start;
	uint64_t end;	
		
	//Read in the count and malloc an array of the correct size	
	scanf("%d", &inputSize);
	intervals = (double*)(malloc(sizeof(double) * inputSize));
	
	//Read in each of the delays and add them to the intervals array
	while(count < inputSize)
	{		
		scanf("%lf", &time);	 
				
		intervals[count] = time; 		
		
		count = count +1;
		
		//printf("seconds: %f\n", intervals[count]);
	}
	
	//Lets add a 60 second sleep before we start sending packets. This lets  
	// us create the session before we start sending character inputSize
	interval.tv_sec = 60; 
	interval.tv_nsec = 0; 
	nanosleep(&interval, &remainder);	
	
	count = 0;
	
	//printf("%d\n", hz());
	
	while(count < inputSize)
	{	
		start = rdtsc();
		
		if(intervals[count] > 1.0 )
		{
			interval.tv_sec = (int)intervals[count];			
			nanosleep(&interval, &remainder);	
		}		
		
		//printf("%f\n", intervals[count]);
		
		while(((double)(rdtsc() - start)/hz()) < intervals[count] )
		{					
		}

		printf("a");
		fflush(stdout);
		count = count +1;	
	}
	
    return 0;
}

