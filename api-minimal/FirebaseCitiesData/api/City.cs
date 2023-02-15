using Google.Cloud.Firestore;

namespace api;

[FirestoreData]
public record City(
    [property: FirestoreProperty] string Name,
    [property: FirestoreProperty] string State
)
{
    public City() : this("", "") { }
}