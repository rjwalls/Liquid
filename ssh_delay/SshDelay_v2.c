#include <stdio.h>
#include <time.h> 
#include <stdlib.h>

int main(int argc, char *argv[])
{
	//We need to create an array of these timespec intervals
	struct timespec interval, remainder;
 	struct timespec* intervals;
	double time = 1.0;
	int count=0, inputSize=100;
	count = 0;
	int seconds = 0;
	long nanoseconds = 0.0;
	
		
	//Read in the count and malloc an array of the correct size	
	scanf("%d", &inputSize);
	intervals = (struct timespec*)(malloc(sizeof(struct timespec) * inputSize));
	
	//Read in each of the delays and add them to the intervals array
	while(count < inputSize)
	{		
		scanf("%lf", &time);
		count = count +1; 
		
		seconds = (int)time;
		nanoseconds = (time - (double)seconds) * 1000000000;
				
		intervals[count].tv_sec = seconds; 
		intervals[count].tv_nsec = nanoseconds; 
		
		//printf("seconds: %d\n", intervals[count].tv_sec);
		//printf("nanoseconds: %d\n", intervals[count].tv_nsec);
	}
	
	//Lets add a 60 second sleep before we start sending packets. This lets  
	// us create the session before we start sending character inputSize
	interval.tv_sec = 60; 
	interval.tv_nsec = 0; 
	nanosleep(&interval, &remainder);	
	
	count = 0;
	
	while(count < inputSize)
	{		
		printf("a");
		fflush(stdout);
		count = count +1; 
		
		nanosleep(&intervals[count], &remainder);		
	}

	//free(intervals);
	
    return 0;
}

