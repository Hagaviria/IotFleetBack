namespace Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordToVerify, string passwordHash);
}
