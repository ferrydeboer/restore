# restore
C# Library for synchronizing between to CRUD data sources. The primary objective of writing this library if to assist in the synchronization of a REST api and a local Sqlite database in an app. 
So far the essential concepts of this library is that synchronization is nothing more than equalizing data between two data sources. The implementations of these data sources and business rules 
in the end should determine how the synchronization is executed. Given this the actualy implementation is expected to be fairly thin.

Given the erratic nature of requests to webservices I decided that I should give Reactive Extensions a go. It's a first attempt in using the technique. So there may very well be constructs that
make clearly I don't grasp the full possibilities of Rx yet.

## Required scenario support
For the paticular application of this library it needs to support the following scenarios
* One way synchronization from a REST api to a local database using lists on both ends.
* Two way synchronization for changes on single items.
* Two way batch synchronization for changes on lists of items.