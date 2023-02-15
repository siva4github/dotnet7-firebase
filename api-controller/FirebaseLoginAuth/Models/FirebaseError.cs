namespace FirebaseLoginAuth.Models;

public class FirebaseError
{
    public Error Error { get; set; } = default!; 
}

public class Error
{
    public int Code { get; set; }
    public string Message { get; set; } = default!;
    public List<Error> Errors { get; set; } = default!;
}
