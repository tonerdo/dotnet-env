using Xunit;

// Make one collection per class, since multiple classes now play with environment variables
// At the same time!
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass)]
