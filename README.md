# Active State Machine Example
Example Implementation of the Active State Machine pattern in .NET. 

An Active State Machine is a state machine that performs transitions on a different thread as the code calling the state machine. 
This allows for multiple threads calling it via a synchronization queue. 

## Implementation Notes
* This implementation takes use of the [Task Parallel Library](http://msdn.microsoft.com/en-us/library/dd460717.aspx) (TPL) to allow input from multiple threads. 
* The state machine handles messages internally using [Reactive Extensions](https://rx.codeplex.com/) (Rx). 
* [Fody](https://github.com/Fody/Fody) is used for Aspects
* [NLog](http://nlog-project.org/) is used for logging

## References
1. [How to: Implement a Producer-Consumer Dataflow Pattern](http://msdn.microsoft.com/en-us/library/hh228601%28v=vs.110%29.aspx)