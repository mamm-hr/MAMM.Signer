namespace MAMM.Signer.Pkcs;

public class Pkcs7Exception : Exception
{
    public Pkcs7Exception(
          string message
        , Dictionary<string, object> data
        )
        : base( string.Format( message, [.. data.Values.Cast<object>()] ) )
    {
        foreach(var kv in data)
            this.Data.Add( kv.Key, kv.Value );
    }
}

/// <summary>
/// Potpisana poruka mora imati barem jednog potpisnika (degenerirani SignedData tip nije dopušten).
/// </summary>
public class DegenerateSignatureException() : Pkcs7Exception( Resources.DegenerateSignature, [] );

/// <summary>
/// Neispravano stanje sadržaja.
/// </summary>
/// <param name="expected">
///     Očekivano stanje sadržaja.</param>
/// <param name="actual">
///     Stvarno stanje sadržaja.</param>
public class InvalidMessageStateException(
      string expected
    , string actual
    )
    : InvalidOperationException( string.Format( Resources.InvalidContentState, expected, actual ) );

/// <summary>
/// Niti jedan od primatelja poruke nije identificiran podržanim načinom identifikacije.
/// </summary>
/// <param name="count">
///     Ukupni broj primatelja poruke.</param>
public class NoIdentifiableRecipientsException(
      int count
    )
    : Pkcs7Exception(
          Resources.NoIdentifiableRecipients
        , new()
        {
            [nameof( count )] = count,
        } );

/// <summary>
/// Niti jedan od potpisnika poruke nije identificiran podržanim načinom identifikacije.
/// </summary>
/// <param name="count">
///     Ukupni broj potpisnika poruke.</param>
public class NoIdentifiableSignersException(
      int count
    )
    : Pkcs7Exception(
          Resources.NoIdentifiableSigners
        , new()
        {
            [nameof( count )] = count,
        } );

/// <summary>
/// Poruka mora imati točno jednog primatelja. Nije podržano više primatelja, niti slučaj bez primatelja.
/// </summary>
/// <param name="count">
///     Stvarni broj primatelja poruke.</param>
public class SingleRecipientRequiredException(
      int count
    )
    : Pkcs7Exception(
          Resources.SingleRecipientRequired
        , new()
        {
            [nameof( count )] = count,
        } );

/// <summary>
/// Poruka mora imati točno jednog potpisnika. Nije podržano više potpisnika, niti slučaj bez potpisnika.
/// </summary>
/// <param name="count">
///     Stvarni broj potpisnika poruke.</param>
public class SingleSignerRequiredException(
      int count
    )
    : Pkcs7Exception(
          Resources.SingleSignerRequired
        , new()
        {
            [nameof( count )] = count,
        } );

/// <summary>
/// Poruka nije adresirana na traženog primatelja.
/// </summary>
/// <param name="issuer">
///     Izdavač certifikata traženog primatelja.</param>
/// <param name="serialNumber">
///     Serijski broj certifikata traženog primatelja.</param>
public class UnknownRecipientException(
      string issuer
    , string serialNumber
    )
    : Pkcs7Exception(
          Resources.UnknownRecipient
        , new()
        {
            [nameof( issuer )] = issuer,
            [nameof( serialNumber )] = serialNumber,
        } );

/// <summary>
/// Sadržaj CMS podatka je sintantkički raspoznatljiv, ali nije jednog od podržanih tipova.
/// </summary>
/// <param name="oid">
///     Identifikator tipa CMS podatka.</param>
public class UnsupportedCmsContentTypeException(
      string oid
    )
    : Pkcs7Exception(
          Resources.UnsupportedCmsContentType
        , new()
        {
            [nameof( oid )] = oid,
        } );

public class PrivateKeyMissingException(
      string issuer
    , string serialNumber
    )
    : Pkcs7Exception(
          Resources.PrivateKeyMissing
        , new()
        {
            [nameof( issuer )] = issuer,
            [nameof( serialNumber )] = serialNumber,
        } );
