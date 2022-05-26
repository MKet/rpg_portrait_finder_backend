using JWT.Algorithms;
using Azure.Security.KeyVault.Keys;
using System.Security.Cryptography;

namespace AuthenticationService.Services;

public class AzureRSAJwtAlgorithm : IJwtAlgorithm, IAsymmetricAlgorithm
{
    KeyClient _keyClient;
    public AzureRSAJwtAlgorithm(KeyClient keyClient)
    {
        _keyClient = keyClient;
    }

    public string Name => "RSA2048";

    public HashAlgorithmName HashAlgorithmName => new(Name);

    public virtual byte[] Sign(byte[] key, byte[] bytesToSign)
    {
        return _keyClient.GetCryptographyClient("jwt-key").Sign(Name, bytesToSign).Signature;
    }

    public virtual bool Verify(byte[] bytesToSign, byte[] signature)
    {
        return _keyClient.GetCryptographyClient("jwt-key").Verify(Name, bytesToSign, signature).IsValid;
    }
}
