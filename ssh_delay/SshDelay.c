#include <stdio.h>
#include <time.h> 

int main(int argc, char *argv[])
{
	struct timespec interval, remainder; 

	
	double time = 1.0;
	int count, input=100;
	count = 0;
	int seconds = 0;
	long nanoseconds = 0.0;
	
	scanf("%d", &input);

	//Lets add a 60 second sleep before we start sending packets. This lets  
	// us create the session before we start sending character input
	interval.tv_sec = 60; 
	interval.tv_nsec = 0; 
	nanosleep(&interval, &remainder);	
	
	while(count < input)
	{		
		printf("a");
		fflush(stdout);
		scanf("%lf", &time);
		count = count +1; 
		
		seconds = (int)time;
		nanoseconds = (time - (double)seconds) * 1000000000;
		
		//printf("Read Value: %lf\n", time);
		//printf("seconds: %d\n", seconds);
		//printf("fractions: %lf\n", time - (double)seconds );		
		//printf("nanoseconds: %d\n", nanoseconds);
		
		interval.tv_sec = seconds; 
		interval.tv_nsec = nanoseconds; 
		nanosleep(&interval, &remainder);		
	}

    return 0;
}

