# restore
C# Library for synchronizing between two CRUD data sources. The primary objective of writing this library is to assist in the synchronization of a REST api and a local Sqlite database in an app. 
So far the essential concepts of this library is that synchronization is nothing more than equalizing data between two data sources. The implementations of these data sources and business rules 
in the end should determine how the synchronization is executed. Given this the actual implementation is expected to be fairly thin.

## Required scenario support
For the paticular application of this library it needs to support the following scenarios
* One way synchronization from a REST api to a local database using lists on both ends.
* Two way synchronization for changes on single items.
* Two way batch synchronization for changes on lists of items.

# Huh, a specific PCL Profile?
The library has a specific PCL profile because that is the profile of the project it is being used in currently. Primary objective is getting this code battle tested in a production scenario.
Only once that is done I have time to make this a proper library with full PCL support.

# Concepts
## Matching
This is an optional first step in the synchronization of lists of data in specific. If decisions need to be made based on the data of both items this step creates a match between two data endpoints.
This match can then facilitate the Change Resolution step.

## Change Resolution
Given a certain item there should be a piece of custom business logic that needs to be configured which determines the actual action that needs to be taken. Potential conflict resolution could be part
of this step.

## Dispatching (Change Execution)
In order to facilitate both single item updates as well as batch updates the execution of the change is separated from the previous decision step.

## Logging / feedback
The design is to simple have a pipeline of enumerables (or observables in the future) it should be possible to simply hook up in the pipeline in certain places to introduce aspects such as 
logging.

**What about that Rx?**
I have given Reactive extensions a try. The initial experiment which is still in a separate namespace was functioning to that point. In separate spike project I failed to fully wrap my head around
some of Rx's constructs and get all scenario's I'd wanted to functioning. Because I needed to move on and also still need to test some of my design ideas I decided to abandon Rx. To an extent I 
attempt to write to code in such a way that a port to Rx later will be somewhat easier. Since conceptually the steps are similar the data that is exchanged between these steps is also similar. This 
for one should lead to a certain amount of code reuse.
