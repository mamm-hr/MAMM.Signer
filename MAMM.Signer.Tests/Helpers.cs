using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Tests;

/// <summary>
/// Pomoćne metode.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Kreira i vraća punu stazu direktorija za izlazne datoteke testiranja koji se zove isto kao testni razred i
    /// nalazi se u <see cref="TestContext.TestRunResultsDirectory"/> direktoriju.
    /// </summary>
    public static string CreateTestRunOutputDirectory( TestContext context )
        => Directory.CreateDirectory( Path.Combine(
            context.TestRunResultsDirectory!,
            context.FullyQualifiedTestClassName!
            ) ).FullName;

    /// <summary>
    /// Vraća istinu ako je istinit samo jedan od dva argumenta.
    /// </summary>
    public static bool EitherOr( bool left, bool right ) => left && !right || !left && right;

    /// <summary>
    /// Dohvaća certifikate po thumbprintu iz korisnikovog spremišta osobnih certifikata.
    /// </summary>
    public static X509Certificate2[] FindCertificate( StoreName storeName, string thumbprint )
    {
        thumbprint = thumbprint.ToUpperInvariant();
        using var store = new X509Store( storeName, StoreLocation.CurrentUser );
        store.Open( OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly );
        try
        {
            var coll = store.Certificates
                .Find( X509FindType.FindByThumbprint, thumbprint, false ) // Pažnja: traži prefiks, a ne točnu vrijednost!
                .Where( cert => string.Equals( cert.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase ) )
                .ToList();
            return [.. coll];
        }
        finally
        {
            store.Close();
        }
    }

    /// <summary>
    /// Učita certifikat iz PFX datoteke u korisničko spremište certifikata. U spremište osobnih certifikata učita
    /// certifikat s privatnim ključem, dok ostale (npr. korijenski, subordinirani) učita bez privatnog ključa.
    /// </summary>
    public static void ImportPfx( string path, string password, StoreName storeName )
    {
        using var certWithPrivateKey = X509CertificateLoader.LoadPkcs12FromFile(path, password
            , StoreName.My == storeName
            ? X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet
            : X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
            );
        using var store = new X509Store( storeName, StoreLocation.CurrentUser );
        store.Open( OpenFlags.ReadWrite );
        if( StoreName.My == storeName )
            store.Add( certWithPrivateKey );
        else
        {
            using var certWithoutPrivateKey = X509CertificateLoader.LoadCertificate( certWithPrivateKey.Export( X509ContentType.Cert ) );
            store.Add( certWithoutPrivateKey );
        }
    }

    /// <summary>
    /// Učita certifikat iz PFX datoteke na način prikladan za privremeno korištenje objekta, tj. vrati samo objekt bez
    /// učitavanja u spremište certifikata.
    /// </summary>
    public static X509Certificate2 LoadPfx( string path, string password )
        => X509CertificateLoader.LoadPkcs12FromFile( path, password, X509KeyStorageFlags.EphemeralKeySet );


    /// <summary>
    /// Obriše certifikat iz svih korisničkih spremišta u kojima se nalazi (osobnih, subordiniranih, korijenskih).
    /// </summary>
    public static void PurgeCertificate( string thumbprint )
    {
        ArgumentNullException.ThrowIfNull( thumbprint );
        thumbprint = thumbprint.ToUpperInvariant();
        RemoveCertificateFromStore( StoreName.My, thumbprint );
        RemoveCertificateFromStore( StoreName.CertificateAuthority, thumbprint );
        RemoveCertificateFromStore( StoreName.Root, thumbprint );
    }

    public static void RemoveCertificateFromStore( StoreName storeName, string thumbprint )
    {
        using var store = new X509Store(storeName, StoreLocation.CurrentUser);
        store.Open( OpenFlags.ReadWrite );
        var matches = store.Certificates.Find( X509FindType.FindByThumbprint, thumbprint, validOnly: false);
        foreach(var cert in matches.Where( c => string.Equals( c.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase ) ))
            store.Remove( cert );
    }
}
